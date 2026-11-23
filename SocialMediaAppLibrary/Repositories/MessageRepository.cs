using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class MessageRepository: IMessageRepository
    {

        private readonly IDbConnect _dbConnect;

        public MessageRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Task<PrivateMessage> WriteAsync(PrivateMessage entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task<PrivateMessage> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<PrivateMessage>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }
       
        public Task<PrivateMessage?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }       

        public Task<PrivateMessage> UpdateAsync(int id, PrivateMessage entity)
        {
            throw new NotImplementedException();
        }


        //BRUG IKKE SYNKRONE METODER - SLETTES NÅR ALLE REPOS ER FLYTTET TIL ASYNC
        public PrivateMessage Write(PrivateMessage entity)
        {
            throw new NotImplementedException();
        }
        public List<PrivateMessage> ReadAll()
        {
            throw new NotImplementedException();
        }

        public PrivateMessage Delete(int id)
        {
            throw new NotImplementedException();
        }
        public PrivateMessage Update(int id, PrivateMessage entity)
        {
            throw new NotImplementedException();
        }
        public PrivateMessage? ReadById(int id)
        {
            throw new NotImplementedException();
        }

    }
}
