using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Lesson;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly ISectionService _sectionService;
    private readonly IInstructorService _instructorService;
    private readonly IRedisService _cacheService;

    public LessonController(ILessonService lessonService, ISectionService sectionService,
        IInstructorService instructorService, IRedisService cacheService)
    {
        _lessonService = lessonService;
        _sectionService = sectionService;
        _instructorService = instructorService;
        _cacheService = cacheService;
    }

    [Authorize(Roles = "Instructor")]
    [HttpPost]
    public async Task<IActionResult> CreateLesson(Guid sectionId, [FromBody] LessonCreateDTO lessonCreateDTO)
    {
        var result = lessonCreateDTO.Validate();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        var instructor = await _instructorService.GetCurrentInstructor();
        if (instructor is null)
        {
            result = ResultDTO.Fail(new[] { "Fail to create lesson" });
            return StatusCode(result.StatusCode, result);
        }

        var section = await _sectionService.GetAsync(sectionId, instructor.ID);
        if (section is null)
        {
            result = ResultDTO.Fail(new[] { "Section not found" }, 404);
            return StatusCode(result.StatusCode, result);
        }

        var lesson = new Lesson()
        {
            Name = lessonCreateDTO.Name,
            Overview = lessonCreateDTO.Overview,
            Content = lessonCreateDTO.Content,
            VideoUrl = lessonCreateDTO.VideoUrl,
            LearningTime = lessonCreateDTO.LearningTime,
            SectionID = sectionId,
            CourseID = section.CourseID,
            InstructorID = instructor.ID
        };

        var createdLesson = await _lessonService.CreateLesson(lesson);
        if (createdLesson is not null)
        {
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + createdLesson.CourseID);
            result = ResultDTO.Success(createdLesson);
            return StatusCode(result.StatusCode, result);
        }

        result = ResultDTO.Fail(new[] { "Fail to create lesson" });
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Instructor")]
    [HttpPut]
    public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] LessonUpdateDTO lessonUpdateDTO)
    {
        var result = lessonUpdateDTO.Validate();
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        var instructor = await _instructorService.GetCurrentInstructor();
        if (instructor is null)
        {
            result = ResultDTO.Fail(new[] { "Fail to update lesson" });
            return StatusCode(result.StatusCode, result);
        }

        var lesson = await _lessonService.GetAsync(id, instructor.ID);
        if (lesson is null)
        {
            result = ResultDTO.Fail(new[] { "Lesson not found" }, 404);
            return StatusCode(result.StatusCode, result);
        }

        lesson.Name = lessonUpdateDTO.Name;
        lesson.Overview = lessonUpdateDTO.Overview;
        lesson.Content = lessonUpdateDTO.Content;
        lesson.VideoUrl = lessonUpdateDTO.VideoUrl;
        lesson.LearningTime = lessonUpdateDTO.LearningTime;

        var updatedLesson = await _lessonService.UpdateLesson(lesson);
        if (updatedLesson is not null)
        {
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + updatedLesson.CourseID);
            result = ResultDTO.Success(updatedLesson);
            return StatusCode(result.StatusCode, result);
        }

        result = ResultDTO.Fail(new[] { "Fail to update lesson" });
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = "Instructor")]
    [HttpDelete]
    public async Task<IActionResult> DeleteLesson(Guid id)
    {
        ResultDTO result;

        var instructor = await _instructorService.GetCurrentInstructor();
        if (instructor is null)
        {
            result = ResultDTO.Fail(new[] { "Fail to delete lesson" });
            return StatusCode(result.StatusCode, result);
        }

        var lesson = await _lessonService.GetAsync(id, instructor.ID);

        if (lesson is null)
        {
            result = ResultDTO.Fail(new[] { "Lesson not found" }, 404);
            return StatusCode(result.StatusCode, result);
        }

        var deletedLesson = await _lessonService.DeleteLesson(lesson);
        if (deletedLesson is not null)
        {
            await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + deletedLesson.CourseID);
            result = ResultDTO.Success(deletedLesson);
            return StatusCode(result.StatusCode, result);
        }

        result = ResultDTO.Fail(new[] { "Fail to delete lesson" });
        return StatusCode(result.StatusCode, result);
    }
}