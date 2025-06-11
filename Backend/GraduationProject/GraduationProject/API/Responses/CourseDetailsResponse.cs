﻿using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;

namespace GraduationProject.API.Responses
{
    public class CourseDetailsResponse
    {
        public CourseDetailsResponse(Course course)
        {
            Func<string, string> nameFunc = (url) => url.Split('/').LastOrDefault() ?? "";

            Id = course.Id;
            Title = course.Title;
            Code = course.Code;
            Stages = course.Stages;
            Description = course.Description;
            InstructorName = course.Instructor.FullName;
            InstructorEmail = course.Instructor.Email;
            Requirements = course.Requirements;
            Image = new()
            {
                ImageURL = course.FileHash.PublicId,
                Name = nameFunc(course.FileHash.PublicId),
                Hash = course.FileHash.Hash,
            };
            RatingStats = new RatingStatsDTO();
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Code { get; set; }
        public string Description { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string InstructorEmail { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public int Stages { get; set; }
        public bool IsEnrolled { get; set; } = false;
        public FileMetadata Image { get; set; }
        public RatingStatsDTO RatingStats { get; set; }
    }
}
