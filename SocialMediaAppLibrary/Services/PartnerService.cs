using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepository;
        public PartnerService(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }
       
        public Task<Partner> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public List<Partner> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Task<List<Partner>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Partner?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
        public Task<Partner> UpdateAsync(int id, Partner entity)
        {
            throw new NotImplementedException();
        }

        public Task<Partner> WriteAsync(Partner entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }





        //NEDENSTÅENDE SLETTES NÅR REPOS ER OVERGÅET TIL TASK
        public Partner Delete(int id)
        {
            throw new NotImplementedException();
        }
        public Partner Write(Partner entity)
        {
            throw new NotImplementedException();
        }
        public Partner Update(int id, Partner entity)
        {
            throw new NotImplementedException();
        }
        public Partner? ReadById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
