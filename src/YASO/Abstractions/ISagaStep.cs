using YASO.Domain;

namespace YASO.Abstractions;

public interface ISagaStep
{
    /// <summary>
    /// Action is a step that is executed by the coordinator
    /// </summary>
    /// <returns></returns>
    Task<bool> Action();

    /// <summary>
    /// Reaction is a step that might be invoked outside the coordinator
    /// Still recommended to use the coordnator GetSaga and Execute
    /// </summary>s
    /// <returns></returns>
    Task<SagaStoredState> Reaction<T>(ISagaIdentifier sagaIdentifier, string stepName, T data);
}