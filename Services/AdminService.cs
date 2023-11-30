using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.Entities;
using Cursus.DTO.Admin;
using Cursus.Repositories;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public AdminService(IUnitOfWork unitOfWork, UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }


        public ResultDTO<EarningsOfMonthDTO> GetTotalEarningsForMonth(int year, int month)
        {
            try
            {
                var currentDate = DateTime.Now;
                var currentYear = currentDate.Year;
                var currentMonth = currentDate.Month;

                var orders = _unitOfWork.OrderRepository.GetQueryable();

                var totalEarningCurrent = orders
                    .Where(order => order.CreatedDate.Year == currentYear && order.CreatedDate.Month == currentMonth
                                                                          && order.Status ==
                                                                          Enum.GetName(OrderStatus.Completed))
                    .Sum(order => order.TotalPrice);

                if (year < currentYear || month < 1 || month > currentMonth)
                {
                    return ResultDTO<EarningsOfMonthDTO>.Fail("Invalid year or month.", 400);
                }

                var totalEarnings = orders
                    .Where(order => order.CreatedDate.Year == year && order.CreatedDate.Month == month
                                                                   && order.Status ==
                                                                   Enum.GetName(OrderStatus.Completed))
                    .Sum(order => order.TotalPrice);

                var earningsDTO = new EarningsOfMonthDTO
                {
                    CurrentEarning = totalEarningCurrent,
                    EarningMonth = totalEarnings,
                    EarningDifference = totalEarningCurrent - totalEarnings
                };

                return ResultDTO<EarningsOfMonthDTO>.Success(earningsDTO);
            }
            catch (Exception ex)
            {
                return ResultDTO<EarningsOfMonthDTO>.Fail($"An error occurred: {ex.Message}", 500);
            }
        }

        public ResultDTO<List<EarningsForYearDTO>> GetTotalEarningsForYear(int year)
        {
            try
            {
                if (year < 1)
                {
                    return ResultDTO<List<EarningsForYearDTO>>.Fail("Invalid year.", 400);
                }

                var orders = _unitOfWork.OrderRepository.GetQueryable();

                var monthlyEarnings = new List<EarningsForYearDTO>();

                for (int month = 1; month <= 12; month++)
                {
                    var totalEarnings = orders
                        .Where(order => order.CreatedDate.Year == year && order.CreatedDate.Month == month)
                        .Sum(order => order.TotalPrice);

                    var earningsDTO = new EarningsForYearDTO
                    {
                        Month = month,
                        Earning = totalEarnings
                    };

                    monthlyEarnings.Add(earningsDTO);
                }

                return ResultDTO<List<EarningsForYearDTO>>.Success(monthlyEarnings);
            }
            catch (Exception ex)
            {
                return ResultDTO<List<EarningsForYearDTO>>.Fail($"An error occurred: {ex.Message}", 500);
            }
        }

        public ResultDTO<List<CourseStatsDTO>> GetCourseStatsForYear(int year)
        {
            try
            {
                if (year < 1)
                {
                    return ResultDTO<List<CourseStatsDTO>>.Fail("Invalid year.", 400);
                }

                var courses = _unitOfWork.CourseRepository.Courses;

                var currentDate = DateTime.Now;
                var monthlyCourseStats = new List<CourseStatsDTO>();

                for (int month = 1; month <= 12; month++)
                {
                    var totalCourses = courses
                        .Count(course => course.CreatedDate.Year == year && course.CreatedDate.Month == month);

                    var courseStatsDTO = new CourseStatsDTO
                    {
                        Month = month,
                        TotalCourses = totalCourses
                    };

                    monthlyCourseStats.Add(courseStatsDTO);
                }

                return ResultDTO<List<CourseStatsDTO>>.Success(monthlyCourseStats);
            }
            catch (Exception ex)
            {
                return ResultDTO<List<CourseStatsDTO>>.Fail($"An error occurred: {ex.Message}", 500);
            }
        }

        public ResultDTO<List<StudentStatsDTO>> GetTotalStudentForMonth(int year)
        {
            try
            {
                //check current DateTime
                var currentDate = DateTime.Now;
                var currentYear = currentDate.Year;
                var currentMonth = currentDate.Month;

                if (year < 1 || year > currentYear)
                {
                    return ResultDTO<List<StudentStatsDTO>>.Fail("Invalid year or month.", 400);
                }

                //check total student
                var allOrders = _unitOfWork.OrderRepository.GetAll();
                var listNumOfStudent = new List<StudentStatsDTO>();

                //total Student 
                for (int i = 1; i <= 12; i++)
                {
                    var totalStudent = allOrders.Where(x => x.CreatedDate.Year == year && x.CreatedDate.Month == i)
                        .Count(x => x.Status == Enum.GetName(OrderStatus.Completed));

                    var numOfStudent = new StudentStatsDTO
                    {
                        Month = i,
                        NumOfStudent = totalStudent,
                    };
                    listNumOfStudent.Add(numOfStudent);
                }


                return ResultDTO<List<StudentStatsDTO>>.Success(listNumOfStudent);
            }
            catch (Exception ex)
            {
                return ResultDTO<List<StudentStatsDTO>>.Fail($"Service Fail, error: {ex.Message}");
            }
        }

        public async Task<ResultDTO<List<InstructorStatsDTO>>> GetTotalInstructorForMonth(int year)
        {
            try
            {
                var currentDate = DateTime.Now;
                var currentYear = currentDate.Year;
                var currentMonth = currentDate.Month;

                if (year < 1 || year > currentYear)
                {
                    return ResultDTO<List<InstructorStatsDTO>>.Fail("Invalid year or month.", 400);
                }

                var ListInstructor = new List<InstructorStatsDTO>();
                for (var i = 1; i <= 12; i++)
                {
                    var instructorCount = (await _userManager.GetUsersInRoleAsync("Instructor"))
                        .Where(x => x.CreatedDate.Year == year && x.CreatedDate.Month == i)
                        .Count();
                    var InstuctorStats = new InstructorStatsDTO
                    {
                        Month = i,
                        NumOfInstructor = instructorCount
                    };
                    ListInstructor.Add(InstuctorStats);
                }

                return ResultDTO<List<InstructorStatsDTO>>.Success(ListInstructor);
            }
            catch (Exception ex)
            {
                return ResultDTO<List<InstructorStatsDTO>>.Fail($"Service failed, error : {ex.Message}");
            }
        }

        public async Task<ResultDTO> GetMostLearnersCoursesAsync(int quantity)
        {
            try
            {
                var courseUserJoin = _unitOfWork.CourseRepository.GetCourseUserJoin();
                var coursesWithLearnerQuantity = await courseUserJoin
                    .GroupBy(cUser => cUser.Course.ID)
                    .Select(group => new
                        {
                            group.FirstOrDefault().Course,
                            LearnerQuantity = group.Count(mem => mem.User != null)
                        }
                    )
                    .OrderByDescending(cLearner => cLearner.LearnerQuantity)
                    .Take(quantity)
                    .ToListAsync();

                return ResultDTO.Success(coursesWithLearnerQuantity
                    .Select(cLearner =>
                    {
                        var dto = _mapper.Map<CourseWithLearnerQuantityDTO>(cLearner.Course);
                        dto.LearnerQuantity = cLearner.LearnerQuantity;
                        return dto;
                    }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ResultDTO.Fail(new[] { "Service is not available" });
            }
        }

        public async Task<ResultDTO> GetLeastLearnersCoursesAsync(int quantity)
        {
            try
            {
                var courseUserJoin = _unitOfWork.CourseRepository.GetCourseUserJoin();
                var coursesWithLearnerQuantity = await courseUserJoin
                    // Take only courses created more than 30 days ago
                    .Where(cLearner => (int)(DateTime.UtcNow - cLearner.Course.CreatedDate).TotalDays > 30)
                    .GroupBy(cUser => cUser.Course.ID)
                    .Select(group => new
                        {
                            group.FirstOrDefault().Course,
                            LearnerQuantity = group.Count(mem => mem.User != null)
                        }
                    )
                    .OrderBy(cLearner => cLearner.LearnerQuantity)
                    .Take(quantity)
                    .ToListAsync();

                return ResultDTO.Success(coursesWithLearnerQuantity
                    .Select(cLearner =>
                    {
                        var dto = _mapper.Map<CourseWithLearnerQuantityDTO>(cLearner.Course);
                        dto.LearnerQuantity = cLearner.LearnerQuantity;
                        return dto;
                    }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return ResultDTO.Fail(new[] { "Service is not available" });
            }
        }
    }
}