using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
            b.HasIndex(t => new { t.UserId, t.DueDate }).IsDescending(false, true);
            b.HasIndex(t => new { t.UserId, t.CreatedAt }).IsDescending(false, true);
            b.HasIndex(t => new { t.UserId, t.UpdatedAt }).IsDescending(false, true);
            b.HasIndex(t => new { t.UserId, t.CompletedAt }).IsDescending(false, true);

            b.HasQueryFilter(t => !t.IsDeleted);

            b.Property<uint>("xmin").IsRowVersion();

            b.Property(p => p.CreatedAt)
                .HasDefaultValueSql("TIMEZONE('UTC', NOW())")
                .ValueGeneratedOnAdd();

            b.Property(p => p.UpdatedAt)
                .ValueGeneratedOnUpdate()
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        });
    }
}