using YASO.Abstractions;
using YASO.Domain;

namespace YASO;

public static class SagaCoordinator
{
    public static async Task<SagaStoredState> ExecuteSagaAsync(Saga saga)
    {
        if (ExitIfCompleted(saga))
        {
            return saga.ExportSaga();
        }

        foreach (var step in saga.GetEligibleSteps())
        {
            if (await step.ExecuteStep().ConfigureAwait(false))
            {
                step.MarkAsCompleted();
            }
        }

        saga.MarkAsCompleted();
        return saga.ExportSaga();
    }

    private static bool ExitIfCompleted(Saga saga)
    {
        saga.MarkAsCompleted();
        return saga.Status == SagaStatus.Success;
    }
}