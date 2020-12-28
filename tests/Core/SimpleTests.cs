using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using UnitTestingSamples.Core.OrderAgg;
using UnitTestingSamples.Core.OrderAgg.Abstractions.Commands;
using UnitTestingSamples.Core.OrderAgg.Repositories;
using UnitTestingSamples.Core.OrderAgg.Services;
using UnitTestingSamples.Core.Shared.Logger;
using Xunit;

namespace UnitTestingSamples.Core.Tests
{
    public class SimpleTests
    {
        [Fact]
        public async Task GivenOrderWhenCreateShouldAdd()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            const long amount = 10000;
            var commandMock = new Mock<ICreateOrder>();
            commandMock.SetupGet(x => x.Amount)
                .Returns(amount);
            var loggerMock = new Mock<ILogger>();
            var repositoryMock = new Mock<IOrderRepository>();
            repositoryMock.Setup(x => x.AddAsync(It.IsAny<Order>(), cancellationToken)).Returns(Task.CompletedTask);
            var service = new OrderService(repositoryMock.Object, loggerMock.Object);

            // Act
            var order = await service.CreateAsync(commandMock.Object, cancellationToken);
            
            // Assert
            order.Should().NotBeNull();
            order.Amount.Should().Be(amount);
            commandMock.VerifyGet(x => x.Amount, Times.Once());
            repositoryMock.Verify(x => x.AddAsync(order, cancellationToken), Times.Once());
        }
        
        [Fact]
        public async Task GivenOrderWhenCreateFailsShouldRetryAdd()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            const long amount = 10000;
            var commandMock = new Mock<ICreateOrder>();
            commandMock.SetupGet(x => x.Amount)
                .Returns(amount);
            var loggerMock = new Mock<ILogger>();
            var repositoryMock = new Mock<IOrderRepository>();
            repositoryMock.SetupSequence(x => x.AddAsync(It.IsAny<Order>(), cancellationToken))
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Throws<Exception>()
                .Returns(Task.CompletedTask);
            var service = new OrderService(repositoryMock.Object, loggerMock.Object);

            // Act
            var order = await service.CreateAsync(commandMock.Object, cancellationToken);
            
            // Assert
            order.Should().NotBeNull();
            order.Amount.Should().Be(amount);
            commandMock.VerifyGet(x => x.Amount, Times.Once());
            repositoryMock.Verify(x => x.AddAsync(order, cancellationToken), Times.Exactly(5));
        }
        
        [Fact]
        public async Task GivenOrderWhenCreateFailsShouldLogRetry()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            const long amount = 10000;
            var commandMock = new Mock<ICreateOrder>();
            commandMock.SetupGet(x => x.Amount)
                .Returns(amount);
            var loggerMock = new Mock<ILogger>();
            var repositoryMock = new Mock<IOrderRepository>();
            repositoryMock.SetupSequence(x => x.AddAsync(It.IsAny<Order>(), cancellationToken))
                .Throws<Exception>()
                .Returns(Task.CompletedTask);
            var serviceMock = new Mock<OrderService>(repositoryMock.Object, loggerMock.Object);
            serviceMock.Protected().Setup("LogRetry", It.IsAny<int>()).Verifiable();
            
            // Act
            var order = await serviceMock.Object.CreateAsync(commandMock.Object, cancellationToken);
            
            // Assert
            order.Should().NotBeNull();
            order.Amount.Should().Be(amount);
            commandMock.VerifyGet(x => x.Amount, Times.Once());
            repositoryMock.Verify(x => x.AddAsync(order, cancellationToken), Times.Exactly(2));
            serviceMock.Protected().Verify("LogRetry", Times.Once(), 1);
        }
        
        [Fact]
        public async Task GivenOrderWhenCreateFailsAndLogShouldCallAsFireAndForget()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var autoreset = new AutoResetEvent(false);
            const long amount = 10000;
            var commandMock = new Mock<ICreateOrder>();
            commandMock.SetupGet(x => x.Amount)
                .Returns(amount);
            var repositoryMock = new Mock<IOrderRepository>();
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.WriteWarning(It.IsAny<string>()))
                .Callback((string msg) => autoreset.Set())
                .Returns(Task.CompletedTask);
            repositoryMock.SetupSequence(x => x.AddAsync(It.IsAny<Order>(), cancellationToken))
                .Throws<Exception>()
                .Returns(Task.CompletedTask);
            var service = new OrderService(repositoryMock.Object, loggerMock.Object);
            
            
            // Act
            var order = await service.CreateAsync(commandMock.Object, cancellationToken);
            autoreset.WaitOne();

            // Assert
            order.Should().NotBeNull();
            order.Amount.Should().Be(amount);
            commandMock.VerifyGet(x => x.Amount, Times.Once());
            repositoryMock.Verify(x => x.AddAsync(order, cancellationToken), Times.Exactly(2));
            loggerMock.VerifyAll();
        }
    }
}
