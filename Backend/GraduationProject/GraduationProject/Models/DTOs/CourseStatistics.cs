namespace GraduationProject.Models.DTOs
{
    public class CourseStatistics
    {
        public CourseStatistics() 
        {
            
        }

        public IEnumerable<string> UserEmails { get; set; } = null!;
        public int TotalEnrollments { get; set; }
        public int TotalCompleted { get; set; }

    }
}
