using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Section;
using Cursus.Entities;
using Cursus.Services;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly IInstructorService _instructorService;
        private readonly IRedisService _cacheService;

        public SectionController(ISectionService sectionService, IInstructorService instructorService,
            IRedisService cacheService)
        {
            _sectionService = sectionService;
            _instructorService = instructorService;
            _cacheService = cacheService;
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(Guid courseId,
            [FromBody] CreateSectionDTO createSectionDTO)
        {
            var result = createSectionDTO.Validate();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            var section = new Section
            {
                Name = createSectionDTO.Name,
                Description = createSectionDTO.Description,
                CourseID = courseId
            };

            try
            {
                var createdSection = await _sectionService.CreateSection(section);
                if (createdSection is not null)
                {
                    await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + createdSection.CourseID);
                    result = ResultDTO.Success(createdSection);
                    return StatusCode(result.StatusCode, result);
                }

                result = ResultDTO.Fail(new[] { "Fail to create section" });
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception e)
            {
                result = ResultDTO.Fail(new[] { e.Message }, 400);
                return StatusCode(result.StatusCode);
            }
        }

        [HttpPut]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Update(Guid id,
            [FromBody] UpdateSectionDTO updateSectionDTO)
        {
            var result = updateSectionDTO.Validate();
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
            {
                result = ResultDTO.Fail(new[] { "Fail to update section" });
                return StatusCode(result.StatusCode, result);
            }

            var section = await _sectionService.GetAsync(id, instructor.ID);
            if (section is null)
            {
                result = ResultDTO.Fail(new[] { "Section not found" }, 404);
                return StatusCode(result.StatusCode, result);
            }

            section.Name = updateSectionDTO.Name;
            section.Description = updateSectionDTO.Description;
            var updatedSection = await _sectionService.UpdateSection(section);
            if (updatedSection is not null)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + updatedSection.CourseID);
                result = ResultDTO.Success(updatedSection);
                return StatusCode(result.StatusCode, result);
            }

            result = ResultDTO.Fail(new[] { "Fail to update section" });
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            ResultDTO result;
            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
            {
                result = ResultDTO.Fail(new[] { "Fail to update section" });
                return StatusCode(result.StatusCode, result);
            }

            var section = await _sectionService.GetAsync(id, instructor.ID);
            if (section is null)
            {
                result = ResultDTO.Fail(new[] { "Section not found" }, 404);
                return StatusCode(result.StatusCode, result);
            }

            var deletedSection = await _sectionService.Delete(section);
            if (deletedSection is not null)
            {
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.CourseDetail + deletedSection.CourseID);
                result = ResultDTO.Success(deletedSection);
                return StatusCode(result.StatusCode, result);
            }

            result = ResultDTO.Fail(new[] { "Fail to delete section" });
            return StatusCode(result.StatusCode, result);
        }
    }
}