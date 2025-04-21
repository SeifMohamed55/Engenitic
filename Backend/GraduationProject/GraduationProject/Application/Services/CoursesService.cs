using CloudinaryDotNet;
using GraduationProject.API.Requests;
using GraduationProject.API.Responses;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Net;

namespace GraduationProject.Application.Services
{

    public class CoursesService : ICoursesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinary;
        private readonly IUploadingService _uploadingService;
        private readonly IMediaValidator _mediaValidator;

        public CoursesService
            (IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IUploadingService uploadingService,
            IMediaValidator mediaValidator
            )
        {
            _unitOfWork = unitOfWork;
            _cloudinary = cloudinaryService;
            _uploadingService = uploadingService;
            _mediaValidator = mediaValidator;
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


        public async Task<CourseDetailsResponse?> GetCourseDetailsById(int courseId)
        {
            var course = await _unitOfWork.CourseRepo.GetDetailsById(courseId);
            if (course == null)
                return null;

            course.Image.ImageURL = _cloudinary.GetImageUrl(course.Image.ImageURL, course.Image.Version);

            var stats = await _unitOfWork.ReviewRepository.GetCourseRatingStats(courseId);
            if (stats == null)
                return null;
                
            course.RatingStats = stats;
                
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

        public async Task<ServiceResult<CourseDetailsResponse>> EditCourse(EditCourseRequest courseReq)
        {
            var tasks = new List<(string, Task<bool>)>();
            foreach (string url in courseReq.Quizes.Select(x => x.VideoUrl))
            {
                tasks.Add((url, _mediaValidator.ValidateAsync(url)));
            }

            var userQuizesIds = courseReq.Quizes.Select(x => x.Id).ToHashSet();

            var userQuestionIds = courseReq.Quizes.SelectMany(x => x.Questions)
                .Select(x => x.Id)
                .ToHashSet();

            var usersAnswerIds = courseReq.Quizes.SelectMany(x => x.Questions)
                .SelectMany(x => x.Answers)
                .Select(x => x.Id)
                .ToHashSet();

            var dbQuestionsWithAnswers = await _unitOfWork.CourseRepo.GetQuizesQuestionAndAnswerIds(courseReq.Id);
            if (dbQuestionsWithAnswers == null)
                return ServiceResult<CourseDetailsResponse>.Failure("course quizzes is not found");

            var userQuestionsInDb = dbQuestionsWithAnswers.QuestionIds.IsSupersetOf(userQuestionIds);
            var userAnswersInDb = dbQuestionsWithAnswers.AnswerIds.IsSupersetOf(usersAnswerIds);
            var userQuizzesInDb = dbQuestionsWithAnswers.QuizzesIds.IsSupersetOf(userQuizesIds);


            if (userQuestionsInDb && userAnswersInDb && userQuizzesInDb)
            {
                var quizzesIdsToRemove = dbQuestionsWithAnswers.QuizzesIds.Except(userQuizesIds).ToHashSet();
                var questionsToRemove = dbQuestionsWithAnswers.QuestionIds.Except(userQuestionIds).ToHashSet();
                var answersIdsToRemove = dbQuestionsWithAnswers.AnswerIds.Except(usersAnswerIds).ToHashSet();

                var urlsValid = (await Task.WhenAll(tasks.Select(x => x.Item2))).All(x => x);
                if (!urlsValid)
                {
                    var invalidUrls = tasks.Where(x => !x.Item2.Result).Select(x => x.Item1).ToList();
                    string invalidUrlsString = string.Join('\n', invalidUrls);
                    return ServiceResult<CourseDetailsResponse>.Failure($"Invalid video url/s:\n{invalidUrlsString}");
                }

                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    await _unitOfWork.QuizRepo.ExecuteDeleteAsync(quizzesIdsToRemove);
                    await _unitOfWork.QuizQuestionRepo.ExecuteDeleteAsync(questionsToRemove);
                    await _unitOfWork.QuizAnswerRepo.ExecuteDeleteAsync(answersIdsToRemove);

                    await _unitOfWork.CommitTransactionAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ServiceResult<CourseDetailsResponse>.Failure("Couldn't Update database. Try again later.");
                }


                var dbCourse = await _unitOfWork.CourseRepo.GetCourseWithQuizes(courseReq.Id);
                if (dbCourse == null)
                    return ServiceResult<CourseDetailsResponse>.Failure("course not found");

                var dbQuizzesDict = dbCourse.Quizes
                    .ToDictionary(q => q.Id, q => q);

                var dbQuestionsDict = dbCourse.Quizes.SelectMany(q => q.Questions)
                    .ToDictionary(q => q.Id, q => q);

                var dbAnswersDict = dbCourse.Quizes.SelectMany(q => q.Questions)
                    .SelectMany(q => q.Answers)
                    .ToDictionary(a => a.Id, a => a);

                dbCourse.UpdateFromRequest(courseReq);

                foreach (var quizDto in courseReq.Quizes)
                {
                    if (dbQuizzesDict.TryGetValue(quizDto.Id, out var dbQuiz))
                    {
                        // Update existing quiz
                        dbQuiz.UpdateFromRequest(quizDto);

                        // Update questions
                        foreach (var questionDto in quizDto.Questions)
                        {

                            if (dbQuestionsDict.TryGetValue(questionDto.Id, out var dbQuestion))
                            {
                                dbQuestion.UpdateFromDto(questionDto);

                                // Update answers
                                foreach (var answerDto in questionDto.Answers)
                                {
                                    if (dbAnswersDict.TryGetValue(answerDto.Id, out var dbAnswer))
                                    {
                                        dbAnswer.UpdateFromDto(answerDto);
                                    }
                                    else
                                    {
                                        dbQuestion.Answers.Add(new QuizAnswer(answerDto));
                                    }
                                }
                            }
                            else
                            {
                                dbQuiz.Questions.Add(new QuizQuestion(questionDto));
                            }
                        }
                    }
                    else
                    {
                        // Add new quiz
                        dbCourse.Quizes.Add(new Quiz(quizDto));
                    }
                        
                }

                await _unitOfWork.SaveChangesAsync();

                var resp = await _unitOfWork.CourseRepo.GetDetailsById(courseReq.Id);
                if (resp == null)
                    return ServiceResult<CourseDetailsResponse>.Failure("course not found");

                resp.Image.ImageURL = _cloudinary.GetImageUrl(resp.Image.ImageURL, resp.Image.Version);

                return ServiceResult<CourseDetailsResponse>.Success(resp);
            }
            else
            {
                return ServiceResult<CourseDetailsResponse>
                    .Failure("Invalid Request Questions or answers don't match");
            }


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
            return await _unitOfWork.CourseRepo.GetEditCourseRequestWithQuizes(courseId);
        }

        public async Task<ServiceResult<List<QuizTitleResponse>>> GetQuizesTitles(int courseId)
        {
            try
            {
                var res = await _unitOfWork.QuizRepo.GetQuizesTitle(courseId);
                return ServiceResult<List<QuizTitleResponse>>.Success(res); 
            }
            catch
            {
                return ServiceResult<List<QuizTitleResponse>>.Failure("Error in getting titles");
            }
        }

        public async Task<List<CourseDTO>> GetRandomCourses(int numberOfCourses)
        {
            var courses = await _unitOfWork.CourseRepo.GetRandomCourses(numberOfCourses);
            courses.ForEach(x=> x.Image.ImageURL =
                _cloudinary.GetImageUrl(x.Image.ImageURL, x.Image.Version));

            return courses;
        }
    }
    
}
