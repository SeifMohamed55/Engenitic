using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class FinalUpdatesToQuizDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Users_UserId",
                table: "UserAnswers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserAnswers",
                newName: "UserQuizAttemptId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_UserId",
                table: "UserAnswers",
                newName: "IX_UserAnswers_UserQuizAttemptId");

            migrationBuilder.RenameColumn(
                name: "QuestionId",
                table: "QuizAnswers",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "AnswerId",
                table: "QuizQuestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "QuizAnswers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "UserEnrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserEnrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserScore = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    QuizId = table.Column<int>(type: "integer", nullable: false),
                    UserEnrollmentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQuizAttempts_Quizes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserQuizAttempts_UserEnrollments_UserEnrollmentId",
                        column: x => x.UserEnrollmentId,
                        principalTable: "UserEnrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_AnswerId",
                table: "QuizQuestions",
                column: "AnswerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserEnrollments_CourseId",
                table: "UserEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEnrollments_UserId",
                table: "UserEnrollments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttempts_QuizId",
                table: "UserQuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizAttempts_UserEnrollmentId",
                table: "UserQuizAttempts",
                column: "UserEnrollmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_QuizAnswers_AnswerId",
                table: "QuizQuestions",
                column: "AnswerId",
                principalTable: "QuizAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserQuizAttempts_UserQuizAttemptId",
                table: "UserAnswers",
                column: "UserQuizAttemptId",
                principalTable: "UserQuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_QuizAnswers_AnswerId",
                table: "QuizQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_UserQuizAttempts_UserQuizAttemptId",
                table: "UserAnswers");

            migrationBuilder.DropTable(
                name: "UserQuizAttempts");

            migrationBuilder.DropTable(
                name: "UserEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_AnswerId",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerId",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "UserQuizAttemptId",
                table: "UserAnswers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserAnswers_UserQuizAttemptId",
                table: "UserAnswers",
                newName: "IX_UserAnswers_UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "QuizAnswers",
                newName: "QuestionId");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "QuizAnswers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuestionId",
                table: "QuizAnswers",
                column: "QuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Users_UserId",
                table: "UserAnswers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
