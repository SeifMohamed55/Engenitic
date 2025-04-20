namespace GraduationProject.Domain.DTOs
{
    public class RatingStats
    {
        public RatingStats()
        {
            
        }

        public RatingStats(double avg, IDictionary<byte, int> dict)
        {
            AverageRating = avg;
            Breakdown = dict;
        }
        public int TotalCount => Breakdown.Values.Sum();
        public double AverageRating { get; set; }
        public IDictionary<byte, int> Breakdown { get; set; } = null!;
    }
}
