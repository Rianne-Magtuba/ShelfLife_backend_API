using Business_Layer.DTOs.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO dto);
        Task<string> LoginAsync(LoginDTO dto);
        Task<UserProfileDTO> GetProfileByEmailAsync(string email);
        Task SendPasswordResetAsync(string email);
        Task UpdateProfileAsync(string currentEmail,UpdateProfileDTO dto);

        Task ChangePasswordAsync(string email,ChangePasswordDTO dto);
        Task<NotificationSettingsResponseDto> GetNotificationSettingsAsync(string userId);

        Task<int> getNumberOfUsersAsync();
        Task<bool> UpdateNotificationSettingsAsync(string userId,UpdateNotificationSettingsRequestDto request);
    }
}
