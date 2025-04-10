using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingQuizDescriptionAndQuizIndexOnPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quizes_Position",
                table: "Quizes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quizes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Quizes_CourseId_Position",
                table: "Quizes",
                columns: new[] { "CourseId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quizes_CourseId_Position",
                table: "Quizes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quizes");

            migrationBuilder.CreateIndex(
                name: "IX_Quizes_Position",
                table: "Quizes",
                column: "Position");
        }
    }
}
