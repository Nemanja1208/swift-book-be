using Application.Auth.Dtos;
using Domain.Models.Common;

namespace Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<OperationResult<AuthResponseDto>> RegisterAsync(RegisterUserDto dto);
        Task<OperationResult<AuthResponseDto>> LoginAsync(LoginUserDto dto);
        Task<OperationResult<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequest dto);
    }
}
