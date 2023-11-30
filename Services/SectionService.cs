using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;

namespace Cursus.Services
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInstructorService _instructorService;
        private readonly IQuizRepository _quizRepository;

        public SectionService(IUnitOfWork unitOfWork, IInstructorService instructorService,
            IQuizRepository quizRepository)
        {
            _unitOfWork = unitOfWork;
            _instructorService = instructorService;
            _quizRepository = quizRepository;
        }

        public async Task<Section> GetAsync(Guid id, Guid instructorID)
        {
            try
            {
                var section = await _unitOfWork.SectionRepository.GetByIdAsync(id);
                if (section is null || section.InstructorID != instructorID)
                    return null;

                return section;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Section> CreateSection(Section section)
        {
            var instructor = await _instructorService.GetCurrentInstructor();
            if (instructor is null)
                return null;

            Course course;
            try
            {
                course = await _unitOfWork.CourseRepository.GetByIdAsync(section.CourseID);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

            if (course is null || course.InstructorID != instructor.ID)
                throw new Exception("Course not found");

            try
            {
                var sectionNoMax = 1;
                var sectionNoList =
                    (await _unitOfWork.SectionRepository.GetManyAsync(sec => sec.CourseID == section.CourseID))
                    .Select(sec => sec.No).ToList();
                if (sectionNoList.Any())
                    sectionNoMax = sectionNoList.Max() + 1;

                section.No = sectionNoMax;
                section.InstructorID = instructor.ID;
                await _unitOfWork.SectionRepository.AddAsync(section);
                await _unitOfWork.CommitAsync();
                return section;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Section> UpdateSection(Section section)
        {
            try
            {
                _unitOfWork.SectionRepository.Update(section);
                await _unitOfWork.CommitAsync();
                return section;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


        public async Task<Section> Delete(Section section)
        {
            try
            {
                _unitOfWork.SectionRepository.Remove(section);
                var sections = await _unitOfWork.SectionRepository.GetManyAsync(sec =>
                    sec.CourseID == section.CourseID && sec.No > section.No);
                sections = sections.Select(sec =>
                {
                    sec.No -= 1;
                    return sec;
                });
                _unitOfWork.SectionRepository.UpdateRange(sections);
                await _unitOfWork.CommitAsync();
                return section;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<int> CalculateNewSectionItemNo(Guid sectionID)
        {
            var noMaxList = new List<int>();
            try
            {
                var lessonNoMax = 0;
                var assignmentNoMax = 0;
                var quizNoMax = 0;
                {
                    var lessonNoList =
                        (await _unitOfWork.LessonRepository.GetManyAsync(les => les.SectionID == sectionID))
                        .Select(les => les.No).ToList();
                    var assignmentNoList =
                        (await _unitOfWork.AssignmentRepository.GetManyAsync(assignment =>
                            assignment.SectionID == sectionID))
                        .Select(assignment => assignment.No).ToList();
                    var quizNoList =
                        (await _quizRepository.GetManyAsync(filter => filter.SectionID == sectionID))
                        .Select(quiz => quiz.No).ToList();
                    if (lessonNoList.Count != 0)
                        lessonNoMax = lessonNoList.Max();
                    if (assignmentNoList.Count != 0)
                        assignmentNoMax = assignmentNoList.Max();
                    if (quizNoList.Count != 0)
                        quizNoMax = quizNoList.Max();
                    noMaxList.AddRange(new[]
                    {
                        lessonNoMax, assignmentNoMax, quizNoMax
                    });
                }

                return noMaxList.Max() + 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        public async Task<int> UpdateSectionItemNoAfterDeleteItem(int itemNo, Guid sectionId)
        {
            try
            {
                var lessons =
                    await _unitOfWork.LessonRepository.GetManyAsync(les =>
                        les.SectionID == sectionId && les.No > itemNo);
                var assignments =
                    await _unitOfWork.AssignmentRepository.GetManyAsync(assig =>
                        assig.SectionID == sectionId && assig.No > itemNo);
                var quiz = await _quizRepository.GetManyAsync(filter =>
                    filter.SectionID == sectionId && filter.No > itemNo);
                lessons = lessons.Select(
                    les =>
                    {
                        les.No -= 1;
                        return les;
                    });
                assignments = assignments.Select(
                    assig =>
                    {
                        assig.No -= 1;
                        return assig;
                    });
                quiz = quiz.Select(
                    qz =>
                    {
                        qz.No -= 1;
                        return qz;
                    }
                ).ToList();
                _unitOfWork.LessonRepository.UpdateRange(lessons);
                _unitOfWork.AssignmentRepository.UpdateRange(assignments);
                foreach (var qz in quiz)
                {
                    _quizRepository.Update(filter => filter.ID == qz.ID, qz);
                }

                await _unitOfWork.CommitAsync();

                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }
    }
}