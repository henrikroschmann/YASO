using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YASO;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

public class SagaCoordinatorTests
{
    private readonly ISagaIdentifier _identifier = Substitute.For<ISagaIdentifier>();
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
    }

    [Test]
    public async Task CoordinatorShouldCompleteSaga()
    {
        var saga = _saga.CreateNewSaga(_identifier)
            .AddStep<StepA>("first")
            .BuildSaga();

        await SagaCoordinator.ExecuteSagaAsync(saga);

        await Assert.That(saga.Status).IsEqualTo(SagaStatus.Success);
    }
}