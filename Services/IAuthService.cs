using backend.DTOs;

namespace backend.Services;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterDto dto);
    Task<string?> LoginAsync(LoginDto dto);
}
