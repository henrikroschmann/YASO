using Microsoft.Extensions.DependencyInjection;
using YASO.Domain;
using YASO;
using YASOTests.Setup;

namespace YASOTests;

public class DependencyInjectionTests
{
    [Test]
    public async Task AddYASORegistersSagaAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddYASO();
        var provider = services.BuildServiceProvider();

        // Assert
        var saga1 = provider.GetService<Saga>();
        var saga2 = provider.GetService<Saga>();

        await Assert.That(saga1).IsNotNull();
        await Assert.That(saga2).IsNotNull();
        await Assert.That(saga1).IsEqualTo(saga2);
    }

    [Test]
    public async Task AddStepsFromAssembly_RegistersAllISagaSteps()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(DependencyInjectionTests).Assembly;

        // Act
        services.AddStepsFromAssembly(assembly);
        var provider = services.BuildServiceProvider();

        var steps = provider.GetServices<StepA>();
        await Assert.That(steps).IsNotNull();

        var step1 = provider.GetService<StepB>();
        var step2 = provider.GetService<StepC>();

        await Assert.That(step1).IsNotNull();
        await Assert.That(step2).IsNotNull();
    }
}