using System.ComponentModel.DataAnnotations;
using YASO.Abstractions;

namespace YASO.Domain;

public sealed record Saga(IServiceProvider ServiceProvider) : Entity
{
    private IServiceProvider? _serviceProvider = ServiceProvider;
    public ISagaIdentifier? Identifier { get; private set; } // Not sure this is what I want
    public List<SagaStep> Steps { get; private set; } = [];
    private readonly Dictionary<string, Type> _stepTypeMap = [];
    public Dictionary<string, string> _stepTypeNames { get; private set; } = [];

    public Saga CreateNewSaga(ISagaIdentifier? identifier)
    {
        Id = Guid.NewGuid();
        Identifier = identifier;
        return this;
    }

    public Saga AddStep<T>(string name, params string[] dependsOn) where T : class, ISagaStep
    {
        Steps.Add(new SagaStep(name, dependsOn));
        var type = typeof(T);

        _stepTypeMap[name] = type;
        _stepTypeNames[name] = type.AssemblyQualifiedName
            ?? throw new InvalidOperationException("Unable to get AssemblyQualifiedName of step type.");

        return this;
    }

    public Saga AddReactiveStep(string name, params string[] dependsOn)
    {
        var step = new SagaStep(name, dependsOn);
        step.AddReaction();
        Steps.Add(step);
        return this;
    }

    public Saga BuildSaga()
    {
        ValidateSteps();
        AttachActionsFromServiceProvider();
        return this;
    }

    internal void ReloadSaga(SagaStoredState sagaStoredState)
    {
        SetStatus(sagaStoredState.Status);
        foreach (var step in Steps)
        {
            if (sagaStoredState.StepStatuses.TryGetValue(step.Name, out var status))
            {
                step.SetStatus(status);
            }
        }
    }

    internal SagaStoredState ExportSaga()
    {
        return new SagaStoredState
        {
            Id = Id,
            Status = Status,
            StepStatuses = Steps.ToDictionary(x => x.Name, x => x.Status)
        };
    }

    internal override void MarkAsCompleted()
    {
        if (Steps.All(x => x.Status == SagaStatus.Success))
        {
            base.MarkAsCompleted();
        }
    }

    private void ValidateSteps()
    {
        HashSet<string> names = [];
        foreach (var step in Steps)
        {
            if (!names.Add(step.Name))
            {
                throw new ValidationException("Step names should be unique");
            }
        }

        foreach (var step in Steps)
        {
            var steps = Steps.Where(x => x.DependsOn.Contains(step.Name)).Select(x => x.Name);
            if (step.DependsOn.Any(x => steps.Contains(x)))
            {
                throw new ValidationException("Circular dependancy deptect");
            }
        }
    }

    internal SagaStep[] GetEligibleSteps()
    {
        var completedSteps = Steps
            .Where(s => s.Status == SagaStatus.Success)
            .Select(s => s.Name)
            .ToHashSet();

        var pendingSteps = Steps.Where(s => s.Status == SagaStatus.Pending).ToList();

        return pendingSteps
            .Where(step => step.DependsOn.All(dep => completedSteps.Contains(dep)))
            .ToArray();
    }

    internal Saga When(bool predicate, Func<Saga, Saga> saga)
    {
        if (predicate)
        {
            saga(this);
        }
        return this;
    }

    private void AttachActionsFromServiceProvider()
    {
        foreach (var step in Steps.Where(s => !s.IsReactive))
        {
            if (_stepTypeMap.TryGetValue(step.Name, out var stepType))
            {
                ISagaStep? action = _serviceProvider?.GetService(stepType) as ISagaStep ?? throw new InvalidOperationException(
                        $"No ISagaStep service registered for step '{step.Name}' of type '{stepType.FullName}'");
                step.AddAction(action);
            }
            else
            {
                throw new InvalidOperationException($"No type mapping found for step '{step.Name}'.");
            }
        }
    }
    internal void SetStatus(SagaStatus status)
    {
        typeof(Entity).GetProperty(nameof(Status))!
            .SetValue(this, status);
    }
}