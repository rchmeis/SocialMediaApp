using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class UserAuthenticationRepository : ICrud<UserAuthentication, int>
    {

        private readonly DbConnect _dbConnect;

        public UserAuthenticationRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public UserAuthentication Delete(int id)
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
