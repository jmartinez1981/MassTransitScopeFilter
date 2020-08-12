using System;

namespace MassTransitScopeFilter.Services
{
    public interface IScopedCache
    {
        Guid ConversationId { get; set; }
    }
}
