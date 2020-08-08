using Microsoft.EntityFrameworkCore.Migrations;

namespace MeshBackend.Migrations
{
    public partial class CreatePasswordSalt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                table: "Users",
                maxLength: 70,
                nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "PasswordSalt",
                table: "Admins",
                maxLength: 70,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Admins");
            
        }
    }
}
