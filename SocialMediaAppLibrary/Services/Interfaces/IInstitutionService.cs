using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services.Interfaces
{
    public interface IInstitutionService :IInstitutionRepository
    {
        public Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken);
    }
}
