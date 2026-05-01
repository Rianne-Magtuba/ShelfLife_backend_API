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
    }
}
