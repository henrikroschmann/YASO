using YASO.Domain;

namespace YASO.Abstractions;

public interface ISagaRepository
{
    Task SaveSaga(SagaStoredState saga, CancellationToken cancellationToken = default);

    Task<SagaStoredState> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken = default);
}