using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeAllDateTimeColumnsDateTimeOffsetAndAddColumnUpdateAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DueDateUtc",
                table: "Tasks",
                newName: "DueDate");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_UserId_DueDateUtc",
                table: "Tasks",
                newName: "IX_Tasks_UserId_DueDate");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "TIMEZONE('UTC', NOW())");

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_updated_at()
                RETURNS trigger AS $$
                BEGIN
                NEW."UpdatedAt" := timezone('utc', now());
                RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            """);

            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_tasks_updated_at ON "Tasks";
                CREATE TRIGGER trg_tasks_updated_at
                BEFORE UPDATE ON "Tasks"
                FOR EACH ROW
                EXECUTE FUNCTION set_updated_at();
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "DueDate",
                table: "Tasks",
                newName: "DueDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_Tasks_UserId_DueDate",
                table: "Tasks",
                newName: "IX_Tasks_UserId_DueDateUtc");

            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_tasks_updated_at ON "Tasks";""");
            migrationBuilder.Sql("""DROP FUNCTION IF EXISTS set_updated_at();""");
        }
    }
}
