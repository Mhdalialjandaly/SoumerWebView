using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace DataAccess
{
    public partial class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public ApplicationDbContext() { }


     
        DbSet<Teacher> Teachers { get; set; }
        DbSet<Course> Courses { get; set; }
        DbSet<TeacherCourse> TeacherCourses { get; set; }
        DbSet<CourseRegistration> CourseRegistrations { get; set; }
        DbSet<Balance> Balances { get; set; }
        DbSet<BalanceTransaction> BalanceTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserRole<string>>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            OnModelCreatingPartial(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}