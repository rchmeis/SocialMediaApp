using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IUserAuthenticationService :ICrud<UserAuthentication, int>
    {
        public Task<LoginCheckResult> AuthenticateAsync(string emailOrUsername, string password, CancellationToken cancellationtoken);
        public string HashPassword(UserAuthentication entity, string password);
        public Task<DuplicateAccountCheck> CheckDuplicateAccountAsync(string email, string username, CancellationToken cancellationToken);


    }
}
