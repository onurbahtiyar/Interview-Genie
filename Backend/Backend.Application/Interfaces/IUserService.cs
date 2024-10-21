using Backend.Application.DTOs;

namespace Backend.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
}
