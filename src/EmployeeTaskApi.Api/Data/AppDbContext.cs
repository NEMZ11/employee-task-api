using EmployeeTaskApi.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTaskApi.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EmployeeTaskItem> Tasks => Set<EmployeeTaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FullName).HasMaxLength(160);
            entity.Property(user => user.Email).HasMaxLength(220);
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<EmployeeTaskItem>(entity =>
        {
            entity.Property(task => task.Title).HasMaxLength(180);
            entity.Property(task => task.Description).HasMaxLength(2000);
            entity.Property(task => task.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasOne(task => task.CreatedBy)
                .WithMany(user => user.CreatedTasks)
                .HasForeignKey(task => task.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(task => task.AssignedTo)
                .WithMany(user => user.AssignedTasks)
                .HasForeignKey(task => task.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
