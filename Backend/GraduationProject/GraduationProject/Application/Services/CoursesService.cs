using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GraduationProject.Application.Services
{

    public interface ICoursesService
    {
        Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index);
        Task<PaginatedList<CourseDTO>> SearchOnPageOfCourses(string search, int index);
        Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index);
        Task<CourseDetailsResponse?> GetCourseDetailsById(int id);
        Task<List<TagDTO>> GetAllTagsAsync();
        Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index);
        Task<CourseStatistics?> GetCourseStatistics(int courseId);
    }
    public class CoursesService : ICoursesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;

        public CoursesService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinaryService;
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCourses(int index)
        {
            var courses = await _unitOfWork.CourseRepo.GetPageOfCourses(index);
            UpdateImagesUrlInList(courses);
            return courses;
        }

        public async Task<PaginatedList<CourseDTO>> SearchOnPageOfCourses(string search, int index)
        {
            var courses = await _unitOfWork.CourseRepo.GetPageOfCoursesBySearching(search, index);
            UpdateImagesUrlInList(courses);
            return courses;
        }

        public async Task<PaginatedList<CourseDTO>> GetPageOfCoursesByTag(string tag, int index)
        {
            var courses = await _unitOfWork.CourseRepo.GetPageOfCoursesByTag(tag, index);
            UpdateImagesUrlInList(courses);
            return courses;
        }

        private void UpdateImagesUrlInList(List<CourseDTO> courses)
        {
            Func<string, string> nameFunc = (url) => url.Split('/').LastOrDefault() ?? "";

            courses.ForEach(x =>
            {
                x.Image.ImageURL = _cloudinary.GetImageUrl(x.Image.ImageURL, x.Image.Version);
                x.Image.Name = nameFunc(x.Image.ImageURL);
            });
        }

        public async Task<List<TagDTO>> GetAllTagsAsync()
        {
            return await _unitOfWork.TagsRepo.GetTagsDTOAsync();
        }

        public async Task<CourseDetailsResponse?> GetCourseDetailsById(int id)
        {
            var course = await _unitOfWork.CourseRepo.GetDetailsById(id);
            if (course == null)
                return null;

            course.Image.ImageURL = _cloudinary.GetImageUrl(course.Image.ImageURL, course.Image.Version);
            return course;

        }

        public async Task<PaginatedList<CourseDTO>> GetInstructorCourses(int instructorId, int index)
        {
            var courses = await _unitOfWork.CourseRepo.GetInstructorCourses(instructorId, index);
            UpdateImagesUrlInList(courses);
            return courses;
        }

        public async Task<CourseStatistics?> GetCourseStatistics(int courseId)
        {
            return await _unitOfWork.CourseRepo.GetCourseStatistics(courseId);
        }
    }
}
