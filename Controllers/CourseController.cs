using System.Security.Claims;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.Services;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IRedisService _cacheService;

        public CourseController(ICourseService courseService, IRedisService cacheService)
        {
            _courseService = courseService;
            _cacheService = cacheService;
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetCoursesByFilter(
            int offset, int limit,
            double minPrice, double maxPrice, List<Guid> catalogIDs,
            string? courseName, string? instructorName,
            CourseSort courseSort
        )
        {
            var result = await _courseService.GetCoursesByFilterAsync(offset, limit, minPrice, maxPrice, catalogIDs, courseName,
                instructorName, courseSort);
            return result._isSuccess ? Ok(result) : StatusCode(500, result);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("get-by-instructor")]
        public async Task<IActionResult> GetCoursesByInstructor(int offset, int limit)
        {
            var result = await _courseService.GetCoursesByInstructorAsync(offset, limit);
            return StatusCode(result._statusCode, result);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Post([FromBody] CreateCourseReqDTO courseRequest)
        {
            if (courseRequest == null)
            {
                return NoContent();
            }

            var createCourse = await _courseService.AddCourse(courseRequest);
            if (!createCourse._isSuccess)
            {
                return NotFound(createCourse);
            }

            return Ok(createCourse);
        }

        [HttpPut]
        [Route("update")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update([FromBody] UpdateCourseDTO course)
        {
            await _courseService.Update(course);
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + course.ID);
            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _courseService.DeleteCourse(id);
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + id);
            return result._isSuccess ? Ok(result) : StatusCode(500, result);
        }

        [HttpGet]
        [Route("course-detail/{courseId}")]
        public async Task<IActionResult> GetPublicCourseDetail(Guid courseId)
        {
            var result = await _courseService.GetPublicCourseDetailAsync(courseId);

            return result._isSuccess ? Ok(result._data) : StatusCode(500, result._message);
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "User,Instructor")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userRoles = User.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);
            var result = userRoles.Contains("Instructor")
                ? await _courseService.GetInstructorCoursesAsync()
                : await _courseService.GetPaidCoursesAsync();
            return StatusCode(result._statusCode, result);
        }

        [HttpGet("my-courses/course-detail/{courseId}")]
        [Authorize(Roles = "User,Instructor")]
        public async Task<IActionResult> GetMyCourseDetail(Guid courseId)
        {
            var userRoles = User.Claims.Where(claim => claim.Type == ClaimTypes.Role).Select(claim => claim.Value);

            var canAccess = await _courseService.CanCurrentUserAccessCourseAsync(courseId,
                userRoles.Contains("Instructor") ? "Instructor" : "User");
            if (!canAccess)
            {
                return StatusCode(404, ResultDTO<CourseDetailDTO>.Fail("Course does not exist", 404));
            }

            var key = $"course_detail:{courseId}";
            var cache = await _cacheService.GetDataAsync<CourseDetailDTO>(key);
            if (cache is not null)
                return StatusCode(200, ResultDTO<CourseDetailDTO>.Success(cache));
            var courseDetail = await _courseService.GetCourseDetailAsync(courseId);
            await _cacheService.SetDataAsync(key, courseDetail);
            return courseDetail is not null
                ? StatusCode(200, ResultDTO<CourseDetailDTO>.Success(courseDetail))
                : StatusCode(404, ResultDTO<CourseDetailDTO>.Fail("Course does not exist", 404));
        }
    }
}