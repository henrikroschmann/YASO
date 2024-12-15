using NSubstitute;
using System.Text.Json;
using YASO.Abstractions;
using YASO.Domain;
using YASOTests.Setup;

namespace YASOTests;

public class SaveAndLoadSagaTest
{
    [Test]
    public async Task SaveAndLoadSaga()
    {
        var sp = Substitute.For<IServiceProvider>();
        var repository = new TestRepository();
        var saga = new Saga(sp, repository).CreateNewSaga(new Identifier { Id = 0 }).BuildSaga();

        var result = await repository.GetSagaAsync(new Identifier { Id = 0 }, CancellationToken.None);
        await Assert.That(result).IsEqualTo(saga);
    }
}

internal class TestRepository : ISagaRepository
{
    public string Data { get; private set; } = string.Empty;

    public async Task<Saga> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken)
    {
        return JsonSerializer.Deserialize<Saga>(Data) ?? throw new InvalidOperationException("Deserialization failed");
    }

    public Task SaveSaga(Saga saga, CancellationToken cancellationToken)
    {
        Data = JsonSerializer.Serialize(saga);
        return Task.CompletedTask;
    }
}