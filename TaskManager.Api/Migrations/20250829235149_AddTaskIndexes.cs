using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_DueDateUtc",
                table: "Tasks",
                columns: new[] { "UserId", "DueDateUtc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_IsCompleted",
                table: "Tasks",
                columns: new[] { "UserId", "IsCompleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_DueDateUtc",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_IsCompleted",
                table: "Tasks");
        }
    }
}
