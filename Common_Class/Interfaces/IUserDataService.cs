using Common_Class.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Class.Interfaces
{
    public interface IUserDataService
    {
        Task<User> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(string id);
    }
}
