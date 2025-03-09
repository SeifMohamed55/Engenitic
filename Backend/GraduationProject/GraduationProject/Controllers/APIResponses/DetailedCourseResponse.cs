using GraduationProject.Models.DTOs;
using GraduationProject.Models;
using AngleSharp.Text;

namespace GraduationProject.Controllers.APIResponses
{
    public class CourseDetailsResponse
    {
        public CourseDetailsResponse(Course course)
        {
            Id = course.Id;
            Title = course.Title;
            Code = course.Code;
            Stages = course.Stages;
            Description =  course.Description;
            InstructorName = course.Instructor.FullName;
            InstructorEmail = course.Instructor.Email;
            InstructorPhone = course.Instructor.PhoneNumber;
            Requirements = course.Requirements;
            Image = new() { ImageURL = course.FileHash.PublicId, Name = "", Hash = course.FileHash.Hash };
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Code { get; set; }
        public string Description { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string InstructorEmail { get; set; } = null!;
        public string? InstructorPhone { get; set; }
        public string Requirements { get; set; } = null!;
        public int Stages { get; set; }
        public ImageMetadata Image { get; set; }
    }
}
