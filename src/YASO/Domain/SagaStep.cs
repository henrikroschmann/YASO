using System.Text.Json.Serialization;
using YASO.Abstractions;

namespace YASO.Domain;

public record SagaStep(string Name, string[] DependsOn) : Entity
{
    [JsonIgnore]
    private ISagaStep? _action;
    public bool IsReactive { get; private set; }

    internal void AddAction(ISagaStep action)
    {
        _action = action;
    }

    internal void AddReaction()
    {
        IsReactive = true;
    }

    internal async Task<bool> ExecuteStep()
    {
        if (IsReactive) { return false; }
        if (_action == null)
        {
            throw new InvalidOperationException("Action is not set.");
        }
        return await _action.Action().ConfigureAwait(false);
    }

    internal void SetStatus(SagaStatus status)
    {
        typeof(Entity).GetProperty(nameof(Status))!
            .SetValue(this, status);
    }
}
