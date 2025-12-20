
using CentroTerapia.Application.DTOs.Auth;

namespace CentroTerapia.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> ResgiterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
    
}
