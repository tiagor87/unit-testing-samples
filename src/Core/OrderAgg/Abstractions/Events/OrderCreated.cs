using System;
using UnitTestingSamples.Core.Shared.Events;

namespace UnitTestingSamples.Core.OrderAgg.Abstractions.Events
{
    public class OrderCreated : IDomainEvent
    {
        public OrderCreated(Order order)
        {
            Order = order ?? throw new ArgumentNullException(nameof(order));
        }
        
        public Order Order { get; }
    }
}