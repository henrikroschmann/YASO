using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
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
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService<TestOrchestratedMethod>().Returns(new TestOrchestratedMethod());
        _saga = new Saga(serviceProvider, new TestRepository());

        var id = new Identifier
        {
            Id = 123
        };

        _saga = _saga.CreateNewSaga(id)
            .AddStep<TestOrchestratedMethod>("first")
            .AddStep<TestOrchestratedMethod>("second", "first")
            .AddStep<TestOrchestratedMethod>("third", "second")
            .When(1 == 1, saga => saga.AddStep<TestOrchestratedMethod>("fourth", "first", "third"))
            .AddStep<TestOrchestratedMethod>("five");
    }

    [Test]
    public async Task ExecuteSagaInOrchestratedFashionAsync()
    {
        var respository = new TestRepository();
        var sc = new SagaCoordinator(respository);

        do
        {
            await sc.ExecuteSagaAsync(_saga, CancellationToken.None);
        } while (_saga.Status != SagaStatus.Success);

        await Assert.That(_saga.Status).IsEquivalentTo(SagaStatus.Success);
    }
}