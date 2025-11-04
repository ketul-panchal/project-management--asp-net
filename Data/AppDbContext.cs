using Microsoft.EntityFrameworkCore;
using PMS.Models;

namespace PMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }   // ✅ NEW

        public DbSet<TeamMember> TeamMembers { get; set; } 

        // (legacy alias if you really need it elsewhere)
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // AppUser unique email
            b.Entity<AppUser>()
             .HasIndex(u => u.Email)
             .IsUnique();

            // ✅ TaskItem enum-as-string for readability
            b.Entity<TaskItem>()
             .Property(t => t.Status)
             .HasConversion<string>()
             .HasMaxLength(20);

            b.Entity<TaskItem>()
             .Property(t => t.Priority)
             .HasConversion<string>()
             .HasMaxLength(20);

            // ✅ FK to Project
            b.Entity<TaskItem>()
             .HasOne(t => t.Project)
             .WithMany()
             .HasForeignKey(t => t.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
