namespace AuthService.Domain.Common;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
