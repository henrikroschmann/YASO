using Microsoft.Extensions.DependencyInjection;
using YASO.Abstractions;

namespace YASO.Domain;

public sealed record Saga(IServiceProvider ServiceProvider, ISagaRepository Repository) : Entity
{
    private readonly IServiceProvider _serviceProvider = ServiceProvider;
    private readonly ISagaRepository _repository = Repository;
    public ISagaIdentifier? Identifier { get; private set; }
    public List<SagaStep> Steps { get; private set; } = [];
    public Saga CreateNewSaga(ISagaIdentifier? identifier)
    {
        Identifier = identifier;
        return this;
    }

    public Saga AddStep<T>(string name, params string[] dependsOn) where T : ISagaStep
    {
        var sagaStep = new SagaStep(name, dependsOn);
        var action = _serviceProvider.GetService<T>() ?? throw new Exception("Service added for step is not registed");
        sagaStep.AddAction(action);
        Steps.Add(sagaStep);
        return this;
    }

    public Saga AddReativeStep<T>(string name, params string[] dependsOn) where T : ISagaStep
    {
        var sagaStep = new SagaStep(name, dependsOn);
        sagaStep.AddReaction();
        Steps.Add(sagaStep);
        return this;
    }

    public Saga BuildSaga()
    {
        _repository.SaveSaga(this, new CancellationToken());
        return this;
    }

    internal override void MarkAsCompleted()
    {
        if (!Steps.Any(x => x.Status != SagaStatus.Success))
        {
            base.MarkAsCompleted();
        }
    }

    internal Saga When(bool predicate, Func<Saga, Saga> saga)
    {
        if (predicate)
        {
            saga(this);
        }

        return this;
    }

    internal SagaStep[] GetEligibleSteps()
    {
        List<SagaStep> eligibleSteps = [];
        var rootSteps = Steps.Where(s => s.DependsOn is null || s.DependsOn.Length == 0).ToList();
        eligibleSteps.AddRange(rootSteps.Where(step => step.Status == SagaStatus.Pending));

        if (eligibleSteps.Count == 0)
        {
            GetDependentSteps(eligibleSteps, rootSteps);
        }

        return [.. eligibleSteps];
    }

    private void GetDependentSteps(List<SagaStep> eligibleSteps, List<SagaStep> rootSteps)
    {
        foreach (var step in rootSteps)
        {
            var dependentSteps = Steps.Where(s => s.DependsOn.Contains(step.Name)).ToList();
            var activeDependentSteps = dependentSteps.Where(s => s.Status == SagaStatus.Pending).ToList();

            if (activeDependentSteps.Count != 0)
            {
                eligibleSteps.AddRange(activeDependentSteps);
            }
            else
            {
                GetDependentSteps(eligibleSteps, dependentSteps);
            }
        }
    }
}