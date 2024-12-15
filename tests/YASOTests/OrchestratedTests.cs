using Microsoft.Extensions.DependencyInjection;
using YASO;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

internal class OrchestratedTests
{
    private Saga? _saga;

    [Before(Test)]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<StepA>();
        serviceCollection.AddTransient<StepB>();
        serviceCollection.AddTransient<StepC>();
        var sp = serviceCollection.BuildServiceProvider();

        _saga = new Saga(sp);

        var id = new Identifier
        {
            Id = 123
        };

        _saga = _saga.CreateNewSaga(id)
            .AddStep<StepA>("first")
            .AddStep<StepB>("second", "first")
            .AddStep<StepC>("third", "second")
            .When(1 == 1, saga => saga.AddStep<StepA>("fourth", "first", "third"))
            .AddStep<StepB>("five")
            .BuildSaga();
    }

    [Test]
    public async Task ExecuteSagaInOrchestratedFashionAsync()
    {
        do
        {
            await SagaCoordinator.ExecuteSagaAsync(_saga);
        } while (_saga.Status != SagaStatus.Success);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Success);
    }
}