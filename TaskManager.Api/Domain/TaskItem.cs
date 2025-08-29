using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Domain;

public class TaskItem
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; }

    public DateTime? DueDateUtc { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; private set; }

    protected TaskItem() { }

    public TaskItem(string title, Guid userId, string? description = null, DateTime? dueDateUtc = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (userId == default) throw new ArgumentException("UserId is required", nameof(userId));

        Id = Guid.NewGuid();
        Title = title.Trim();
        UserId = userId;
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        DueDateUtc = dueDateUtc;
        IsCompleted = false;
    }
}
