using AutoMapper;
using Backend.Application.Interfaces;
using Backend.Common.Results;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Backend.Repository;
using Backend.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly JwtSettings _jwtSettings;
        private readonly IAESEncryptionService _aesEncryptionService;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> userRepository, IOptions<JwtSettings> jwtSettings, IAESEncryptionService aesEncryptionService, IMapper mapper)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _aesEncryptionService = aesEncryptionService;
            _mapper = mapper;
        }

        public async Task<IDataResult<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.GetAsync(u => u.Username == registerDto.Username);
            if (existingUser != null)
            {
                return new ErrorDataResult<UserDto>("Username already exists.");
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return new SuccessDataResult<UserDto>(userDto, "User registered successfully.");
        }

        public async Task<IDataResult<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetAsync(u => u.Username == loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return new ErrorDataResult<LoginResponseDto>("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            var loginResponse = new LoginResponseDto
            {
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };

            return new SuccessDataResult<LoginResponseDto>(loginResponse, "Login successful.");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
