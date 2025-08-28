using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Api.Domain;

public class TaskItem
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; private set; } = null!;

    [MaxLength(2000)]
    public string? Description { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? DueDateUtc { get; private set; }

    [Required]
    public string UserId { get; private set; } = null!;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; private set; }

    protected TaskItem() { }

    public TaskItem(string title, string userId, string? description = null, DateTime? dueAt = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required", nameof(userId));

        Id = Guid.NewGuid();
        Title = title.Trim();
        UserId = userId;
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        DueDateUtc = dueAt;
        IsCompleted = false;
    }
}
