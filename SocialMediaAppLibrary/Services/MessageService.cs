using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _privateMessageRepository;
        public MessageService(IMessageRepository privateMessageRepository)
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

        public Task<PrivateMessage?> ReadByIdAsync(int id)
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

        public Task<PrivateMessage> WriteAsync(PrivateMessage entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
