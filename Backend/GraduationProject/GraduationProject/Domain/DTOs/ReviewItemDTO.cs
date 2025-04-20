namespace GraduationProject.Domain.DTOs
{
    public class CourseStatDTO
    {
        public CourseStatDTO(int count, float percentage)
        {
            Count = count;
            Percentage = percentage;
        }
        public int Count { get; set; }
        public float Percentage { get; set; }
    }
}
