using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace MeshBackend.Migrations
{
    public partial class CreateDevelop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Users",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorPreference",
                table: "Users",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LayoutPreference",
                table: "Users",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevealedPreference",
                table: "Users",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Finished",
                table: "Subtasks",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(short),
                oldType: "bit",
                oldDefaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Projects",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Publicity",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AccessCount",
                table: "Cooperations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Develops",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    ProjectId = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UpdatedTime = table.Column<DateTime>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Develops", x => new { x.UserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_Develops_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Develops_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Develops_ProjectId",
                table: "Develops",
                column: "ProjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Develops");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ColorPreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LayoutPreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RevealedPreference",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Publicity",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AccessCount",
                table: "Cooperations");

            migrationBuilder.AlterColumn<short>(
                name: "Finished",
                table: "Subtasks",
                type: "bit",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(bool),
                oldDefaultValue: false);
        }
    }
}
