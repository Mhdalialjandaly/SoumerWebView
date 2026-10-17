using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SeedStudentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "1", "1", "Student", "STUDENT" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AvatarUrl", "BalanceId", "ConcurrencyStamp", "CreatedAt", "DeletedAt", "DeletedBy", "Description", "Email", "EmailConfirmed", "FullName", "IsActive", "LastLogin", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifiedBy", "NormalizedEmail", "NormalizedUserName", "Password", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "2", 0, null, null, "FIXED_CONCURRENCY_12345", new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "مدير النظام", "student@system.com", true, null, true, null, true, null, null, null, "Student@SYSTEM.COM", "Student@SYSTEM.COM", null, "AQAAAAIAAYagAAAAEIJPo4WuBKtpsZhjP/uYwhUixPJs+RLJFHwMaU12CUPdcLTVbv9T0ODgOKvSMnkITg==", null, false, "FIXED_STAMP_12345", false, "Student" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "2" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "2" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2");
        }
    }
}
