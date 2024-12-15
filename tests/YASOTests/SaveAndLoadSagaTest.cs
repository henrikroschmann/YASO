using Microsoft.Extensions.DependencyInjection;
using YASO;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

public class SaveAndLoadSagaTest
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

        _saga = new Saga(sp)
            .CreateNewSaga(new Identifier { Id = 123 })
            .AddStep<StepA>("first")
            .AddStep<StepB>("second", "first")
            .AddStep<StepC>("third", "second")
            .BuildSaga();
    }

    [Test]
    public async Task SaveAndLoadSaga()
    {
        var repository = new TestRepository();
        var data = await SagaCoordinator.ExecuteSagaAsync(_saga);
        await data.SaveStateAsync(repository);

        

        // 2 steps should be completed
        await Assert.That(_saga.Steps.Count(s => s.Status != SagaStatus.Success)).IsEqualTo(2);

        // progress another step without saving
        await SagaCoordinator.ExecuteSagaAsync(_saga);
        await Assert.That(_saga.Steps.Count(s => s.Status != SagaStatus.Success)).IsEqualTo(1);

        // Let's reload the state from storage
        var sagaStates = await repository.GetSagaAsync(new Identifier { Id = 123 }, CancellationToken.None);
        _saga.ReloadSaga(sagaStates);

        // Verify that state is restored
        await Assert.That(_saga.Steps.Count(s => s.Status != SagaStatus.Success)).IsEqualTo(2);
    }
}
