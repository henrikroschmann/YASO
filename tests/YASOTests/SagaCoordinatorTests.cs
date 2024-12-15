using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YASO;
using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests;

public class SagaCoordinatorTests
{
    private readonly ISagaIdentifier _identifier = Substitute.For<ISagaIdentifier>();
    private Saga? _saga;

    [Before(Test)]
    public void Setup()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService<TestMethod>().Returns(new TestMethod());
        _saga = new Saga(serviceProvider, new TestRepository());
    }

    [Test]
    public async Task CoordinatorShouldCompleteSaga()
    {
        var repository = Substitute.For<ISagaRepository>();
        repository.SaveSaga(Arg.Any<Saga>(), Arg.Any<CancellationToken>());

        var saga = _saga.CreateNewSaga(_identifier)
            .AddStep<TestMethod>("first");

        var sc = new SagaCoordinator(repository);
        await sc.ExecuteSagaAsync(saga, CancellationToken.None);

        repository.Received(1).SaveSaga(saga, CancellationToken.None);
        await Assert.That(saga.Status).IsEqualTo(SagaStatus.Success);
    }

    [Test]
    public async Task CoordinatorShouldTerminateEarly()
    {
        var repository = Substitute.For<ISagaRepository>();
        repository.SaveSaga(Arg.Any<Saga>(), Arg.Any<CancellationToken>());

        var saga = _saga.CreateNewSaga(_identifier)
            .AddStep<TestMethod>("first");
        saga.Steps[0].MarkAsCompleted();

        var sc = new SagaCoordinator(repository);
        await sc.ExecuteSagaAsync(saga, CancellationToken.None);

        repository.Received(0).SaveSaga(saga, CancellationToken.None);
    }

    [Test]
    public async Task CoordinatorGetSaga()
    {
        ISagaIdentifier sagaIdentifier = Substitute.For<ISagaIdentifier>();
        var saga = _saga.CreateNewSaga(sagaIdentifier).AddStep<TestMethod>("first");
        var repository = Substitute.For<ISagaRepository>();
        repository.GetSagaAsync(Arg.Any<ISagaIdentifier>(), Arg.Any<CancellationToken>()).Returns(saga);

        var sc = new SagaCoordinator(repository);
        var result = sc.GetSagaAsync(sagaIdentifier, CancellationToken.None);
        await Assert.That(result).IsEqualTo(saga);
    }

    public class TestMethod : ISagaStep
    {
        public Task<bool> Action()
        {
            Console.WriteLine("Hello");
            return Task.FromResult(true);
        }

        public Task<bool> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data)
        {
            throw new NotImplementedException();
        }
    }
}