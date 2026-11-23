using SocialMediaAppLibrary.Database;
using SocialMediaAppLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Repositories.Interfaces
{
    public class EventRepository: IEventRepository
    {
        private readonly IDbConnect _dbConnect;

        public EventRepository(IDbConnect dbConnect)
        {
            _dbConnect = dbConnect;
        }

        public Task<Event> WriteAsync(Event entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Event?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Event> UpdateAsync(int id, Event entity)
        {
            throw new NotImplementedException();
        }

        public Task<Event> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        //BRUG IKKE SYNKRONE METODER, slettes fra ICRUD når alle repos er skiftet til async
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
            throw new NotImplementedException();
        }


    }
}
