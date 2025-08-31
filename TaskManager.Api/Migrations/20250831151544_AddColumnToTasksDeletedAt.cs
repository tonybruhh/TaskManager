using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnToTasksDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION set_deleted_at()
                RETURNS trigger AS $$
                BEGIN
                IF TG_OP = 'INSERT' AND NEW."IsDeleted" AND NEW."DeletedAt" IS NULL THEN
                    NEW."DeletedAt" := timezone('utc', now());
                END IF;

                IF NEW."IsDeleted" IS DISTINCT FROM OLD."IsDeleted" THEN
                    IF NEW."IsDeleted" THEN
                        NEW."DeletedAt" := timezone('utc', now());
                    ELSE
                        NEW."DeletedAt" := null;
                    END IF;
                END IF;
                RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            """);

            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS trg_tasks_deleted_at ON "Tasks";
                CREATE TRIGGER trg_tasks_deleted_at
                BEFORE INSERT OR UPDATE ON "Tasks"
                FOR EACH ROW
                EXECUTE FUNCTION set_deleted_at();
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tasks");

            
            migrationBuilder.Sql("""DROP TRIGGER IF EXISTS trg_tasks_deleted_at ON "Tasks";""");
            migrationBuilder.Sql("""DROP FUNCTION IF EXISTS set_deleted_at();""");
        }
    }
}
