using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class PrivateMessageService : IPrivateMessageService
    {
        private readonly IPrivateMessageRepository _privateMessageRepository;
        public PrivateMessageService(IPrivateMessageRepository privateMessageRepository)
        {
            _privateMessageRepository = privateMessageRepository;
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
