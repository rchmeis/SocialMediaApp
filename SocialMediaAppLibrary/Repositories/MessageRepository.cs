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

        public PrivateMessage Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<PrivateMessage> ReadAll()
        {
            throw new NotImplementedException();
        }

        public PrivateMessage? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public PrivateMessage Update(int id, PrivateMessage entity)
        {
            throw new NotImplementedException();
        }

        public PrivateMessage Write(PrivateMessage entity)
        {
            throw new NotImplementedException();
        }
    }
}
