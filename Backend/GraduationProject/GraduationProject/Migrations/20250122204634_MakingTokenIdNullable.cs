using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class MakingTokenIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "RefreshTokenId",
                table: "Users",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "Users",
                column: "RefreshTokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "RefreshTokenId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RefreshTokens_RefreshTokenId",
                table: "Users",
                column: "RefreshTokenId",
                principalTable: "RefreshTokens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
