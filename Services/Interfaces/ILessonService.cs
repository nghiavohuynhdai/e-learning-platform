using Cursus.DTO;
using Cursus.DTO.Lesson;
using Cursus.Entities;

namespace Cursus.Services.Interfaces;

public interface ILessonService
{
    Task<Lesson> GetAsync(Guid id, Guid instructorId);
    Task<Lesson> CreateLesson(Lesson lesson);
    Task<Lesson> UpdateLesson(Lesson lesson);
    Task<Lesson> DeleteLesson(Lesson lesson);
}