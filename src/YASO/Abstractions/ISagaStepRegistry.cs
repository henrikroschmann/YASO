namespace YASO.Abstractions;

public interface ISagaStepRegistry
{
    ISagaStep ResolveAction(string stepName);
}