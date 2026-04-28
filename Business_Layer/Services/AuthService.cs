using BCrypt.Net;
using Business_Layer.DTOs.UserDTO;
using Common_Class.Entities;
using Common_Class.Interfaces;
using Data_Layer.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserDataService _repo;
        private readonly JwtSettings _jwt;

        public AuthService(IUserDataService repo, IOptions<JwtSettings> jwtOptions)
        {
            _jwt = jwtOptions.Value;
            _repo = repo;
        }

        public async Task<string> RegisterAsync(RegisterDTO dto)
        {
            var existingEmail = await _repo.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new Exception("Email already exists");

            var existingUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (existingUsername != null)
                throw new Exception("Username already taken");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = await _repo.CreateAsync(new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = hash
            });

            return "Registration Successful";
        }

        public async Task<string> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials");

            var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!valid)
                throw new Exception("Invalid credentials");

            return GenerateJwt(user);
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Key)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username),
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id)
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<UserProfileDTO> GetProfileByEmailAsync(string email)
        {
            var user = await _repo.GetByIdAsync(email);

            if (user == null)
                throw new Exception("User not found");

            return new UserProfileDTO
            {
                Username = user.Username,
                Email = user.Email
            };
        }
    }
}
