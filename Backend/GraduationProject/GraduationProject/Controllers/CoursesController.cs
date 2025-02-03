using Microsoft.AspNetCore.Mvc;
using GraduationProject.Models.DTOs;
using GraduationProject.Controllers.ResponseModels;
using GraduationProject.Repositories;

namespace GraduationProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly CoursesRepository _coursesRepo;

        public CoursesController(CoursesRepository coursesRepository)
        {
            _coursesRepo = coursesRepository;
        }



        [HttpGet("{index}")]
        public async Task<IActionResult> GetPageOfCourses(int index = 1)
        {
            if (index <= 0)
                return BadRequest();

            var courses = await _coursesRepo.GetPageOfCourses(index);

            if (index > courses.TotalPages)
                return BadRequest();

            return Ok(new PaginatedResponse<CourseDTO>(courses));

        }


        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> GetCoursesBySearching(string searchTerm, int index = 1)
        {
            if (index <= 0)
                return BadRequest();

            var courses = await _coursesRepo.GetPageOfCoursesBySearching(searchTerm, index);

            if (courses.TotalPages == 0)
                return NotFound();

            if (index > courses.TotalPages)
                return BadRequest("Invalid Page Number");

            return Ok(new PaginatedResponse<CourseDTO>(courses));
        }


        [HttpGet("id/{courseId}")]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            var course = await _coursesRepo.GetById(courseId);

            if (course == null)
                return NotFound();

            return Ok(course);
        }
    }
}
