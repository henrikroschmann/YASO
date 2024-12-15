using YASO.Abstractions;

namespace YASOTests.Setup;

internal class TestOrchestratedMethod : ISagaStep
{
    public Task<bool> Action()
    {
        Console.WriteLine("Hello");
        return Task.FromResult(true);
    }

    public Task<bool> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data)
    {
        throw new NotImplementedException();
    }
}