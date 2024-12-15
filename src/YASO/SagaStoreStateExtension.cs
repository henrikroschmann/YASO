using YASO.Abstractions;
using YASO.Domain;

namespace YASO;

public static class SagaStoreStateExtension
{
    public static async Task SaveStateAsync(this SagaStoredState saga, ISagaRepository sagaRepository) =>
        await sagaRepository.SaveSaga(saga);
}