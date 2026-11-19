using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserAuthenticationRepository: ICrud<UserAuthentication, int>
    {
        public Task<UserAuthentication> GetByEmailAsync(string email);
        public Task<UserAuthentication> GetByUsernameAsync(string username);
    }
}
