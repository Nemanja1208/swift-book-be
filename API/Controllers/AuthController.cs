using Application.Auth.Dtos;
using Application.Common.Interfaces;
using Domain.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<OperationResult<AuthResponseDto>>> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            SetRefreshTokenCookie(result.Data!.RefreshToken);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<OperationResult<AuthResponseDto>>> Login([FromBody] LoginUserDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(result);

            SetRefreshTokenCookie(result.Data!.RefreshToken);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<OperationResult<AuthResponseDto>>> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(OperationResult<AuthResponseDto>.Failure("Missing refresh token."));

            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest
            {
                RefreshToken = refreshToken
            });

            if (!result.IsSuccess)
                return Unauthorized(result);

            SetRefreshTokenCookie(result.Data!.RefreshToken);
            return Ok(result);
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}
