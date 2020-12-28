using System.Threading;
using System.Threading.Tasks;

namespace UnitTestingSamples.Core.OrderAgg.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order, CancellationToken cancellationToken);
    }
}