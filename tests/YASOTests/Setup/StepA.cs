using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests.Setup;

public class StepA : ISagaStep
{
    public Task<bool> Action()
    {
        Console.WriteLine("Hello from saga Step A");
        return Task.FromResult(true);
    }

    public Task<SagaStoredState> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data)
    {
        throw new NotImplementedException();
    }
}