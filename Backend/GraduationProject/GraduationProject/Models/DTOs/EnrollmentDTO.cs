namespace GraduationProject.Models.DTOs
{
    public class EnrollmentDTO
    {
        public EnrollmentDTO(UserEnrollment enrollment)
        {
            Id = enrollment.Id;
            EnrolledAt = enrollment.EnrolledAt;
            CurrentStage = enrollment.CurrentStage;
            IsCompleted = enrollment.IsCompleted;
            TotalStages = enrollment.TotalStages;
            Progress = ((float)CurrentStage / TotalStages) * 100;
            CourseId = enrollment.CourseId;
            Course = new CourseDTO(enrollment.Course);
        }
        public int Id { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int CurrentStage { get; set; }
        public int TotalStages { get; set; }
        public bool IsCompleted { get; set; }
        public float Progress { get; set; }
        public CourseDTO Course { get; set; }
        public int CourseId { get; set; }

    }
}
