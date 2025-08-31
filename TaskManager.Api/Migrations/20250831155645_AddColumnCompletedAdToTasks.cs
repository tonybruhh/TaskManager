using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnCompletedAdToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_completed_at()
                RETURNS trigger AS $$
                BEGIN
                IF TG_OP = 'INSERT' AND NEW."IsCompleted" AND NEW."IsCompleted" IS NULL THEN
                    NEW."CompletedAt" := timezone('utc', now());
                    RETURN NEW;
                END IF;

                IF NEW."IsCompleted" IS DISTINCT FROM OLD."IsCompleted" THEN
                    IF NEW."IsCompleted" THEN
                        NEW."CompletedAt" := timezone('utc', now());
                    ELSE
                        NEW."CompletedAt" := null;
                    END IF;
                END IF;
                RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            """);

            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_tasks_completed_at ON "Tasks";
                CREATE TRIGGER trg_tasks_completed_at
                BEFORE INSERT OR UPDATE ON "Tasks"
                FOR EACH ROW
                EXECUTE FUNCTION set_completed_at();
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_tasks_completed_at ON "Tasks";""");
            migrationBuilder.Sql("""DROP FUNCTION IF EXISTS set_completed_at();""");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Tasks");
        }
    }
}
