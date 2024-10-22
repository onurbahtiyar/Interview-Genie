using Backend.Common.Results;
using Backend.Domain.DTOs;

namespace Backend.Application.Interfaces;

public interface IUserService
{
    Task<IDataResult<UserDto>> RegisterAsync(RegisterDto registerDto);
    Task<IDataResult<LoginResponseDto>> LoginAsync(LoginDto loginDto);
}
