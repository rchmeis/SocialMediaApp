using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<User> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UserSummary> GetByUserAuthIdAsync(int userAuthId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetByUserAuthIdAsync(userAuthId, cancellationToken);
        }

        public List<User> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public User? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<User?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            return _userRepository.ReadByIdAsync(id, cancellationToken);
        }

        public User Update(int id, User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(int id, User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public User Write(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User> WriteAsync(User entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
