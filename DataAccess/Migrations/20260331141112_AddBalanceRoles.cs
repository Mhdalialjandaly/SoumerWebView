using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MyProperty",
                table: "BalanceTransactions",
                newName: "TransactionType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "BalanceTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "BalanceTransactions");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "BalanceTransactions",
                newName: "MyProperty");
        }
    }
}
