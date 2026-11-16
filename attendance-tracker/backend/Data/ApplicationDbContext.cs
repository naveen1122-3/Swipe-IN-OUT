using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<SwipeLog> SwipeLogs { get; set; }

        public DbSet<AttendanceSession> AttendanceSessions { get; set; }

        public DbSet<TopUpRequest> TopUpRequests { get; set;}

        public DbSet<ApprovalConfiguration> ApprovalConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApprovalConfiguration>()
                .HasIndex(configuration => new
                {
                    configuration.ManagerId,
                    configuration.ProjectId
                })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}