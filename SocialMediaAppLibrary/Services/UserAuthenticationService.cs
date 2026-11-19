using Microsoft.AspNetCore.Identity;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserAuthenticationRepository _authenticationRepository;
        private readonly PasswordHasher<UserAuthentication> _passwordHasher = new PasswordHasher<UserAuthentication>();
        public UserAuthenticationService(IUserAuthenticationRepository authenticationRepository)
        {
            _authenticationRepository = authenticationRepository;
        }

        /// <summary>
        /// Authenticates a user by validating their email or username against the database 
        /// and verifying the provided password using a secure password hasher.
        /// </summary>
        /// <param name="emailOrUsername">The user's email address or username.</param>
        /// <param name="password">The plaintext password entered by the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains 
        /// the <see cref="UserAuthentication"/> object if authentication succeeds; otherwise, <c>null</c>.
        /// </returns>
        public async Task<UserAuthentication> AuthenticateAsync(string emailOrUsername, string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }
            string inputNormalised = emailOrUsername.Trim().ToLowerInvariant();     //no difference between capitalised og regular letters in username and password ie. us-04 og 01
            UserAuthentication userAuth = (await GetByEmailAsync(inputNormalised)) ?? (await GetByUsernameAsync(inputNormalised));
            if (userAuth is null) return null;

            try
            {
                PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(userAuth, userAuth.PasswordHashed, password);
                return result == PasswordVerificationResult.Success ? userAuth : null;
            }
            catch(FormatException)
            {
                // Safety net in case the password hash stored in the database is corrupted or malformed
                return null;
            }
        }

        public UserAuthentication Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAuthentication> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAuthentication> GetByUsernameAsync(string username)
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
