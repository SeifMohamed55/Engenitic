using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingUserEnrollmentIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserEnrollments_CourseId",
                table: "UserEnrollments");

            migrationBuilder.CreateIndex(
                name: "IX_UserEnrollments_CourseId_UserId",
                table: "UserEnrollments",
                columns: new[] { "CourseId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserEnrollments_CourseId_UserId",
                table: "UserEnrollments");

            migrationBuilder.CreateIndex(
                name: "IX_UserEnrollments_CourseId",
                table: "UserEnrollments",
                column: "CourseId");
        }
    }
}
