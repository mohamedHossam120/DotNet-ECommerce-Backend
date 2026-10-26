
using E_Commerce.DTOs.Auth;

namespace E_Commerce.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    Task<AuthResponseDto> LoginAsync(LoginDto dto);

    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

    Task LogoutAsync(LogoutDto dto);
}
