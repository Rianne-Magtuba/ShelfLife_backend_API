using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common_Class.Entities;
using Common_Class.Interfaces;
using Google.Cloud.Firestore;
using Data_Layer.Configuration;

namespace Data_Layer.Services
{

    public class UserDataService : IUserDataService
    {
        private readonly CollectionReference _users;

        public UserDataService(FirestoreDb db)
        {
            _users = db.Collection("Users");
        }

        public async Task<User> CreateAsync(User user)
        {
            
            var doc = await _users.AddAsync(user);
            user.Id = doc.Id;
            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var snapshot = await _users
                .WhereEqualTo("Email", email)
                .Limit(1)
                .GetSnapshotAsync();

            if (snapshot.Count == 0)
                return null;

            return snapshot.Documents[0].ConvertTo<User>();
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            var snapshot = await _users
                .WhereEqualTo("Username", username)
                .Limit(1)
                .GetSnapshotAsync();

            if (snapshot.Count == 0)
                return null;

            return snapshot.Documents[0].ConvertTo<User>();
        }
        public async Task<User?> GetByIdAsync(string id)
        {
            var doc = await _users.Document(id).GetSnapshotAsync();

            if (!doc.Exists)
                return null;

            return doc.ConvertTo<User>();
        }
        public async Task UpdateAsync(User user)
        {
            await _users
                .Document(user.Id)
                .SetAsync(user);
        }

        public async Task<int> getNumberOfUsersAsync()
        {
           await _users.GetSnapshotAsync();
            var snapshot = await _users.GetSnapshotAsync();
            return snapshot.Count;
        }
    }
}
