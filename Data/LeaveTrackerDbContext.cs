using Microsoft.EntityFrameworkCore;
using LeaveTracker.Api.Models;

namespace LeaveTracker.Api.Data;

public class LeaveTrackerDbContext : DbContext
{
    public LeaveTrackerDbContext(DbContextOptions<LeaveTrackerDbContext> options) : base(options) { }

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<ApprovalNote> ApprovalNotes => Set<ApprovalNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalNote>()
            .HasOne<LeaveRequest>()
            .WithMany()
            .HasForeignKey(n => n.LeaveRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
