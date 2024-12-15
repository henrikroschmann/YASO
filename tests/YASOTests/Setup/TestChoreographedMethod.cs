using YASO;
using YASO.Abstractions;

namespace YASOTests.Setup;

internal class TestChoreographedMethod(SagaCoordinator sagaCoordinator) : ISagaStep
{
    public Task<bool> Action()
    {
        Console.WriteLine("Hello");
        return Task.FromResult(true);
    }

    public async Task<bool> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T? data)
    {
        var saga = await sagaCoordinator.GetSagaAsync(sagaIdentifier, CancellationToken.None);
        saga?.Steps.FirstOrDefault(x => x.Name == stepName)?.MarkAsCompleted();
        await sagaCoordinator.ExecuteSagaAsync(saga, CancellationToken.None);
        return true;
    }
}