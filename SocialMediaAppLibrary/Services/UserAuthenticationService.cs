using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserAuthenticationRepository _authenticationRepository;
        public UserAuthenticationService(IUserAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
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
