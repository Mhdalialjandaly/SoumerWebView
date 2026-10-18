using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddMainCourseToTeacher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainCourseId",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_MainCourseId",
                table: "Teachers",
                column: "MainCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Courses_MainCourseId",
                table: "Teachers",
                column: "MainCourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Courses_MainCourseId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_MainCourseId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "MainCourseId",
                table: "Teachers");
        }
    }
}
