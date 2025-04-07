using GraduationProject.Domain.Models;

namespace GraduationProject.Domain.DTOs
{
    public class EnrollmentDTO
    {
        public EnrollmentDTO() { }

        public int Id { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int CurrentStage { get; set; }
        public int TotalStages { get; set; }
        public bool IsCompleted { get; set; }
        public float Progress { get; set; }
        public CourseDTO Course { get; set; } = null!;
        public int CourseId { get; set; }

    }
}
