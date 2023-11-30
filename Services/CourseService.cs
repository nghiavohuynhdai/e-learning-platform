using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Course;
using Cursus.DTO.CourseCatalog;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace Cursus.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICatalogService _catalogService;
        private readonly ICourseCatalogService _courseCatalogService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IQuizRepository _quizRepository;
        private readonly IInstructorService _instructorService;
        private readonly ICartService _cartService;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper, ICourseCatalogService courseCatalogService,
            ICatalogService catalogService, UserManager<User> userManager, IUserService userService,
            IInstructorService instructorService,
            IQuizRepository quizRepository, ICartService cartService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _catalogService = catalogService;
            _courseCatalogService = courseCatalogService;
            _userManager = userManager;
            _userService = userService;
            _instructorService = instructorService;
            _quizRepository = quizRepository;
            _cartService = cartService;
        }

        private async Task<IEnumerable<CourseDTO>> GetCoursesAsync(bool deletedFilter)
        {
            var courseCatalogs = _unitOfWork.CourseCatalogRepository.GetQueryable();

            var courseFeedbacks = _unitOfWork.CourseFeedbackRepository.GetQueryable()
                .GroupBy(cf => cf.CourseID)
                .Select(g => new
                {
                    CourseID = g.Key,
                    AvgRate = g.Average(cf => cf.Rate)
                });

            var orderDetails = _unitOfWork.OrderDetailRepository.GetQueryable()
                .GroupBy(r => r.CourseID)
                .Select(g => new
                {
                    CourseID = g.Key,
                    LearnerQuantity = g.Count()
                });

            var instructors = _unitOfWork.InstructorRepository.GetQueryable()
                .Join(
                    _userManager.Users.AsQueryable(),
                    i => i.UserID.ToString(),
                    u => u.Id,
                    (i, u) => new InstructorDTO()
                    {
                        ID = i.ID,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Image = u.Image
                    });

            var courses = deletedFilter
                ? _unitOfWork.CourseRepository.Courses
                    .Where(c => !c.IsDeleted)
                : _unitOfWork.CourseRepository.Courses;
            var courseDTOs = (await courses
                    .GroupJoin(
                        courseFeedbacks,
                        outer => outer.ID,
                        inner => inner.CourseID,
                        (outer, inners) => new { Course = outer, CourseFeedbacks = inners })
                    .SelectMany(
                        src => src.CourseFeedbacks.DefaultIfEmpty(),
                        (src, item) => new
                        {
                            src.Course,
                            AvgRate = item.AvgRate != null ? item.AvgRate : 0
                        }
                    )
                    .GroupJoin(
                        orderDetails,
                        outer => outer.Course.ID,
                        inner => inner.CourseID,
                        (outer, inners) =>
                            new { outer.Course, outer.AvgRate, OrderDetails = inners }
                    )
                    .SelectMany(
                        src => src.OrderDetails.DefaultIfEmpty(),
                        (src, item) => new
                        {
                            src.Course,
                            src.AvgRate,
                            LearnerQuantity = item.LearnerQuantity != null ? item.LearnerQuantity : 0
                        }
                    )
                    .GroupJoin(
                        courseCatalogs,
                        outer => outer.Course.ID,
                        inner => inner.CourseID,
                        (outer, inners) => new
                        {
                            outer.Course,
                            outer.AvgRate,
                            outer.LearnerQuantity,
                            CourseCatalogs = inners
                        }
                    )
                    .SelectMany(
                        src => src.CourseCatalogs.DefaultIfEmpty(),
                        (src, item) => new
                        {
                            src.Course,
                            src.AvgRate,
                            src.LearnerQuantity,
                            CatalogID = item.CatalogID != null ? item.CatalogID : Guid.Empty,
                        }
                    )
                    .Join(
                        instructors,
                        outer => outer.Course.InstructorID,
                        inner => inner.ID,
                        (outer, inner) => new
                        {
                            CourseJoin = new
                            {
                                outer.Course,
                                outer.AvgRate,
                                outer.LearnerQuantity,
                                Instructor = inner
                            },
                            outer.CatalogID,
                        }
                    )
                    .ToListAsync())
                .GroupBy(src => src.CourseJoin.Course.ID)
                .Select(group => new
                {
                    CourseJoin = group.FirstOrDefault().CourseJoin,
                    CatalogIDs = group.Select(g => g.CatalogID)
                })
                .Select(join =>
                {
                    var dto = _mapper.Map<CourseDTO>(join.CourseJoin.Course);
                    dto.Instructor = join.CourseJoin.Instructor;
                    dto.AvgRate = join.CourseJoin.AvgRate;
                    dto.LearnerQuantity = join.CourseJoin.LearnerQuantity;
                    dto.CatalogIDs = join.CatalogIDs.Contains(Guid.Empty)
                        ? new List<Guid>()
                        : join.CatalogIDs.ToList();
                    return dto;
                });

            var lessons = await _unitOfWork.LessonRepository.Lessons
                .Select(les => new { les.CourseID, les.LearningTime })
                .ToListAsync();
            var assignments = await _unitOfWork.AssignmentRepository.Assignments
                .Select(assig => new { assig.CourseID, assig.TimeTaken })
                .ToListAsync();
            var quizzes = await MongoDB.Driver.IAsyncCursorSourceExtensions.ToListAsync(_quizRepository.Quizzes
                .Select(quiz => new { quiz.CourseID, quiz.TimeTaken }));

            courseDTOs = courseDTOs.Select(c =>
                {
                    var courseLessons = lessons.Where(les => les.CourseID == c.ID).ToList();
                    var courseAssignments = assignments.Where(assig => assig.CourseID == c.ID).ToList();
                    var courseQuizzes = quizzes.Where(quiz => quiz.CourseID == c.ID).ToList();

                    var totalLearningTime = 0;
                    if (courseLessons.Count != 0)
                        totalLearningTime = courseLessons.Sum(les => les.LearningTime);

                    var totalAssignmentTime = 0;
                    if (courseAssignments.Count != 0)
                        totalAssignmentTime = courseAssignments.Sum(assig => assig.TimeTaken);

                    var totalQuizTime = 0;
                    if (courseQuizzes.Count != 0)
                        totalQuizTime = courseQuizzes.Sum(quiz => quiz.TimeTaken);
                    c.LessonQuantity = courseLessons.Count;
                    c.TotalTimeTaken = totalLearningTime + totalAssignmentTime + totalQuizTime;
                    return c;
                }
            );
            return courseDTOs;
        }

        public async Task<CourseDetailDTO> GetCourseDetailAsync(Guid courseId)
        {
            try
            {
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    return null;
                }

                var courseDetailDto = _mapper.Map<CourseDetailDTO>(course);
                var sections = await _unitOfWork.SectionRepository.GetManyByCourseIdAsync(courseDetailDto.ID);
                var lessons = await _unitOfWork.LessonRepository.GetManyByCourseIdAsync(courseDetailDto.ID);
                var assignments = await _unitOfWork.AssignmentRepository.GetManyByCourseIdAsync(courseDetailDto.ID);
                var quizzes = await _quizRepository.GetManyByCourseIdAsync(courseDetailDto.ID);
                courseDetailDto.Sections = _mapper.Map<List<CourseSectionDTO>>(sections);
                courseDetailDto.Sections = courseDetailDto.Sections.Select(
                    sec =>
                    {
                        sec.Lessons = _mapper.Map<List<CourseLessonDTO>>(lessons
                            .Where(les => les.SectionID == sec.ID).ToList()
                        );
                        sec.Assignments = _mapper.Map<List<CourseAssignmentDTO>>(assignments
                            .Where(assign => assign.SectionID == sec.ID).ToList()
                        );
                        sec.Quizzes = _mapper.Map<List<CourseQuizDTO>>(quizzes
                            .Where(quiz => quiz.SectionID == sec.ID).ToList()
                        );
                        return sec;
                    }
                ).ToList();

                return courseDetailDto;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return null;
            }
        }

        public async Task<ResultDTO<CourseListDTO>> GetCoursesByFilterAsync(
            int offset, int limit,
            double minPrice, double maxPrice, List<Guid> catalogIDs,
            string? courseName,
            string? instructorName,
            CourseSort courseSort
        )
        {
            if (offset < 0 || limit <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "offset must be greater or equal to 0 and limit must be greater than 0");
            }

            if (minPrice < 0 || maxPrice <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "minimum price must be greater or equal to 0 and maximum price must be greater than 0"
                );
            }

            try
            {
                var courseDTOs = (await GetCoursesAsync(true))
                    .Where(
                        c => c.Price >= minPrice &&
                             c.Price <= maxPrice
                    );

                if (!string.IsNullOrEmpty(courseName))
                    courseDTOs = courseDTOs.Where(c =>
                        c.Name.Normalize().ToLower().Contains(courseName.Normalize().ToLower()));

                if (!string.IsNullOrEmpty(instructorName))
                    courseDTOs = courseDTOs.Where(c =>
                        (c.Instructor.LastName + " " + c.Instructor.FirstName).Normalize().ToLower()
                        .Contains(instructorName.Normalize().ToLower())
                    );

                if (catalogIDs.Any())
                    courseDTOs = courseDTOs.Where(c => c.CatalogIDs.Intersect(catalogIDs).Any());

                switch (courseSort)
                {
                    case CourseSort.TopRate:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.AvgRate);
                        break;
                    case CourseSort.AscName:
                        courseDTOs = courseDTOs.OrderBy(c => c.Name);
                        break;
                    case CourseSort.DscName:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.Name);
                        break;
                    case CourseSort.Newest:
                        courseDTOs = courseDTOs.OrderByDescending(c => c.CreatedDate);
                        break;
                    case CourseSort.Oldest:
                        courseDTOs = courseDTOs.OrderBy(c => c.CreatedDate);
                        break;
                }

                var result = new CourseListDTO()
                {
                    List = courseDTOs.Skip(offset).Take(limit).ToList(),
                    Total = courseDTOs.Count(),
                    SortBy = Enum.GetName(courseSort)
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<CourseListDTO>> GetCoursesByInstructorAsync(
            int offset, int limit
        )
        {
            if (offset < 0 || limit <= 0)
            {
                return ResultDTO<CourseListDTO>.Fail(
                    "offset must be greater or equal to 0 and limit must be greater than 0");
            }

            try
            {
                var currentUser = await _userService.GetCurrentUser();

                if (currentUser is null)
                    return ResultDTO<CourseListDTO>.Fail("Instructor is not found");

                var instructor =
                    await _unitOfWork.InstructorRepository.GetAsync(i => i.UserID == Guid.Parse(currentUser.Id));
                if (instructor is null)
                    return ResultDTO<CourseListDTO>.Fail("Instructor is not found");

                var courseDTOs = (await GetCoursesAsync(true))
                    .Where(c => c.Instructor is not null && c.Instructor.ID == instructor.ID);

                var result = new CourseListDTO()
                {
                    List = courseDTOs.Skip(offset).Take(limit).ToList(),
                    Total = courseDTOs.Count()
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<CreateCourseResDTO>> AddCourse(CreateCourseReqDTO courseRequest)
        {
            if (string.IsNullOrEmpty(courseRequest.Outcome))
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Outcome is required.");
            }

            if (string.IsNullOrEmpty(courseRequest.Image))
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Image is required.");
            }

            if (string.IsNullOrEmpty(courseRequest.VideoIntroduction))
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Video introduction is required.");
            }

            if (courseRequest.Price < 0)
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Price should be greater than 0.");
            }

            if (string.IsNullOrEmpty(courseRequest.Description))
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Description is required.");
            }

            if (string.IsNullOrEmpty(courseRequest.Name))
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Name is required.");
            }

            if (courseRequest.CatalogIDs.Count <= 0)
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Catalog ID is required.");
            }

            var currentUser = await _userService.GetCurrentUser();

            if (currentUser is null)
                return ResultDTO<CreateCourseResDTO>.Fail("Instructor is not found");

            var instructor =
                await _unitOfWork.InstructorRepository.GetAsync(i => i.UserID == Guid.Parse(currentUser.Id));
            if (instructor is null)
                return ResultDTO<CreateCourseResDTO>.Fail("Instructor is not found");

            var _course = new Course
            {
                ID = Guid.NewGuid(),
                Name = courseRequest.Name,
                Description = courseRequest.Description,
                Price = courseRequest.Price,
                Outcome = courseRequest.Outcome,
                Image = courseRequest.Image,
                VideoIntroduction = courseRequest.VideoIntroduction,
                InstructorID = instructor.ID,
            };
            if (courseRequest.CatalogIDs != null)
            {
                var hasErrors = false;
                var distinctCatalogIDs = courseRequest.CatalogIDs.Distinct().ToList();
                if (distinctCatalogIDs.Count < courseRequest.CatalogIDs.Count)
                {
                    hasErrors = true;
                    return ResultDTO<CreateCourseResDTO>.Fail("Duplicate catalog IDs found.");
                }

                foreach (var catalogID in distinctCatalogIDs)
                {
                    var catalogExists = await _catalogService.CatalogExists(catalogID);
                    if (!catalogExists._isSuccess)
                    {
                        hasErrors = true;
                        return ResultDTO<CreateCourseResDTO>.Fail("Catalogs does not exist:" + catalogID);
                    }
                }

                if (!hasErrors)
                {
                    foreach (var catalogID in distinctCatalogIDs)
                    {
                        var catalogExists = await _catalogService.CatalogExists(catalogID);
                        var courseCatalog = new CourseCatalogReqDTO
                        {
                            CatalogID = catalogExists._data.ID,
                            CourseID = _course.ID
                        };

                        var result = await _courseCatalogService.AddCourseCatalog(courseCatalog);
                        if (!result._isSuccess)
                        {
                            return ResultDTO<CreateCourseResDTO>.Fail("Failed to add course catalog: " + result);
                        }
                    }
                }
            }

            await _unitOfWork.CourseRepository.AddAsync(_course);

            try
            {
                await _unitOfWork.CommitAsync();
                var course = _mapper.Map<CreateCourseResDTO>(_course);
                return ResultDTO<CreateCourseResDTO>.Success(course);
            }

            catch (Exception ex)
            {
                return ResultDTO<CreateCourseResDTO>.Fail("Failed to add course: " + ex.Message);
            }
        }

        public async Task Update(UpdateCourseDTO updateCourse)
        {
            try
            {
                //check model is valid
                if (updateCourse == null)
                {
                    throw new Exception("Course not found");
                }

                string message = "";
                //check all fields are valid
                if (updateCourse.ID == null || string.IsNullOrEmpty(updateCourse.Name) ||
                    string.IsNullOrEmpty(updateCourse.Description) ||
                    updateCourse.Price == null || updateCourse.CatalogIDs == null ||
                    string.IsNullOrEmpty(updateCourse.Outcome) ||
                    string.IsNullOrEmpty(updateCourse.Image) ||
                    string.IsNullOrEmpty(updateCourse.VideoIntroduction))
                {
                    message = "All fields is empty!";
                }
                else if (updateCourse.Name.Length < 3)
                {
                    message = "Name must be greater than 3 characters!";
                }
                else if (updateCourse.Description.Length < 3)
                {
                    message = "Description must be greater than 3 characters!";
                }
                else if (updateCourse.Price < 0)
                {
                    message = "Price must be greater than 0!";
                }

                //if List catalog name is null
                if (updateCourse.CatalogIDs == null)
                {
                    message = "List catalog ID is null!";
                }
                else if (updateCourse.CatalogIDs.Count == 0)
                {
                    message = "List catalog ID is empty!";
                }
                else
                {
                    foreach (var catalogID in updateCourse.CatalogIDs)
                    {
                        //check exist catalog name
                        if (!(await _catalogService.CatalogExists(catalogID))._isSuccess)
                        {
                            message = "Catalog ID not found!";
                        }
                    }
                }

                var course = await _unitOfWork.CourseRepository.GetByIdAsync(updateCourse.ID);
                if (course is null)
                    message = "Course not found";
                //if any field is invalid, throw exception
                if (message != "")
                {
                    throw new ExceptionError(400, message);
                }

                //Get catalogCourse by course id
                var catalogCourses = await _unitOfWork.CourseCatalogRepository
                    .GetAllAsync(x => x.CourseID.Equals(updateCourse.ID));
                //update course and catalogCourse
                course.Name = updateCourse.Name;
                course.Description = updateCourse.Description;
                course.Price = updateCourse.Price;
                course.Outcome = updateCourse.Outcome;
                course.Image = updateCourse.Image;
                course.VideoIntroduction = updateCourse.VideoIntroduction;
                _unitOfWork.CourseRepository.Update(course);
                foreach (var catalogCourse in catalogCourses)
                {
                    _unitOfWork.CourseCatalogRepository.Remove(catalogCourse);
                }

                //add new catalogCourse
                foreach (var catalogID in updateCourse.CatalogIDs)
                {
                    var catalog = (await _catalogService.CatalogExists(catalogID))._data;
                    var catalogCourse = new CourseCatalog
                    {
                        CourseID = updateCourse.ID,
                        CatalogID = catalog.ID
                    };
                    _unitOfWork.CourseCatalogRepository.Add(catalogCourse);
                }

                //save change
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResultDTO<string>> DeleteCourse(Guid id)
        {
            try
            {
                var course = await _unitOfWork.CourseRepository.GetByIdAsync(id);

                if (course is null)
                    return ResultDTO<string>.Fail("Course is not found", 404);

                course.IsDeleted = true;
                _unitOfWork.CourseRepository.Update(course);
                await _unitOfWork.CommitAsync();
                var users = await _userManager.GetUsersInRoleAsync("User");
                await Task.WhenAll(users.Select(user => _cartService.RemoveItemAsync(Guid.Parse(user.Id), id)));

                return ResultDTO<string>.Success("", "Course is deleted");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<CourseListDTO>> GetInstructorCoursesAsync()
        {
            try
            {
                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<CourseListDTO>.Fail("Instructor is not found");

                var courseDTOs = (await GetCoursesAsync(true)).Where(c => c.Instructor.ID == instructor.ID);

                var result = new CourseListDTO()
                {
                    List = courseDTOs.ToList(),
                    Total = courseDTOs.Count()
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<CourseListDTO>> GetPaidCoursesAsync()
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                if (user is null)
                    return ResultDTO<CourseListDTO>.Fail("User not found", 404);

                var paidCourseIds =
                    (await _unitOfWork.CourseRepository.GetUserPaidCoursesAsync(Guid.Parse(user.Id))).Select(course =>
                        course.ID).ToList();


                var courseDTOs = (await GetCoursesAsync(false)).ToList();
                courseDTOs = courseDTOs.Where(c => paidCourseIds.Contains(c.ID)).ToList();

                var result = new CourseListDTO()
                {
                    List = courseDTOs,
                    Total = courseDTOs.Count
                };

                return ResultDTO<CourseListDTO>.Success(result);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseListDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<PublicCourseDetailDTO>> GetPublicCourseDetailAsync(Guid courseId)
        {
            try
            {
                var courseDetailDto = await GetCourseDetailAsync(courseId);
                if (courseDetailDto is null)
                {
                    return ResultDTO<PublicCourseDetailDTO>.Fail("Course does not exist");
                }

                return ResultDTO<PublicCourseDetailDTO>.Success(_mapper.Map<PublicCourseDetailDTO>(courseDetailDto));
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<PublicCourseDetailDTO>.Fail("Error occurred during processing");
            }
        }

        public async Task<bool> CanCurrentUserAccessCourseAsync(Guid courseId, string userRole)
        {
            try
            {
                if (userRole == "Instructor")
                {
                    var instructor = await _instructorService.GetCurrentInstructor();
                    if (instructor is null)
                        return false;
                    var course = await _unitOfWork.CourseRepository.Courses
                        .Where(c => c.ID == courseId && c.InstructorID == instructor.ID)
                        .Select(c => c.ID.ToString()).FirstOrDefaultAsync();

                    return course is not null;
                }

                if (userRole == "User")
                {
                    var user = await _userService.GetCurrentUser();
                    if (user is null)
                        return false;
                    return (await _unitOfWork.CourseRepository.GetUserPaidCoursesAsync(Guid.Parse(user.Id)))
                        .FirstOrDefault(c => c.ID == courseId) is not null;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<ResultDTO<CourseDetailDTO>> GetInstructorCourseDetailAsync(Guid courseId)
        {
            try
            {
                var instructor = await _instructorService.GetCurrentInstructor();
                if (instructor is null)
                    return ResultDTO<CourseDetailDTO>.Fail("User not found", 404);
                var course =
                    await _unitOfWork.CourseRepository.GetByIdAsync(courseId);

                if (course is null || course.InstructorID != instructor.ID)
                {
                    return ResultDTO<CourseDetailDTO>.Fail("Course does not exist", 404);
                }

                var courseDetailDto =
                    await GetCourseDetailAsync(courseId);

                return ResultDTO<CourseDetailDTO>.Success(courseDetailDto);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseDetailDTO>.Fail("Error occurred during processing");
            }
        }

        public async Task<ResultDTO<CourseDetailDTO>> GetPaidCourseDetailAsync(Guid courseId)
        {
            try
            {
                var canAccess = await CanCurrentUserAccessCourseAsync(courseId, "User");
                if (canAccess)
                {
                    return ResultDTO<CourseDetailDTO>.Fail("Course does not exist", 404);
                }

                var courseDetailDto = await GetCourseDetailAsync(courseId);

                return ResultDTO<CourseDetailDTO>.Success(courseDetailDto);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<CourseDetailDTO>.Fail("Error occurred during processing");
            }
        }
    }
}