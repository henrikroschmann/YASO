using YASO.Abstractions;

namespace YASO.Domain;

public record SagaStep(string Name, string[] DependsOn) : Entity
{
    private ISagaStep? _action;
    private bool reactiveStep = false;

    internal void AddAction(ISagaStep action)
    {
        _action = action;
    }

    internal void AddReaction()
    {
        reactiveStep = true;
    }

    internal async Task<bool> ExecuteStep()
    {
        if (reactiveStep) { return false; }
        if (_action == null)
        {
            throw new InvalidOperationException("Action is not set.");
        }
        await _action.Action();
        return true;
    }
}