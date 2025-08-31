using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesOnTasksDateColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_CompletedAt",
                table: "Tasks",
                columns: new[] { "UserId", "CompletedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_CreatedAt",
                table: "Tasks",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_UserId_UpdatedAt",
                table: "Tasks",
                columns: new[] { "UserId", "UpdatedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_CompletedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_CreatedAt",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_UserId_UpdatedAt",
                table: "Tasks");
        }
    }
}
