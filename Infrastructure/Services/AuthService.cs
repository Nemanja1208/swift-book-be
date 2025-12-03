using Application.Auth.Dtos;
using Application.Common.Interfaces;
using Domain.Models.Common;
using Domain.Models.Users;
using Infrastructure.Configuration;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<OperationResult<AuthResponseDto>> RegisterAsync(RegisterUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return OperationResult<AuthResponseDto>.Failure("Username already exists.");

        CreatePasswordHash(dto.Password, out byte[] hash, out byte[] salt);

        var refresh = GenerateRefreshToken();

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RefreshToken = refresh.Token,
            RefreshTokenExpiresAt = refresh.ExpiresAt
        };

        // Assign default role
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User")
                          ?? new Role { Name = "User" };

        user.Roles.Add(new UserRole { Role = defaultRole });

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwt(user);
        return OperationResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            User = MapUser(user)
        });
    }

    public async Task<OperationResult<AuthResponseDto>> LoginAsync(LoginUserDto dto)
    {
        var user = await _context.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == dto.Username);

        if (user is null || !VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            return OperationResult<AuthResponseDto>.Failure("Invalid username or password.");

        var token = GenerateJwt(user);

        var refresh = GenerateRefreshToken();
        user.RefreshToken = refresh.Token;
        user.RefreshTokenExpiresAt = refresh.ExpiresAt;

        await _context.SaveChangesAsync();

        return OperationResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            User = MapUser(user)
        });
    }

    public async Task<OperationResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest dto)
    {
        var user = await _context.Users
            .Include(u => u.Roles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

        if (user is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return OperationResult<AuthResponseDto>.Failure("Invalid or expired refresh token.");

        // ✅ Generate new tokens
        var token = GenerateJwt(user);
        var newRefresh = GenerateRefreshToken();

        user.RefreshToken = newRefresh.Token;
        user.RefreshTokenExpiresAt = newRefresh.ExpiresAt;

        await _context.SaveChangesAsync();

        return OperationResult<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            User = MapUser(user)
        });
    }

    private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return hash.SequenceEqual(computedHash);
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
        };

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r.Role.Name)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserDtoResponse MapUser(User user)
    {
        return new UserDtoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = user.Roles.Select(r => r.Role.Name).ToList()
        };
    }

    private (string Token, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        return (token, DateTime.UtcNow.AddDays(7)); // expires in 7 days
    }
}
