using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests.Setup;

public class StepB : ISagaStep
{
    public Task<bool> Action()
    {
        Console.WriteLine("Hello from Saga step B");
        return Task.FromResult(true);
    }

    public Task<SagaStoredState> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data)
    {
        throw new NotImplementedException();
    }
}