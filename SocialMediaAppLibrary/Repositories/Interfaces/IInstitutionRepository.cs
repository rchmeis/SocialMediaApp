using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public interface IInstitutionRepository:ICrud<Institute, int>
    {
        public Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken);
    }
}
