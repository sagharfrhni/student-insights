using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentInsights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationIndexesAndLearningActivityCompletionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedAtUtc",
                table: "LearningActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Type_SourceId",
                table: "Notifications",
                columns: new[] { "UserId", "Type", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Type_SourceId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LastCompletedAtUtc",
                table: "LearningActivities");
        }
    }
}
