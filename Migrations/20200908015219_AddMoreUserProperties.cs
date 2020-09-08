using Microsoft.EntityFrameworkCore.Migrations;

namespace MeshBackend.Migrations
{
    public partial class AddMoreUserProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskFeeds",
                table: "TaskFeeds");

            migrationBuilder.DropIndex(
                name: "IX_TaskFeeds_UserId",
                table: "TaskFeeds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BulletinFeeds",
                table: "BulletinFeeds");

            migrationBuilder.DropIndex(
                name: "IX_BulletinFeeds_UserId",
                table: "BulletinFeeds");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Users",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Users",
                maxLength: 100,
                nullable: true);
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskFeeds",
                table: "TaskFeeds",
                columns: new[] { "UserId", "TaskId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BulletinFeeds",
                table: "BulletinFeeds",
                columns: new[] { "UserId", "BulletinId" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskFeeds_TaskId",
                table: "TaskFeeds",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinFeeds_BulletinId",
                table: "BulletinFeeds",
                column: "BulletinId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TaskFeeds",
                table: "TaskFeeds");

            migrationBuilder.DropIndex(
                name: "IX_TaskFeeds_TaskId",
                table: "TaskFeeds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BulletinFeeds",
                table: "BulletinFeeds");

            migrationBuilder.DropIndex(
                name: "IX_BulletinFeeds_BulletinId",
                table: "BulletinFeeds");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaskFeeds",
                table: "TaskFeeds",
                column: "TaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BulletinFeeds",
                table: "BulletinFeeds",
                column: "BulletinId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskFeeds_UserId",
                table: "TaskFeeds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BulletinFeeds_UserId",
                table: "BulletinFeeds",
                column: "UserId");
        }
    }
}
