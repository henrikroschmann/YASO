using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
using System.ComponentModel.DataAnnotations;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

public class SagaValidationTests
{
    private Saga? _saga;

    [Before(Test)]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<StepA>();
        serviceCollection.AddTransient<StepB>();
        var sp = serviceCollection.BuildServiceProvider();

        _saga = new Saga(sp);
    }

    [Test]
    public async Task ThrowValidationErrorIfDuplicatedStepNames()
    {
        await Assert.That(() => _saga.CreateNewSaga(new Identifier { Id = 123 })
            .AddStep<StepA>("StepA")
            .AddStep<StepB>("StepA")
            .BuildSaga()).Throws<ValidationException>().WithMessage("Step names should be unique");
    }

    [Test]
    public async Task ThrowValidationErrorIfCircularDependancy()
    {
        await Assert.That(() => _saga.CreateNewSaga(new Identifier { Id = 123 })
           .AddStep<StepA>("StepA", "StepB")
           .AddStep<StepB>("StepB", "StepA")
           .BuildSaga()).Throws<ValidationException>().WithMessage("Circular dependancy deptect");
    }
}