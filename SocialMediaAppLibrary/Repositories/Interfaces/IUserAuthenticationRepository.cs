using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IUserAuthenticationRepository: ICrud<UserAuthentication, int>
    {
        public Task<UserAuthentication> GetByEmailOrUsernameAsync(string email);        
    }
}
