using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentInsights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudyLogNotesLengthAndAddCourseIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StudyLogs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyLogs_UserId_CourseId",
                table: "StudyLogs",
                columns: new[] { "UserId", "CourseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudyLogs_UserId_CourseId",
                table: "StudyLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "StudyLogs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
