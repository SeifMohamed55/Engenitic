using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class ModifyingLatestJWTAccessTokenToJti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatestJwtAccessToken",
                table: "RefreshTokens",
                newName: "LatestJwtAccessTokenJti");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LatestJwtAccessTokenJti",
                table: "RefreshTokens",
                newName: "LatestJwtAccessToken");
        }
    }
}
