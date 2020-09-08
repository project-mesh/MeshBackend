using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeshBackend.Migrations
{
    public partial class AddMoreAdminProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Admins",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Admins",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Admins",
                nullable: true,
                maxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Admins",
                nullable: true,
                maxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "Users",
                nullable: true);
            
            migrationBuilder.AddColumn<DateTime>(
                name: "Birthday",
                table: "Admins",
                nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Birthday",
                table: "Admins");

        }
    }
}
