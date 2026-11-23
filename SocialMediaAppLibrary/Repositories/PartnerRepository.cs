using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class PartnerRepository: IPartnerRepository
    {

        private readonly IDbConnect _dbConnect;

        public PartnerRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Task<Partner> WriteAsync(Partner entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Partner> DeleteAsync(int id)
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

        //BRUG IKKE SYNKRONE METODER - slettes npr alle repos har flyttet metoder til task
        public Partner? ReadById(int id)
        {
            throw new NotImplementedException();
        }
        public Partner Update(int id, Partner entity)
        {
            throw new NotImplementedException();
        }
        public List<Partner> ReadAll()
        {
            throw new NotImplementedException();
        }
        public Partner Write(Partner entity)
        {
            throw new NotImplementedException();
        }
       
        public Partner Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
