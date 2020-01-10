using Microsoft.EntityFrameworkCore.Migrations;

namespace Kitakun.VkModules.Persistance.Migrations
{
    public partial class AddAppTokenToSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupAppToken",
                table: "GroupSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupAppToken",
                table: "GroupSettings");
        }
    }
}
