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

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DueDate { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; private set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    protected TaskItem() { }

    public TaskItem(string title, Guid userId, string? description = null, DateTimeOffset? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required", nameof(title));
        if (userId == default) throw new ArgumentException("UserId is required", nameof(userId));

        Id = Guid.NewGuid();
        Title = title.Trim();
        UserId = userId;
        Description = description?.Trim();
        DueDate = dueDate;
        IsCompleted = false;
    }

    public static TaskItem CreateStub(Guid id)
    {
        return new TaskItem() { Id = id };
    }
}
