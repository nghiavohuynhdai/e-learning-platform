using System.Configuration;
using System.Runtime.InteropServices;
using Cursus.DTO;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = ("Admin"))]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("total-earnings")]
        public IActionResult GetTotalEarningsForMonth([FromQuery] int year, [FromQuery] int month)
        {
            var result = _adminService.GetTotalEarningsForMonth(year, month);

            if (result._isSuccess)
            {
                return Ok(result._data);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpGet("total-earnings-for-year")]
        public IActionResult GetTotalEarningsForYear([FromQuery] int year)
        {
            var result = _adminService.GetTotalEarningsForYear(year);

            if (result._isSuccess)
            {
                return Ok(result._data);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpGet("course-stats-for-year")]
        public IActionResult GetCourseStatsForYear([FromQuery] int year)
        {
            var result = _adminService.GetCourseStatsForYear(year);

            if (result._isSuccess)
            {
                return Ok(result._data);
            }

            return StatusCode(result._statusCode, result._message);
        }

        [HttpGet("total-student")]
        public IActionResult GetTotalStudentForMonth([FromQuery] int year)
        {
            var result = _adminService.GetTotalStudentForMonth(year);
            if (result._isSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("total-Instructors")]
        public async Task<IActionResult> GetTotalInstructorForMonth([FromQuery] int year)
        {
            var result = await _adminService.GetTotalInstructorForMonth(year);
            if (result is not null)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("most-learners-courses")]
        public async Task<IActionResult> GetCoursesHaveMostLearners([FromQuery] int quantity = 5)
        {
            if (quantity is <= 0 or > 100)
                return BadRequest(ResultDTO.Fail(new[] { "Quantity must be between 1 and 100" }, 400));
            var result = await _adminService.GetMostLearnersCoursesAsync(quantity);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("least-learners-courses")]
        public async Task<IActionResult> GetCoursesHaveLeastLearners([FromQuery] int quantity = 5)
        {
            if (quantity is <= 0 or > 100)
                return BadRequest(ResultDTO.Fail(new[] { "Quantity must be between 1 and 100" }, 400));
            var result = await _adminService.GetLeastLearnersCoursesAsync(quantity);
            return StatusCode(result.StatusCode, result);
        }
    }
}