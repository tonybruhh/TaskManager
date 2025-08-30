using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Domain;

namespace TaskManager.Api.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>(b =>
        {
            b.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(t => t.UserId);
            b.HasIndex(t => new { t.UserId, t.IsCompleted });
            b.HasIndex(t => new { t.UserId, t.DueDateUtc }).IsDescending(false, true);
        });
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        foreach (var e in ChangeTracker.Entries<TaskItem>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedAt = now;
            }
        }

        return await base.SaveChangesAsync(ct);
    }
}