using Microsoft.EntityFrameworkCore.Migrations;

namespace Kitakun.VkModules.Persistance.Migrations
{
    public partial class PreventMultipleSameJobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastRunnedJobId",
                table: "GroupSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRunnedJobId",
                table: "GroupSettings");
        }
    }
}
