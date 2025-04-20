namespace GraduationProject.Domain.DTOs
{
    public class RatingStatsDTO
    {
        public RatingStatsDTO()
        {
            
        }

        public RatingStatsDTO(float avg, IDictionary<byte, CourseStatDTO> dict)
        {
            AverageRating = avg;
            Breakdown = dict;
        }
        public int TotalCount => Breakdown.Values.Select(x=> x.Count).Sum();
        public float AverageRating { get; set; }
        public IDictionary<byte, CourseStatDTO> Breakdown { get; set; } = null!;
    }
}
