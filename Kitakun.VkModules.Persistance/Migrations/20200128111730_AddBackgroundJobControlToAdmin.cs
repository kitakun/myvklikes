using Microsoft.EntityFrameworkCore.Migrations;

namespace Kitakun.VkModules.Persistance.Migrations
{
    public partial class AddBackgroundJobControlToAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "BackgroundJobType",
                table: "GroupSettings",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "RecuringBackgroundJobId",
                table: "GroupSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundJobType",
                table: "GroupSettings");

            migrationBuilder.DropColumn(
                name: "RecuringBackgroundJobId",
                table: "GroupSettings");
        }
    }
}
