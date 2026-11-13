using SocialMediaAppLibrary.Models;
using SocialMediaAppLibrary.Repositories;
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
