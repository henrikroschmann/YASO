namespace YASO.Domain;

public abstract record Entity
{
    protected Guid Id { get; set; }
    public SagaStatus Status { get; private set; } = SagaStatus.Pending;
    internal virtual void MarkAsCompleted() => Status = SagaStatus.Success;
    internal void MarkAsFailed() => Status = SagaStatus.Failure;
}