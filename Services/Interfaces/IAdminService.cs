using Cursus.DTO;
using Cursus.DTO.Admin;
using static Cursus.Services.AdminService;

namespace Cursus.Services.Interfaces
{
    public interface IAdminService
    {
        public ResultDTO<List<StudentStatsDTO>> GetTotalStudentForMonth(int year);
        public Task<ResultDTO<List<InstructorStatsDTO>>> GetTotalInstructorForMonth(int year);
        public ResultDTO<EarningsOfMonthDTO> GetTotalEarningsForMonth(int year, int month);
        public ResultDTO<List<EarningsForYearDTO>> GetTotalEarningsForYear(int year);

        public ResultDTO<List<CourseStatsDTO>> GetCourseStatsForYear(int year);
        public Task<ResultDTO> GetMostLearnersCoursesAsync(int quantity);
        Task<ResultDTO> GetLeastLearnersCoursesAsync(int quantity);
    }
}