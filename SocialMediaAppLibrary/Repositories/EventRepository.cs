using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Repositories;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories
{
    public class EventRepository: IEventRepository
    {
        private readonly DbConnect _dbConnect;

        public EventRepository(DbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Event Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Event> ReadAll()
        {
            throw new NotImplementedException();
        }

        public Event? ReadById(int id)
        {
            throw new NotImplementedException();
        }

        public Event Update(int id, Event entity)
        {
            throw new NotImplementedException();
        }

        public Event Write(Event entity)
        {
            throw new NotImplementedException();        }

      
    }
}
