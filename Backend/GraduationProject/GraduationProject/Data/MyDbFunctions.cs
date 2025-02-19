using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data
{
    public static class MyDbFunctions
    {
        [DbFunction("short_description", Schema = "public")]
        public static string ShortDescription(string Description) => throw new NotImplementedException();
    }

}
