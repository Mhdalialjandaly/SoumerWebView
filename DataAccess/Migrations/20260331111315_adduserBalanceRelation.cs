using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class adduserBalanceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BalanceId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                column: "BalanceId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BalanceId",
                table: "AspNetUsers",
                column: "BalanceId",
                unique: true,
                filter: "[BalanceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Balances_BalanceId",
                table: "AspNetUsers",
                column: "BalanceId",
                principalTable: "Balances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Balances_BalanceId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BalanceId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BalanceId",
                table: "AspNetUsers");
        }
    }
}
