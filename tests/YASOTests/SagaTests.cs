using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests;

internal class SagaTests
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
    public async Task BuildSagaAsync()
    {
        var saga = _saga.CreateNewSaga(_identifier);
        await Verify(saga);
    }

    [Test]
    public void SagaNotCompletedIfStepStillActive()
    {
        var saga = _saga.CreateNewSaga(_identifier).AddStep<TestMethod>("first");
        saga.MarkAsCompleted();
        Assert.Equals(saga.Status, SagaStatus.Pending);
    }

    [Test]
    public async Task GetNextEligableStep()
    {
        var saga = _saga.CreateNewSaga(_identifier)
            .AddStep<TestMethod>("first")
            .AddStep<TestMethod>("second", "first")
            .AddStep<TestMethod>("third", "first");

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
        saga.AddStep<TestMethod>("first")
            .When(featureFlag && 1 == 1, saga =>
             saga.AddStep<TestMethod>("second"));

        await Assert.That(saga.Steps.Count).IsEqualTo(1);
    }

    public class TestMethod : ISagaStep
    {
        public Task<bool> Action()
        {
            Console.WriteLine();
            return Task.FromResult(true);
        }

        public Task<bool> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data)
        {
            throw new NotImplementedException();
        }
    }
}