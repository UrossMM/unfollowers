using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace unfollowers.Migrations
{
    /// <inheritdoc />
    public partial class FollowersStatistic_Updated_Image_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "FollowersStatistics",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "FollowersStatistics");
        }
    }
}
