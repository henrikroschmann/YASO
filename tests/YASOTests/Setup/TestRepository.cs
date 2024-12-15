using System.Text.Json;
using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests.Setup;

internal class TestRepository : ISagaRepository
{
    public string Data { get; private set; } = string.Empty;

    public async Task<SagaStoredState> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken)
    {
        return JsonSerializer.Deserialize<SagaStoredState>(Data) ?? throw new InvalidOperationException("Deserialization failed");
    }

    public Task SaveSaga(SagaStoredState saga, CancellationToken cancellationToken = default)
    {
        Data = JsonSerializer.Serialize(saga);
        return Task.CompletedTask;
    }
}