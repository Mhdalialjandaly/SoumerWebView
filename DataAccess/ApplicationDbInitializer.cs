using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public static class ApplicationDbInitializer
    {
        public static void SeedData(this ModelBuilder modelBuilder)
        {

            var fixedDate = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            //"Admin@123"
            const string fixedPasswordHash = "AQAAAAIAAYagAAAAEIJPo4WuBKtpsZhjP/uYwhUixPJs+RLJFHwMaU12CUPdcLTVbv9T0ODgOKvSMnkITg==";


            modelBuilder.Entity<IdentityRole>()
              .HasData(new List<IdentityRole>
              {
                    new() { Id = "2", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1" },
                    new() { Id = "3", Name = "Teacher", NormalizedName = "TEACHER", ConcurrencyStamp = "2" },
                    new() { Id = "4", Name = "Employee", NormalizedName = "EMPLOYEE", ConcurrencyStamp = "3" },
                    new() { Id = "5", Name = "FinEmployee", NormalizedName = "FINEMPLOYEE", ConcurrencyStamp = "4" },
                    new() { Id = "6", Name = "HREmployee", NormalizedName = "HREMPLOYEE", ConcurrencyStamp = "5" },
                    new() { Id = "7", Name = "STUEmployee", NormalizedName = "STUEMPLOYEE", ConcurrencyStamp = "6" }
              });


            modelBuilder.Entity<User>().HasData(new User
            {
                Id = "1",
                UserName = "admin@system.com",
                NormalizedUserName = "ADMIN@SYSTEM.COM",
                Email = "admin@system.com",
                NormalizedEmail = "ADMIN@SYSTEM.COM",
                EmailConfirmed = true,
                PasswordHash = fixedPasswordHash, 
                SecurityStamp = "FIXED_STAMP_12345",
                ConcurrencyStamp = "FIXED_CONCURRENCY_12345",
                Description = "مدير النظام",
                IsActive = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                CreatedAt = fixedDate
            });


            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "1", RoleId = "2" }
            );
        }
    }
}