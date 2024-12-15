namespace YASO.Domain;

public class SagaStoredState
{
    public Guid Id { get; set; }
    public SagaStatus Status { get; set; }
    public Dictionary<string, SagaStatus> StepStatuses { get; set; } = [];
}
