using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repository
{
    public class PrivateMessageRepository: ICrud<PrivateMessage, int>
    {

        private readonly DbConnect _dbConnect;

        public PrivateMessageRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        PrivateMessage IWrite<PrivateMessage, int>.Delete(int id)
        {
            throw new NotImplementedException();
        }

        List<PrivateMessage> IRead<PrivateMessage, int>.ReadAll()
        {
            throw new NotImplementedException();
        }

        PrivateMessage? IRead<PrivateMessage, int>.ReadById(int id)
        {
            throw new NotImplementedException();
        }

        PrivateMessage IWrite<PrivateMessage, int>.Update(int id, PrivateMessage entity)
        {
            throw new NotImplementedException();
        }

        PrivateMessage IWrite<PrivateMessage, int>.Write(PrivateMessage entity)
        {
            throw new NotImplementedException();
        }
    }
}
