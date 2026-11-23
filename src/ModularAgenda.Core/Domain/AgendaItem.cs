namespace ModularAgenda.Core.Domain;

public abstract class AgendaItem
{
    public Guid Id { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    protected AgendaItem()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        MarkUpdated();
    }

    protected void MarkUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public abstract string Summary { get; }
}
