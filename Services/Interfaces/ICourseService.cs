using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.Entities;

namespace Cursus.Services
{
    public interface ICourseService
    {
        public Task<ResultDTO<CourseListDTO>> GetCoursesByFilterAsync(
            int offset, int limit,
            double minPrice, double maxPrice, List<Guid> catalogIDs,
            string? courseName,
            string? instructorName,
            CourseSort courseSort
        );

        public Task<CourseDetailDTO> GetCourseDetailAsync(Guid courseId);

        public Task<ResultDTO<CourseListDTO>> GetCoursesByInstructorAsync(
            int offset, int limit
        );

        Task<ResultDTO<CreateCourseResDTO>> AddCourse(CreateCourseReqDTO courseRequest);
        Task Update(UpdateCourseDTO updateCourse);
        Task<ResultDTO<string>> DeleteCourse(Guid id);
        Task<ResultDTO<CourseListDTO>> GetInstructorCoursesAsync();
        Task<ResultDTO<CourseListDTO>> GetPaidCoursesAsync();
        Task<ResultDTO<PublicCourseDetailDTO>> GetPublicCourseDetailAsync(Guid courseId);
        Task<bool> CanCurrentUserAccessCourseAsync(Guid courseId, string userRole);
        Task<ResultDTO<CourseDetailDTO>> GetPaidCourseDetailAsync(Guid courseId);
    }
}