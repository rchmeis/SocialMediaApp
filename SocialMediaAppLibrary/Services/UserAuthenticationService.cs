using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Cms;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Security;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    /// <summary>
    /// Handles user authentication, password verification, and in-memory failed-login lockout tracking.
    /// </summary>
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockOutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);

        private readonly IUserAuthenticationRepository _authenticationRepository;
        private readonly IMemoryCache _cache;
        private readonly PasswordHasher<UserAuthentication> _passwordHasher = new PasswordHasher<UserAuthentication>();

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthenticationService"/> class.
        /// </summary>
        /// <param name="authenticationRepository">The repository used to look up user authentication records.</param>
        /// <param name="cache">The in-memory cache used to track failed login attempts and lockouts.</param>
        public UserAuthenticationService(IUserAuthenticationRepository authenticationRepository, IMemoryCache cache)
        {
            _authenticationRepository = authenticationRepository;
            _cache = cache;
        }

        /// <summary>
        /// Authenticates a user by email or username and password while enforcing an in-memory lockout
        /// after consecutive failed attempts within the attempt window.
        /// </summary>
        /// <param name="emailOrUsername">The user's email address or username.</param>
        /// <param name="password">The plaintext password entered by the user.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a
        /// <see cref="LoginCheckResult"/> indicating whether authentication succeeded, the authenticated
        /// <see cref="UserAuthentication"/> record when successful, or a user-facing error message when denied.
        /// </returns>
        public async Task<LoginCheckResult> AuthenticateAsync(string emailOrUsername, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginCheckResult
                {
                    Succeeded = false,
                    ErrorMessage = "Ugyldigt login."
                };
            }

            string inputNormalised = emailOrUsername.Trim().ToLowerInvariant();
            var normalizedKey = $"login-lockout:{inputNormalised}";
            var now = DateTimeOffset.UtcNow;  //captures a UTC timestamp for time comparisons throughout this call


            //1. Check if locked out through state saved in cache
            if (_cache.TryGetValue(normalizedKey, out LoginAttemptState? state) && state is not null)
            {
                if (state.LockedUntilUtc is not null && now < state.LockedUntilUtc.Value)
                {
                    var remaining = state.LockedUntilUtc.Value - now;
                    var minutesLeft = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes)); //Calculates remaining time and rounds up to whole minutes (Math.Ceiling), with a minimum of 1 minute so it never says "0 minutes" when 20 seconds remain.

                    return new LoginCheckResult
                    {
                        Succeeded = false,
                        ErrorMessage = $"Kontoen er midlertidigt låst efter for mange mislykkede forsøg. Prøv igen om {minutesLeft} minut(ter)."
                    };
                }
                // if the lock has just expired reset state.
                if (state.LockedUntilUtc is not null && now >= state.LockedUntilUtc.Value)
                {
                    state.FailedAttempts = 0;
                    state.LockedUntilUtc = null;
                }
            }
            else //if no entry existed in the cache yet, create a fresh LoginAttemptState (FailedAttempts = 0, LockedUntilUtc = null).
            {
                state = new LoginAttemptState();
            }


            //2. After checking cache and confirming that the user is not locked out:
            UserAuthentication? userAuth = await _authenticationRepository.GetByEmailOrUsernameAsync(inputNormalised, cancellationToken);         

            var isValidUserAuth = userAuth is not null && userAuth.DeletedAt is null && VerifyPassword(userAuth, password);

            if (!isValidUserAuth)
            {
                state.FailedAttempts++;

                if (state.FailedAttempts >= MaxFailedAttempts)
                {
                    state.LockedUntilUtc = now.Add(LockOutDuration);
                    state.FailedAttempts = 0;

                    //Stores state in _cache with a slightly longer expiration (15 min + 1 min buffer) so the cache entry doesn't vanish before LockedUntilUtc is checked.
                    _cache.Set(normalizedKey, state, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = LockOutDuration + TimeSpan.FromMinutes(1)
                    });

                    return new LoginCheckResult
                    {
                        Succeeded = false,
                        ErrorMessage = "For mange mislykkede login-forsøg. Kontoen er låst i 15 minutter."
                    };
                }
                _cache.Set(normalizedKey, state, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = AttemptWindow //Saves/refreshes the cache entry with AbsoluteExpirationRelativeToNow = AttemptWindow (15 minutes).
                });

                return new LoginCheckResult
                {
                    Succeeded = false,
                    ErrorMessage = "Ugyldigt login."
                };
            }
            // Succes: ryd tæller/lås
            _cache.Remove(normalizedKey);

            return new LoginCheckResult
            {
                Succeeded = true,
                UserAuthentication = userAuth
            };
        }

        /// <summary>
        /// Verifies a plaintext password against the stored hash for the specified user authentication record.
        /// </summary>
        /// <param name="userAuth">The user authentication record containing the stored password hash.</param>
        /// <param name="password">The plaintext password to verify.</param>
        /// <returns><c>true</c> if the password matches (including when a rehash is recommended); otherwise, <c>false</c>.</returns>
        private bool VerifyPassword(UserAuthentication userAuth, string password)
        {
            try
            {
                PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(userAuth, userAuth.PasswordHashed, password);
                return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (FormatException ex)
            {
                //_logger.LogWarning(ex, "Malformed password hash for UserAuthentication {UserId}", userAuth.Id);
                // Safety net in case the password hash stored in the database is corrupted or malformed - avoids crashing the login request.
                return false;
            }
        }

       
        public Task<UserAuthentication?> ReadById(int id, CancellationToken cancellationToken)
        {
            return _authenticationRepository.ReadByIdAsync(id, cancellationToken);
        }
      
        /// <summary>
        /// Asynchronously writes a <see cref="UserAuthentication"/> record in the DBMS using the underlying authentication repository.
        /// </summary>
        /// <param name="entity">The <see cref="UserAuthentication"/> entity to persist.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous operation.</param>
        /// <returns>The persisted <see cref="UserAuthentication"/> entity returned by the repository.</returns>
        public async Task<UserAuthentication> WriteAsync(UserAuthentication entity, CancellationToken cancellationToken)
        {
            return await _authenticationRepository.WriteAsync(entity, cancellationToken);
        }

        public string HashPassword(UserAuthentication entity, string password)
        {
            
            return _passwordHasher.HashPassword(entity,password);
        }


        /// <summary>
        /// Checks whether an existing account already uses the specified email address or username.
        /// </summary>
        /// <param name="email">The email address candidate to check for duplicates.</param>
        /// <param name="username">The username candidate to check for duplicates.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="DuplicateAccountCheck"/> indicating whether the email and/or username is already taken.
        /// </returns>
        public async Task<DuplicateAccountCheck> CheckDuplicateAccountAsync(string email, string username, CancellationToken cancellationToken = default)
        {
            return await _authenticationRepository.CheckDuplicateAccountAsync(email, username, cancellationToken);
        }

        public Task<UserAuthentication?> ReadByIdAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserAuthentication>> ReadAllAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserAuthentication> UpdateAsync(int id, UserAuthentication entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserAuthentication> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
 

    }
}
