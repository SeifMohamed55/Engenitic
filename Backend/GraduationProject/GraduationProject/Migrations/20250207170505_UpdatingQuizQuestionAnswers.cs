using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingQuizQuestionAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_QuizAnswers_AnswerId",
                table: "QuizQuestions");

            migrationBuilder.DropIndex(
                name: "IX_QuizQuestions_AnswerId",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerId",
                table: "QuizQuestions");

            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "QuizAnswers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAnswers_QuestionId",
                table: "QuizAnswers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuestionId",
                table: "QuizAnswers",
                column: "QuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAnswers_QuizQuestions_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropIndex(
                name: "IX_QuizAnswers_QuestionId",
                table: "QuizAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "QuizAnswers");

            migrationBuilder.AddColumn<int>(
                name: "AnswerId",
                table: "QuizQuestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_AnswerId",
                table: "QuizQuestions",
                column: "AnswerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_QuizAnswers_AnswerId",
                table: "QuizQuestions",
                column: "AnswerId",
                principalTable: "QuizAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
