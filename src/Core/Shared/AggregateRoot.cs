using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnitTestingSamples.Core.Shared.Events;

namespace UnitTestingSamples.Core.Shared
{
    public abstract class AggregateRoot
    {
        private readonly ConcurrentQueue<IDomainEvent> _events;
        
        protected AggregateRoot()
        {
            _events = new ConcurrentQueue<IDomainEvent>();
        }
        
        public long Id { get; protected set; }
        
        protected void AddDomainEvent(IDomainEvent @event) => _events.Enqueue(@event);
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _events.ToImmutableList();
        public void ClearDomainEvents() => _events.Clear();
    }
}