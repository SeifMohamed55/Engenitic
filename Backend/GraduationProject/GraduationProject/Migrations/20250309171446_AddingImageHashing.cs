using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class AddingImageHashing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageSrc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Courses");

            migrationBuilder.AddColumn<int>(
                name: "HashId",
                table: "Courses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FileHash",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Hash = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileHash", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUserFileHash",
                columns: table => new
                {
                    FileHashesId = table.Column<int>(type: "integer", nullable: false),
                    UsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserFileHash", x => new { x.FileHashesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AppUserFileHash_FileHash_FileHashesId",
                        column: x => x.FileHashesId,
                        principalTable: "FileHash",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppUserFileHash_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_HashId",
                table: "Courses",
                column: "HashId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUserFileHash_UsersId",
                table: "AppUserFileHash",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_FileHash_PublicId",
                table: "FileHash",
                column: "PublicId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_FileHash_HashId",
                table: "Courses",
                column: "HashId",
                principalTable: "FileHash",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_FileHash_HashId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "AppUserFileHash");

            migrationBuilder.DropTable(
                name: "FileHash");

            migrationBuilder.DropIndex(
                name: "IX_Courses_HashId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "HashId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "ImageSrc",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Courses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
