using System.Text.Json;
using YASO.Abstractions;
using YASO.Domain;

namespace YASOTests.Setup;

internal class TestRepository : ISagaRepository
{
    public string data { get; private set; }

    public async Task<Saga> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken)
    {
        return JsonSerializer.Deserialize<Saga>(data);
    }

    public Task SaveSaga(Saga saga, CancellationToken cancellationToken)
    {
        data = JsonSerializer.Serialize(saga);
        return Task.CompletedTask;
    }
}