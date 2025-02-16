using GraduationProject.Controllers.APIResponses;
using GraduationProject.Models;
using GraduationProject.Models.DTOs;
using GraduationProject.Repositories;
using GraduationProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace GraduationProject.Controllers
{
    // TODO: Add Stages(number of videos and quizes), Requirements and Tags

    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseRepository _coursesRepo;

        public CoursesController(ICourseRepository coursesRepository)
        {
            _coursesRepo = coursesRepository;
        }



        [HttpGet("{index}")]
        public async Task<IActionResult> GetPageOfCourses(int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetPageOfCourses(index);

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse()
                    {
                        Message = "Invalid Page Number",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });
            }

        }


        [HttpGet("search")]
        public async Task<IActionResult> GetCoursesBySearching
                                        ([FromQuery]string search, [FromQuery]int index = 1)
        {
            if (index <= 0)
                return BadRequest(new ErrorResponse()
                {
                    Message = "Invalid Page Number",
                    Code = System.Net.HttpStatusCode.BadRequest,
                });
            try
            {
                var courses = await _coursesRepo.GetPageOfCoursesBySearching(search, index);

                if (courses.TotalPages == 0)
                    return NotFound(new ErrorResponse()
                    {
                        Message = "No Courses Found.",
                        Code = System.Net.HttpStatusCode.NotFound,
                    });

                if (index > courses.TotalPages)
                    return BadRequest(new ErrorResponse
                    {
                        Message = "Invalid Page Number",
                        Code = System.Net.HttpStatusCode.BadRequest,
                    });

                return Ok(new SuccessResponse
                {
                    Message = "Courses Retrieved Successfully.",
                    Data = new PaginatedResponse<CourseDTO>(courses),
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });

            }
        }


        [HttpGet("id/{courseId}")]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            try
            {
                var course = await _coursesRepo.GetById(courseId);

                if (course == null)
                    return NotFound(new ErrorResponse
                    {
                        Message = "Course Not Found.",
                        Code = System.Net.HttpStatusCode.NotFound,
                    });

                return Ok(new SuccessResponse()
                {
                    Message = "Course Retrieved Successfully.",
                    Data = course,
                    Code = System.Net.HttpStatusCode.OK,
                });
            }
            catch
            {
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, new ErrorResponse()
                {
                    Message = "Something Wrong Happened.",
                    Code = System.Net.HttpStatusCode.InternalServerError,
                });
            }
        }


        
        

        [HttpGet("dummy/{index}")]
        // GET: /api/courses/dummy/1
        public IActionResult GetDummyCourses(int index)
        {

            var courses = new List<Course>
            {
                // First 10 courses (already provided)
                new Course { Id = 1, Title = "C# Basics", Code = "C#101", Description = "Learn C# from scratch", Instructor = new AppUser { FullName = "John Doe", Email = "john@example.com", PhoneNumber = "+201142236508" }, ImageUrl = "csharp_basics.jpg" },
                new Course { Id = 2, Title = "ASP.NET Core", Code = "ASP200", Description = "Build APIs with ASP.NET Core", Instructor = new AppUser { FullName = "Jane Smith", Email = "jane@example.com", PhoneNumber = "+201142236509" }, ImageUrl = "aspnet_core.jpg" },
                new Course { Id = 3, Title = "Angular Fundamentals", Code = "ANG101", Description = "Learn the basics of Angular", Instructor = new AppUser { FullName = "David Brown", Email = "david@example.com", PhoneNumber = "+201142236510" }, ImageUrl = "angular_fundamentals.jpg" },
                new Course { Id = 4, Title = "React for Beginners", Code = "REACT101", Description = "Start building React apps", Instructor = new AppUser { FullName = "Alice Johnson", Email = "alice@example.com", PhoneNumber = "+201142236511" }, ImageUrl = "react_basics.jpg" },
                new Course { Id = 5, Title = "Node.js Essentials", Code = "NODE200", Description = "Learn server-side JavaScript", Instructor = new AppUser { FullName = "Bob Martin", Email = "bob@example.com", PhoneNumber = "+201142236512" }, ImageUrl = "nodejs_essentials.jpg" },
                new Course { Id = 6, Title = "Python for Data Science", Code = "PYDS300", Description = "Data Science with Python", Instructor = new AppUser { FullName = "Sarah Lee", Email = "sarah@example.com", PhoneNumber = "+201142236513" }, ImageUrl = "python_datascience.jpg" },
                new Course { Id = 7, Title = "Django Web Development", Code = "DJANGO101", Description = "Build websites with Django", Instructor = new AppUser { FullName = "Tom Wilson", Email = "tom@example.com", PhoneNumber = "+201142236514" }, ImageUrl = "django_web.jpg" },
                new Course { Id = 8, Title = "Java Spring Boot", Code = "JAVA102", Description = "Backend with Spring Boot", Instructor = new AppUser { FullName = "Emma Davis", Email = "emma@example.com", PhoneNumber = "+201142236515" }, ImageUrl = "spring_boot.jpg" },
                new Course { Id = 9, Title = "Flutter Mobile Apps", Code = "FLUTTER101", Description = "Build mobile apps with Flutter", Instructor = new AppUser { FullName = "Chris Evans", Email = "chris@example.com", PhoneNumber = "+201142236516" }, ImageUrl = "flutter_apps.jpg" },
                new Course { Id = 10, Title = "Machine Learning with Python", Code = "MLPY500", Description = "Intro to Machine Learning", Instructor = new AppUser { FullName = "Michael Johnson", Email = "michael@example.com", PhoneNumber = "+201142236517" }, ImageUrl = "ml_python.jpg" },

                // Additional 20 courses
                new Course { Id = 11, Title = "Vue.js Basics", Code = "VUE101", Description = "Learn Vue.js for modern web apps", Instructor = new AppUser { FullName = "Ethan Parker", Email = "ethan@example.com", PhoneNumber = "+201142236518" }, ImageUrl = "vue_basics.jpg" },
                new Course { Id = 12, Title = "TypeScript Masterclass", Code = "TS200", Description = "Deep dive into TypeScript", Instructor = new AppUser { FullName = "Olivia Adams", Email = "olivia@example.com", PhoneNumber = "+201142236519" }, ImageUrl = "typescript_masterclass.jpg" },
                new Course { Id = 13, Title = "Kotlin for Android", Code = "KOTLIN101", Description = "Android development with Kotlin", Instructor = new AppUser { FullName = "Daniel Carter", Email = "daniel@example.com", PhoneNumber = "+201142236520" }, ImageUrl = "kotlin_android.jpg" },
                new Course { Id = 14, Title = "Swift for iOS", Code = "SWIFT101", Description = "iOS app development with Swift", Instructor = new AppUser { FullName = "Sophia White", Email = "sophia@example.com", PhoneNumber = "+201142236521" }, ImageUrl = "swift_ios.jpg" },
                new Course { Id = 15, Title = "GraphQL API Development", Code = "GRAPHQL101", Description = "Build APIs with GraphQL", Instructor = new AppUser { FullName = "James Anderson", Email = "james@example.com", PhoneNumber = "+201142236522" }, ImageUrl = "graphql_api.jpg" },
                new Course { Id = 16, Title = "Docker & Kubernetes", Code = "DOCKER101", Description = "Containerization and orchestration", Instructor = new AppUser { FullName = "Ava Thomas", Email = "ava@example.com", PhoneNumber = "+201142236523" }, ImageUrl = "docker_kubernetes.jpg" },
                new Course { Id = 17, Title = "Cybersecurity Basics", Code = "CYBER100", Description = "Introduction to cybersecurity", Instructor = new AppUser { FullName = "Liam Roberts", Email = "liam@example.com", PhoneNumber = "+201142236524" }, ImageUrl = "cybersecurity_basics.jpg" },
                new Course { Id = 18, Title = "Blockchain Development", Code = "BLOCK101", Description = "Blockchain and smart contracts", Instructor = new AppUser { FullName = "Mia Garcia", Email = "mia@example.com", PhoneNumber = "+201142236525" }, ImageUrl = "blockchain_dev.jpg" },
                new Course { Id = 19, Title = "DevOps Practices", Code = "DEVOPS102", Description = "CI/CD and automation tools", Instructor = new AppUser { FullName = "Henry Wilson", Email = "henry@example.com", PhoneNumber = "+201142236526" }, ImageUrl = "devops_practices.jpg" },
                new Course { Id = 20, Title = "Big Data with Hadoop", Code = "HADOOP101", Description = "Data processing with Hadoop", Instructor = new AppUser { FullName = "Ella Martinez", Email = "ella@example.com", PhoneNumber = "+201142236527" }, ImageUrl = "bigdata_hadoop.jpg" }
            };

            // Convert to DTO before returning
            var courseDTOs = courses.Select(course => new CourseDTO(course)).ToList();

            var paginated = PaginatedList<CourseDTO>.Create(courseDTOs.AsQueryable(), index, 6);

            if (index > paginated.TotalPages)
                return BadRequest(new ErrorResponse()
                {
                    Code = HttpStatusCode.BadRequest,
                    Message = "Invalid Page Number"
                });

            return Ok(new SuccessResponse
            {
                Message = "Courses Retrieved Successfully.",
                Data = new PaginatedResponse<CourseDTO>(paginated),
                Code = System.Net.HttpStatusCode.OK,
            });
        }

    }
}
