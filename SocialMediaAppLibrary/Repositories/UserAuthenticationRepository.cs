using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using SocialMediaAppLibrary.Repositories.Interfaces;

namespace SocialMediaAppLibrary.Repositories
{
    public class UserAuthenticationRepository : IUserAuthenticationRepository
    {

        private readonly IDbConnect _dbConnect;

        public UserAuthenticationRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public UserAuthentication Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAuthentication> GetByEmailOrUsernameAsync(string email)
        {
            throw new NotImplementedException();
        }


        public List<UserAuthentication> ReadAll()
        {
            throw new NotImplementedException();
        }

        public UserAuthentication? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public UserAuthentication Update(int id, UserAuthentication entity)
        {
            throw new NotImplementedException();
        }

        public UserAuthentication Write(UserAuthentication entity)
        {
            throw new NotImplementedException();
        }
    }
}
