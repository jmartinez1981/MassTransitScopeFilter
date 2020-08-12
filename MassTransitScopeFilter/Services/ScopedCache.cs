using System;

namespace MassTransitScopeFilter.Services
{
    public class ScopedCache : IScopedCache
    {
        public Guid ConversationId { get; set; }
    }
}
