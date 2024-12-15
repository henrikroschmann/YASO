using YASO.Abstractions;
using YASO.Domain;

namespace YASO;

public class SagaCoordinator(ISagaRepository sagaRepository)
{
    private readonly ISagaRepository _sagaRepository = sagaRepository;

    public async Task ExecuteSagaAsync(Saga saga, CancellationToken cancellationToken)
    {
        if (ExitIfCompleted(saga))
        {
            return;
        }

        foreach (var step in saga.GetEligibleSteps())
        {
            if (await step.ExecuteStep())
            {
                step.MarkAsCompleted();
            }
        }

        saga.MarkAsCompleted();
        await _sagaRepository.SaveSaga(saga, cancellationToken);
    }

    public async Task<Saga> GetSagaAsync(ISagaIdentifier sagaIdentifier, CancellationToken cancellationToken) =>
         await _sagaRepository.GetSagaAsync(sagaIdentifier, cancellationToken);

    private static bool ExitIfCompleted(Saga saga)
    {
        saga.MarkAsCompleted();
        return saga.Status == SagaStatus.Success;
    }
}