using YASO;
using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests.Setup;

internal class TestChoreographedMethod(ISagaRepository sagaRepository, Saga saga) : ISagaStep
{
    public Task<bool> Action()
    {
        Console.WriteLine("Hello");
        return Task.FromResult(true);
    }

    public async Task<SagaStoredState> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T? data)
    {
        var sagaState = await sagaRepository.GetSagaAsync(sagaIdentifier);
        saga.ReloadSaga(sagaState);

        Console.WriteLine(data);

        saga?.Steps.FirstOrDefault(x => x.Name.Equals(stepName, StringComparison.InvariantCultureIgnoreCase))?
            .MarkAsCompleted();
        return await SagaCoordinator.ExecuteSagaAsync(saga);
    }
}