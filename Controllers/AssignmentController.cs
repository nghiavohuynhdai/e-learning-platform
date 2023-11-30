using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Assignment;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ISectionService _sectionService;
        private readonly IInstructorService _instructorService;
        private readonly IRedisService _cacheService;

        public AssignmentController(IAssignmentService assignmentService,
            ISectionService sectionService, IInstructorService instructorService, IRedisService cacheService)
        {
            _assignmentService = assignmentService;
            _sectionService = sectionService;
            _instructorService = instructorService;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateAsync(Guid sectionId, CreateAssignmentDTO createAssignmentDTO)
        {
            ResultDTO result;
            result = createAssignmentDTO.Validate();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
            {
                result = ResultDTO.Fail(new[] { "Fail to create assignment" });
                return StatusCode(result.StatusCode, result);
            }

            var section = await _sectionService.GetAsync(sectionId, instructor.ID);
            if (section is null)
            {
                result = ResultDTO.Fail(new[] { "Section not found" }, 404);
                return StatusCode(result.StatusCode, result);
            }

            var assignment = new Assignment()
            {
                Title = createAssignmentDTO.Title,
                Description = createAssignmentDTO.Description,
                TimeTaken = createAssignmentDTO.TimeTaken,
                SectionID = sectionId,
                CourseID = section.CourseID,
                InstructorID = instructor.ID
            };
            var createdAssignment = await _assignmentService.CreateAsync(assignment);

            if (createdAssignment is not null)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + createdAssignment.CourseID);
                result = ResultDTO.Success(createdAssignment);
                return StatusCode(result.StatusCode, result);
            }

            result = ResultDTO.Fail(new[] { "Fail to create assignment" });
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateAsync(Guid id, UpdateAssignmentDTO updateAssignmentDTO)
        {
            ResultDTO result;
            result = updateAssignmentDTO.Validate();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
            {
                result = ResultDTO.Fail(new[] { "Fail to update assignment" });
                return StatusCode(result.StatusCode, result);
            }

            var assignment = await _assignmentService.GetAsync(id, instructor.ID);
            if (assignment is null)
            {
                result = ResultDTO.Fail(new[] { "Assignment not found" }, 404);
                return StatusCode(result.StatusCode, result);
            }

            assignment.Title = updateAssignmentDTO.Title;
            assignment.Description = updateAssignmentDTO.Description;
            assignment.TimeTaken = updateAssignmentDTO.TimeTaken;

            var updatedAssignment = await _assignmentService.UpdateAsync(assignment);
            if (updatedAssignment is not null)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + updatedAssignment.CourseID);
                result = ResultDTO.Success(updatedAssignment);
                return StatusCode(result.StatusCode, result);
            }

            result = ResultDTO.Fail(new[] { "Fail to update assignment" });
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            ResultDTO result;

            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
            {
                result = ResultDTO.Fail(new[] { "Fail to delete assignment" });
                return StatusCode(result.StatusCode, result);
            }

            var assignment = await _assignmentService.GetAsync(id, instructor.ID);
            if (assignment is null)
            {
                result = ResultDTO.Fail(new[] { "Assignment not found" }, 404);
                return StatusCode(result.StatusCode, result);
            }

            var deletedAssignment = await _assignmentService.DeleteAsync(assignment);
            if (deletedAssignment is not null)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + deletedAssignment.CourseID);
                result = ResultDTO.Success(deletedAssignment);
                return StatusCode(result.StatusCode, result);
            }

            result = ResultDTO.Fail(new[] { "Fail to delete assignment" });
            return StatusCode(result.StatusCode, result);
        }
    }
}