using YASO.Domain;

namespace YASO.Abstractions;

public interface ISagaRepository
{
    Task SaveSaga(Saga saga, CancellationToken cancellationToken);

    Task<Saga> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken);
}