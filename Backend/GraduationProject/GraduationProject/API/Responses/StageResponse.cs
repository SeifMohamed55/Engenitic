using GraduationProject.Domain.DTOs;

namespace GraduationProject.API.Responses
{
    public class StageResponse : QuizDTO
    {
        public StageResponse(ReviewDTO? reviewDTO, QuizDTO quizDTO, int latestStage, float progress)
        {
            this.Id = quizDTO.Id;
            this.Title = quizDTO.Title;
            this.Position = quizDTO.Position;
            this.Questions = quizDTO.Questions;
            this.LatestStage = latestStage;
            this.VideoUrl = quizDTO.VideoUrl;
            this.Description = quizDTO.Description;
            this.Progress = progress;
            this.ReviewDTO = reviewDTO;
        }

        public int LatestStage { get; set; }
        public float Progress { get; set; }
        public ReviewDTO? ReviewDTO { get; set; } = null!;
    }
}
