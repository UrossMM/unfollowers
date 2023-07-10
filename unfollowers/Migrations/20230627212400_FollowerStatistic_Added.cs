using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace unfollowers.Migrations
{
    /// <inheritdoc />
    public partial class FollowerStatistic_Added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowersStatistics",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InitialState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnfollowMe = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowersStatistics", x => x.Username);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowersStatistics");
        }
    }
}
