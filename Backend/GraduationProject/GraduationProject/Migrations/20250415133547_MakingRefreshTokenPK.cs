using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class MakingRefreshTokenPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                columns: new[] { "DeviceId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "DeviceId");
        }
    }
}
