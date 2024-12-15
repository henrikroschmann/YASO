using Microsoft.Extensions.DependencyInjection;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

internal class SagaTests
{
    private readonly ISagaIdentifier _identifier = new Identifier();
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
    public async Task BuildSagaAsync()
    {
        var saga = _saga.CreateNewSaga(_identifier).AddStep<StepA>("first")
            .AddStep<StepB>("second", "first")
            .AddStep<StepC>("third", "first").BuildSaga();
        await Verify(saga.ExportSaga());
    }

    [Test]
    public async Task SagaNotCompletedIfStepStillActive()
    {
        var saga = _saga.CreateNewSaga(_identifier).AddStep<StepA>("first");
        saga.MarkAsCompleted();
        await Assert.That(saga.Status).IsEqualTo(SagaStatus.Pending);
    }

    [Test]
    public async Task GetNextEligableStep()
    {
        var saga = _saga.CreateNewSaga(_identifier)
            .AddStep<StepA>("first")
            .AddStep<StepB>("second", "first")
            .AddStep<StepC>("third", "first")
            .BuildSaga();

        var nextSteps = saga.GetEligibleSteps();
        await Assert.That(nextSteps.Length).IsEqualTo(1);
        nextSteps[0].MarkAsCompleted();

        var remainingSteps = saga.GetEligibleSteps();
        await Assert.That(remainingSteps.Length).IsEqualTo(2);
    }

    [Test]
    public async Task CreateSagaWithRestrictions()
    {
        const bool featureFlag = false;

        var saga = _saga.CreateNewSaga(_identifier);
        saga.AddStep<StepA>("first")
            .When(featureFlag && 1 == 1, saga =>
             saga.AddStep<StepB>("second"))
            .BuildSaga();

        await Assert.That(saga.Steps.Count).IsEqualTo(1);
    }
}