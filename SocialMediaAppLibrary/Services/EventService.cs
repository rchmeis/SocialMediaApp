using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories.Interfaces;
using SocialMediaAppLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialMediaAppLibrary.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
       
        public Task<Event> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> ReadAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Event?> ReadByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Event> UpdateAsync(int id, Event entity)
        {
            throw new NotImplementedException();
        }
        public Task<Event> WriteAsync(Event entity, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }





        //DONT USE SYNC METHODS FOR DB CALLS
        public Event? ReadById(int id)
        {
            throw new NotImplementedException();
        }
        public Event Delete(int id)
        {
            throw new NotImplementedException();
        }
        public Event Write(Event entity)
        {
            throw new NotImplementedException();
        }
        public Event Update(int id, Event entity)
        {
            throw new NotImplementedException();
        }
        public List<Event> ReadAll()
        {
            throw new NotImplementedException();
        }
    }
}
