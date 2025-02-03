using GraduationProject.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GraduationProject.Services;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;
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


/*
        [HttpGet("{index}")]
        public async Task<IActionResult> GetPageOfCourses(int index = 1)
        {
            if (index <= 0)
                return BadRequest();

            var courses = _context.Foods
                .Include(x => x.FoodTags)
                .Include(x => x.FoodOrigins)
                .Where(x => x.Hidden == false)
                .OrderBy(x=> x.Name)
                .Select(x => new FoodDTO(x));

            var paginatedList = await PaginatedList<CourseDTO>.CreateAsync(courses, index);

            if (index > paginatedList.TotalPages)
                return BadRequest();
            
            return Ok(new PaginatedResponse<CourseDTO>(paginatedList));

        }


        [HttpGet("search/{searchTerm}")]
        public async Task<IActionResult> GetCoursesBySearching(string searchTerm, int index = 1)
        {
            if (index <= 0)
                return BadRequest();

            var foods = _context.Foods
                .Include(x => x.FoodTags)
                .Include(x => x.FoodOrigins)
                .Where(x => x.Hidden == false)
                .Where(x => x.Name.ToLower().Contains(searchTerm.ToLower()))
                .OrderBy(x => x.Name)
                .Select(x => new FoodDTO(x));
            var paginatedList = await PaginatedList<FoodDTO>.CreateAsync(foods, index);

            if (paginatedList.TotalPages == 0)
                return NotFound();

            if (index > paginatedList.TotalPages)
                return BadRequest();

            return Ok(new PaginatedResponse<FoodDTO>(paginatedList));
        }
        
        
        [HttpGet("id/{courseId}")]
        public async Task<IActionResult> getCourseById(int foodId)
        {
            var food = await _context.Foods
                .Include(x => x.FoodTags)
                .Include(x => x.FoodOrigins)
                .Select(x => new FoodDTO(x))
                .FirstOrDefaultAsync(x => x.Id == foodId);

            if (food is null)
                return NotFound();

            return Ok(food);
        }*/
    }
}
