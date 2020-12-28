using System.Threading.Tasks;

namespace UnitTestingSamples.Core.Shared.Logger
{
    public interface ILogger
    {
        Task WriteWarning(string message);
    }
}