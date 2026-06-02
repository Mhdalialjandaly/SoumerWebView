using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class newmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIJPo4WuBKtpsZhjP/uYwhUixPJs+RLJFHwMaU12CUPdcLTVbv9T0ODgOKvSMnkITg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENQkDyycIUJm60T/4IcwVcKX2kl0jPBjlH8QdlXPTEwql8ASanOeRRQ5ML9l0t9S/A==");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "BalanceId", "ConcurrencyStamp", "CreatedAt", "DeletedAt", "DeletedBy", "Description", "Email", "EmailConfirmed", "IsActive", "LastLogin", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifiedBy", "NormalizedEmail", "NormalizedUserName", "Password", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "2", 0, null, "FIXED_CONCURRENCY_12345", new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "مدير النظام", "testuser@diditdev.com", true, true, null, true, null, null, null, "TESTUSER@DIDITDEV.COM", "TESTUSER@DIDITDEV.COM", null, "AQAAAAIAAYagAAAAEIJPo4WuBKtpsZhjP/uYwhUixPJs+RLJFHwMaU12CUPdcLTVbv9T0ODgOKvSMnkITg==", null, false, "FIXED_STAMP_12345", false, "testuser@diditdev.com" });
        }
    }
}
