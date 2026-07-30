using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentInsights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseSemester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_UserId_Name",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "Courses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId_Name_Semester",
                table: "Courses",
                columns: new[] { "UserId", "Name", "Semester" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_UserId_Name_Semester",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId_Name",
                table: "Courses",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
