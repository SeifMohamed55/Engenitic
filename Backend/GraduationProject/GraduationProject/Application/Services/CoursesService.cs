using CloudinaryDotNet;
using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

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
        Task<CourseDTO> AddCourse(RegisterCourseRequest course);
        Task<CourseDTO> EditCourse(EditCourseRequest course);
        Task DeleteCourse(int courseId);
        Task<ServiceResult<bool>> EditCourseImage(IFormFile image, int courseId);
        Task<int?> GetCourseInstructorId(int courseId);
        Task<EditCourseRequest?> GetCourseWithQuizes(int courseId);

        public class CoursesService : ICoursesService
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICloudinaryService _cloudinary;
            private readonly IUploadingService _uploadingService;

            public CoursesService
                (IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IUploadingService uploadingService)
            {
                _unitOfWork = unitOfWork;
                _cloudinary = cloudinaryService;
                _uploadingService = uploadingService;
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

            public async Task<CourseDTO> AddCourse(RegisterCourseRequest course)
            {
                var defaultCourseHash = await _unitOfWork.FileHashRepo.GetDefaultCourseImageAsync();

                var addedCourse = await _unitOfWork.CourseRepo.MakeCourse(course, defaultCourseHash);
                await _unitOfWork.SaveChangesAsync();

                var imageName = "course_" + addedCourse.Id;

                using var stream = course.Image.OpenReadStream();

                var hash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.CourseImage);

                if (hash == null)
                    hash = defaultCourseHash;

                addedCourse.FileHash = hash;

                await _unitOfWork.SaveChangesAsync();

                var dto = new CourseDTO(addedCourse);

                return dto;
            }

            public async Task<CourseDTO> EditCourse(EditCourseRequest course)
            {
                var addedCourse = await _unitOfWork.CourseRepo.EditCourse(course);
                await _unitOfWork.SaveChangesAsync();

                var resp = new CourseDTO(addedCourse);
                resp.Image.ImageURL = _cloudinary.GetImageUrl(resp.Image.ImageURL, resp.Image.Version);

                return resp;
            }

            public async Task<ServiceResult<bool>> EditCourseImage(IFormFile image, int courseId)
            {
                var addedCourse = await _unitOfWork.CourseRepo.GetCourseWithImageAndInstructor(courseId);
                if (addedCourse == null)
                    return ServiceResult<bool>.Failure("Course not found.");


                var defaultCourseHash = await _unitOfWork.FileHashRepo.GetDefaultCourseImageAsync();

                var imageName = "course_" + addedCourse.Id;

                using var stream = image.OpenReadStream();

                var hash = await _uploadingService.UploadImageAsync(stream, imageName, CloudinaryType.CourseImage);

                if (hash == null)
                    return ServiceResult<bool>.Failure("Image Upload Failed.");


                if (_unitOfWork.FileHashRepo.IsDefaultCourseImageHash(addedCourse.FileHash))
                {
                    addedCourse.FileHash = hash;
                }
                else
                {
                    addedCourse.FileHash.UpdateFromHash(hash);
                }

                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);

            }

            public async Task<int?> GetCourseInstructorId(int courseId)
            {
                return await _unitOfWork.CourseRepo.GetCourseInstructorId(courseId);
            }

            public async Task DeleteCourse(int courseId)
            {
                var course = await _unitOfWork.CourseRepo.GetCourseWithImageAndInstructor(courseId);
                if(course == null)
                    throw new ArgumentNullException("course is not found");

                course.hidden = true;
                await _unitOfWork.SaveChangesAsync();
            }

            public async Task<EditCourseRequest?> GetCourseWithQuizes(int courseId)
            {
                return await _unitOfWork.CourseRepo.GetCourseWithQuizes(courseId);
            }
        }
    }
}
