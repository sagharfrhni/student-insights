using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentInsights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicatePreventionIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearningActivities_CourseId",
                table: "LearningActivities");

            migrationBuilder.DropIndex(
                name: "IX_Goals_UserId",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CourseId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserId",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_LearningActivities_CourseId_Title_DueDateUtc",
                table: "LearningActivities",
                columns: new[] { "CourseId", "Title", "DueDateUtc" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId_RelatedActivityId_ProjectDeadlineUnique",
                table: "Goals",
                columns: new[] { "UserId", "RelatedActivityId" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Type] = 'ProjectDeadline'");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId_Type_GpaUnique",
                table: "Goals",
                columns: new[] { "UserId", "Type" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Type] = 'GradePointAverage'");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CourseId_Title_ExamDateUtc",
                table: "Exams",
                columns: new[] { "CourseId", "Title", "ExamDateUtc" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId_Name",
                table: "Courses",
                columns: new[] { "UserId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearningActivities_CourseId_Title_DueDateUtc",
                table: "LearningActivities");

            migrationBuilder.DropIndex(
                name: "IX_Goals_UserId_RelatedActivityId_ProjectDeadlineUnique",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Goals_UserId_Type_GpaUnique",
                table: "Goals");

            migrationBuilder.DropIndex(
                name: "IX_Exams_CourseId_Title_ExamDateUtc",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UserId_Name",
                table: "Courses");

            migrationBuilder.CreateIndex(
                name: "IX_LearningActivities_CourseId",
                table: "LearningActivities",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId",
                table: "Goals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_CourseId",
                table: "Exams",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UserId",
                table: "Courses",
                column: "UserId");
        }
    }
}
