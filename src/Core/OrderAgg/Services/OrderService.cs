using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using UnitTestingSamples.Core.OrderAgg.Abstractions.Commands;
using UnitTestingSamples.Core.OrderAgg.Repositories;
using UnitTestingSamples.Core.Shared.Logger;

namespace UnitTestingSamples.Core.OrderAgg.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger _logger;

        public OrderService(IOrderRepository repository, ILogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Order> CreateAsync(ICreateOrder createOrder, CancellationToken cancellationToken)
        {
            var order = new Order(createOrder);
            await Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, i =>
                {
                    LogRetry(i);
                    return TimeSpan.FromMilliseconds(i * 100);
                })
                .ExecuteAsync(async ct => await _repository.AddAsync(order, ct), cancellationToken);
            return order;
        }

        protected virtual void LogRetry(int attempt)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(500);
                _logger.WriteWarning($"[OrderService] CreateAsyc: Attempt {attempt} failed.");
            });
        }   
    }
}