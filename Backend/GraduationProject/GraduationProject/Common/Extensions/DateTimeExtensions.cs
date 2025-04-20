namespace GraduationProject.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToTimeAgo(this DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
                return "Just now";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes < 2 ? "" : "s")} ago";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours < 2 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays < 2 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{(timeSpan.TotalDays < 14 ? "" : "s")} ago";

            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{(timeSpan.TotalDays < 60 ? "" : "s")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{(timeSpan.TotalDays < 730 ? "" : "s")} ago";
        }
    }

}
