using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace unfollowers.Migrations
{
    /// <inheritdoc />
    public partial class FollowersStatistic_Updated_Password_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "FollowersStatistics",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "FollowersStatistics");
        }
    }
}
