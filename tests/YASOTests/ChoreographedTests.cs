using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YASO;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

internal class ChoreographedTests
{
    private Saga? _saga;
    private IServiceProvider _serviceProvider;
    private ISagaIdentifier _sagaIdentifier;
    private ISagaRepository _sagaRepository;

    [Before(Test)]
    public void Setup()
    {
        _sagaRepository = new TestRepository();
        var sc = new SagaCoordinator(_sagaRepository);

        _serviceProvider = Substitute.For<IServiceProvider>();
        _serviceProvider.GetService<TestChoreographedMethod>().Returns(new TestChoreographedMethod(sc));

        _sagaIdentifier = new Identifier
        {
            Id = 123
        };

        _saga = new Saga(_serviceProvider, _sagaRepository).CreateNewSaga(_sagaIdentifier);

        _saga.AddStep<TestChoreographedMethod>("First")
            .AddReativeStep<TestChoreographedMethod>("Second", "First")
            .AddStep<TestChoreographedMethod>("Third", "Second")
            .BuildSaga();
    }

    [Test]
    public async Task ExecuteSagaInOrchestratedFashionWithoutSuccessAsync()
    {
        var respository = new TestRepository();
        var sc = new SagaCoordinator(respository);

        int counter = 0;

        do
        {
            await sc.ExecuteSagaAsync(_saga, CancellationToken.None);
            counter++;
        } while (counter == 4);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Pending);
    }

    [Test]
    public async Task TriggerSagaProgressionInAChoreographedWay()
    {
        var sc = new SagaCoordinator(_sagaRepository);

        await Assert.That(_saga.Steps.Count(x => x.Status == SagaStatus.Pending)).IsEquivalentTo(3);

        await sc.ExecuteSagaAsync(_saga, CancellationToken.None);
        await Assert.That(_saga.Steps.Count(x => x.Status == SagaStatus.Pending)).IsEquivalentTo(2);

        var tcm = _serviceProvider.GetService<TestChoreographedMethod>();

        var request = new SecondStepRequest("test");

        await tcm.Reaction(_sagaIdentifier, "Second", request);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Success);
    }

    internal record SecondStepRequest(string Name);
}