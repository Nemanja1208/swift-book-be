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
        public async Task<ActionResult<OperationResult<AuthResponseDto>>> Register(RegisterUserDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (!result.IsSuccess)
                return Unauthorized(result);

            var refreshToken = result.Data!.RefreshToken;

            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                token = result.Data.Token,
                user = result.Data.User
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest
            {
                RefreshToken = refreshToken!
            });

            if (!result.IsSuccess)
                return Unauthorized(result);

            // Renew cookie
            var newRefresh = result.Data!.RefreshToken;
            Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                token = result.Data.Token,
                user = result.Data.User
            });
        }
    }
}
