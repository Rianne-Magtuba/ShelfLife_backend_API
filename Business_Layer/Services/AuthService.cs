using BCrypt.Net;
using Business_Layer.DTOs.UserDTO;
using Common_Class.Entities;
using Common_Class.Interfaces;
using Data_Layer.Configuration;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Google.Rpc.Context.AttributeContext.Types;

namespace Business_Layer.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUserDataService _repo;
        private readonly JwtSettings _jwt;
        private readonly EmailService _emailService;


        public AuthService(IUserDataService repo, IOptions<JwtSettings> jwtOptions, EmailService emailService, ILogger<AuthService> logger)
        {

            _jwt = jwtOptions.Value;
            _repo = repo;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<string> RegisterAsync(RegisterDTO dto)
        {
            var existingEmail = await _repo.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                throw new ArgumentException("Email already exists");

            var existingUsername = await _repo.GetByUsernameAsync(dto.Username);
            if (existingUsername != null)
                throw new ArgumentException("Username already taken");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var auth = FirebaseAuth.DefaultInstance;

            var firebaseUser = await auth.CreateUserAsync(new UserRecordArgs
            {
                Email = dto.Email,
                Password = dto.Password,
                DisplayName = dto.Username
            });

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
                throw new UnauthorizedAccessException("Invalid credentials");

            var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!valid)
                throw new UnauthorizedAccessException("Invalid credentials");
            //var token = GenerateJwt(user);
            //Console.WriteLine(token);
            //return token;
            var token = GenerateJwt(user);
            _logger.LogInformation("LOGIN TOKEN: {Token}", token);
            return token;

        }

        private string GenerateJwt(User user)
        {
            Console.WriteLine($"[LOGIN JWT KEY] {_jwt.Key}");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Key)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<UserProfileDTO> GetProfileByEmailAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found");

            return new UserProfileDTO
            {
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task SendPasswordResetAsync(string email)
        {
            var auth = FirebaseAuth.DefaultInstance;

            UserRecord user;
            try
            {
                user = await auth.GetUserByEmailAsync(email);
            }
            catch (FirebaseAuthException)
            {
                throw new Exception("No account found with that email address");
            }

            if (user == null)
                throw new Exception("User not found");

            var resetLink =
                await auth.GeneratePasswordResetLinkAsync(email);

            await _emailService.SendResetPasswordEmail(
                email,
                resetLink);
        }


        public async Task UpdateProfileAsync(
        string currentEmail,
        UpdateProfileDTO dto)
        {
            var user = await _repo.GetByEmailAsync(currentEmail);

            if (user == null)
                throw new Exception("User not found");

            var existingEmail =
                await _repo.GetByEmailAsync(dto.Email);

            if (existingEmail != null &&
                existingEmail.Id != user.Id)
            {
                throw new Exception(
                    "Email already exists");
            }

            var existingUsername =
                await _repo.GetByUsernameAsync(dto.Username);

            if (existingUsername != null &&
                existingUsername.Id != user.Id)
            {
                throw new Exception(
                    "Username already exists");
            }

            user.Username = dto.Username;
            user.Email = dto.Email;

            await _repo.UpdateAsync(user);
        }

        public async Task ChangePasswordAsync(string email, ChangePasswordDTO dto)
        {
            var user = await _repo.GetByEmailAsync(email);

            if (user == null)
                throw new Exception("User not found");

            var valid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!valid)
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _repo.UpdateAsync(user);
        }

        public async Task<NotificationSettingsResponseDto>
    GetNotificationSettingsAsync(string userId)
        {
            var user = await _repo.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return new NotificationSettingsResponseDto
            {
                Enabled = user.NotificationEnabled,
                Frequency = user.NotificationFrequency,
                AlertLeadDays = user.NotificationLeadDays,
                ReminderHour = user.NotificationReminderHour,
                ReminderMinute = user.NotificationReminderMinute
            };
        }

        public async Task<bool>
            UpdateNotificationSettingsAsync(
                string userId,
                UpdateNotificationSettingsRequestDto request)
        {
            var user = await _repo.GetByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.NotificationEnabled = request.Enabled;
            user.NotificationFrequency = request.Frequency;
            user.NotificationLeadDays = request.AlertLeadDays;
            user.NotificationReminderHour = request.ReminderHour;
            user.NotificationReminderMinute = request.ReminderMinute;

            await _repo.UpdateAsync(user);

            return true;
        }
    }
}
