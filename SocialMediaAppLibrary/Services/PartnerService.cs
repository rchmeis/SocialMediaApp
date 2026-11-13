using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
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
        public Partner Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Partner> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Partner? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Partner Update(int id, Partner entity)
        {
            throw new NotImplementedException();
        }

        public Partner Write(Partner entity)
        {
            throw new NotImplementedException();
        }
    }
}
