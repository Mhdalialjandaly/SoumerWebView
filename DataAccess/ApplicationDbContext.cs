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
     
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<TeacherCourse> TeacherCourses { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<BalanceTransaction> BalanceTransactions { get; set; }
        public DbSet<TeacherGradeAssignment> TeacherGradeAssignments { get; set; }
        public DbSet<CourseVideo> CourseVideos { get; set; }
        public DbSet<PointsCode> PointsCodes { get; set; }
        public DbSet<CoursePdf> CoursePdfs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });
            // 1. ExamAttempt → Exam (Cascade)
            modelBuilder.Entity<ExamAttempt>()
                .HasOne(a => a.Exam)
                .WithMany(e => e.Attempts)
                .HasForeignKey(a => a.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. ExamQuestion → Exam (Cascade)
            modelBuilder.Entity<ExamQuestion>()
                .HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. ExamOption → ExamQuestion (Cascade)
            modelBuilder.Entity<ExamOption>()
                .HasOne(o => o.ExamQuestion)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.ExamQuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. ExamAnswer → ExamAttempt (Cascade)
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.ExamAttempt)
                .WithMany(t => t.Answers)
                .HasForeignKey(a => a.ExamAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ 5. ExamAnswer → ExamQuestion (NoAction) — حل مشكلة Cascade Cycle
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.ExamQuestion)
                .WithMany()
                .HasForeignKey(a => a.ExamQuestionId)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ 6. ExamAnswer → ExamOption (NoAction) — لتجنب مسار ثالث
            modelBuilder.Entity<ExamAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.NoAction);
            OnModelCreatingPartial(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}