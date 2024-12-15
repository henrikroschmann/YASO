using Microsoft.Extensions.DependencyInjection;
using YASO;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

internal class ChoreographedTests
{
    private Saga? _saga;
    private IServiceProvider _serviceProvider;

    [Before(Test)]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<StepA>();
        serviceCollection.AddTransient<TestChoreographedMethod>();
        serviceCollection.AddTransient<StepC>();
        serviceCollection.AddSingleton<ISagaRepository, TestRepository>();
        serviceCollection.AddSingleton<Saga>();
        _serviceProvider = serviceCollection.BuildServiceProvider();

        _saga = _serviceProvider.GetService<Saga>();

        var id = new Identifier
        {
            Id = 123
        };

        _saga.CreateNewSaga(id)
            .AddStep<StepA>("first")
            .AddReactiveStep("second", "first")
            .AddStep<StepC>("third", "second")
            .BuildSaga();
    }

    [Test]
    public async Task ExecuteSagaInOrchestratedFashionWithoutSuccessAsync()
    {
        int counter = 0;

        do
        {
            await SagaCoordinator.ExecuteSagaAsync(_saga);
            counter++;
        } while (counter == 4);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Pending);
    }

    [Test]
    public async Task TriggerSagaProgressionInAChoreographedWay()
    {
        await Assert.That(_saga.Steps.Count(x => x.Status == SagaStatus.Pending)).IsEquivalentTo(3);

        var sagaStatus = await SagaCoordinator.ExecuteSagaAsync(_saga);
        await Assert.That(_saga.Steps.Count(x => x.Status == SagaStatus.Pending)).IsEquivalentTo(2);
        await sagaStatus.SaveStateAsync(_serviceProvider.GetService<ISagaRepository>());

        var tcm = _serviceProvider.GetService<TestChoreographedMethod>();

        var request = new SecondStepRequest("test");

        await tcm.Reaction(new Identifier
        {
            Id = 123
        }, "Second", request);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Success);
    }

    internal record SecondStepRequest(string Name);
}