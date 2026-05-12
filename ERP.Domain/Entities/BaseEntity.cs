using ERP.Domain.Interfaces;

namespace ERP.Domain.Entities;

public abstract class BaseEntity : ISoftDelete
{
    public int Id { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
