using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class InstitutionService : IInstitutionService
    {
        private readonly IInstitutionRepository _institutionRepository;
        public InstitutionService(IInstitutionRepository institutionRepository)
        {
            _institutionRepository = institutionRepository;
        }
        public Institute Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Institute> ReadAll()
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> ReadAllDomainsAsync(CancellationToken cancellationToken)
        {
            return await _institutionRepository.ReadAllDomainsAsync(cancellationToken);
        }

        public Institute? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Institute Update(int id, Institute entity)
        {
            throw new NotImplementedException();
        }

        public Institute Write(Institute entity)
        {
            throw new NotImplementedException();
        }
    }
}
