using System;
using UnitTestingSamples.Core.OrderAgg.Abstractions.Commands;
using UnitTestingSamples.Core.OrderAgg.Abstractions.Events;
using UnitTestingSamples.Core.Shared;


namespace UnitTestingSamples.Core.OrderAgg
{
    public class Order : AggregateRoot
    {
        public Order(ICreateOrder createOrder)
        {
            Amount = createOrder?.Amount ?? throw new ArgumentNullException(nameof(createOrder));
            AddDomainEvent(new OrderCreated(this));
        }
        
        public long Amount { get; }
    }
}