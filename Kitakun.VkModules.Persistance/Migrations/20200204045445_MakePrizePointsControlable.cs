using Microsoft.EntityFrameworkCore.Migrations;

namespace Kitakun.VkModules.Persistance.Migrations
{
    public partial class MakePrizePointsControlable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentPrice",
                table: "GroupSettings",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LikePrice",
                table: "GroupSettings",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "RepostPrice",
                table: "GroupSettings",
                nullable: false,
                defaultValue: 3);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentPrice",
                table: "GroupSettings");

            migrationBuilder.DropColumn(
                name: "LikePrice",
                table: "GroupSettings");

            migrationBuilder.DropColumn(
                name: "RepostPrice",
                table: "GroupSettings");
        }
    }
}
