using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AppUserIdentifierToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- 1) DROP ALL FKs that depend on AspNetUsers / AspNetRoles (and Tasks->Users)
            migrationBuilder.DropForeignKey("FK_Tasks_AspNetUsers_UserId", "Tasks");
            migrationBuilder.DropForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUserTokens");
            migrationBuilder.DropForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUserClaims");
            migrationBuilder.DropForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUserLogins");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoleClaims");

            // ---- 2) DROP PKs where PK includes the changing columns
            migrationBuilder.DropPrimaryKey("PK_AspNetUsers", "AspNetUsers");          // PK(Id)
            migrationBuilder.DropPrimaryKey("PK_AspNetRoles", "AspNetRoles");          // PK(Id)
            migrationBuilder.DropPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles");  // PK(UserId, RoleId)
            migrationBuilder.DropPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens");// PK(UserId, LoginProvider, Name)

            // (Optional) If you had a default value on Tasks.CreatedAt you're removing, do it first
            migrationBuilder.DropColumn(name: "CreatedAt", table: "Tasks");

            // ---- 3) ALTER principal keys first
            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUsers"
                ALTER COLUMN "Id" TYPE uuid
                USING "Id"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetRoles"
                ALTER COLUMN "Id" TYPE uuid
                USING "Id"::uuid;
            """);

            // ---- 4) ALTER all dependent columns
            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserClaims"
                ALTER COLUMN "UserId" TYPE uuid
                USING "UserId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserLogins"
                ALTER COLUMN "UserId" TYPE uuid
                USING "UserId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserRoles"
                ALTER COLUMN "UserId" TYPE uuid
                USING "UserId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserRoles"
                ALTER COLUMN "RoleId" TYPE uuid
                USING "RoleId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetRoleClaims"
                ALTER COLUMN "RoleId" TYPE uuid
                USING "RoleId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserTokens"
                ALTER COLUMN "UserId" TYPE uuid
                USING "UserId"::uuid;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Tasks"
                ALTER COLUMN "UserId" TYPE uuid
                USING "UserId"::uuid;
            """);

            // ---- 5) RE-ADD PKs
            migrationBuilder.AddPrimaryKey("PK_AspNetUsers", "AspNetUsers", new[] { "Id" });
            migrationBuilder.AddPrimaryKey("PK_AspNetRoles", "AspNetRoles", new[] { "Id" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles", new[] { "UserId", "RoleId" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens", new[] { "UserId", "LoginProvider", "Name" });

            // ---- 6) RE-ADD FKs with cascade (match your previous behavior)
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ---- 1) DROP FKs
            migrationBuilder.DropForeignKey("FK_Tasks_AspNetUsers_UserId", "Tasks");
            migrationBuilder.DropForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUserTokens");
            migrationBuilder.DropForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUserClaims");
            migrationBuilder.DropForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUserLogins");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetUserRoles");
            migrationBuilder.DropForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoleClaims");

            // ---- 2) DROP PKs that include changing cols
            migrationBuilder.DropPrimaryKey("PK_AspNetUsers", "AspNetUsers");
            migrationBuilder.DropPrimaryKey("PK_AspNetRoles", "AspNetRoles");
            migrationBuilder.DropPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles");
            migrationBuilder.DropPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens");

            // ---- 3) ALTER columns back to text
            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUsers"
                ALTER COLUMN "Id" TYPE text
                USING "Id"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetRoles"
                ALTER COLUMN "Id" TYPE text
                USING "Id"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserClaims"
                ALTER COLUMN "UserId" TYPE text
                USING "UserId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserLogins"
                ALTER COLUMN "UserId" TYPE text
                USING "UserId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserRoles"
                ALTER COLUMN "UserId" TYPE text
                USING "UserId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserRoles"
                ALTER COLUMN "RoleId" TYPE text
                USING "RoleId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetRoleClaims"
                ALTER COLUMN "RoleId" TYPE text
                USING "RoleId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserTokens"
                ALTER COLUMN "UserId" TYPE text
                USING "UserId"::text;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Tasks"
                ALTER COLUMN "UserId" TYPE text
                USING "UserId"::text;
            """);

            // ---- 4) RE-ADD PKs
            migrationBuilder.AddPrimaryKey("PK_AspNetUsers", "AspNetUsers", new[] { "Id" });
            migrationBuilder.AddPrimaryKey("PK_AspNetRoles", "AspNetRoles", new[] { "Id" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserRoles", "AspNetUserRoles", new[] { "UserId", "RoleId" });
            migrationBuilder.AddPrimaryKey("PK_AspNetUserTokens", "AspNetUserTokens", new[] { "UserId", "LoginProvider", "Name" });

            // ---- 5) (Optional) restore Tasks.CreatedAt if you really need it back
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // ---- 6) RE-ADD FKs
            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_UserId",
                table: "Tasks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

    }
}
