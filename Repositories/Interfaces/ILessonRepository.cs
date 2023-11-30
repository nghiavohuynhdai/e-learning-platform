using System.Linq.Expressions;
using Cursus.Entities;

namespace Cursus.Repositories.Interfaces
{
    public interface ILessonRepository
    {
        IQueryable<Lesson> Lessons { get; }

        Task<IEnumerable<Lesson>> GetManyAsync(Expression<Func<Lesson, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Lesson>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default);

        Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
        void Update(Lesson lesson);
        void UpdateRange(IEnumerable<Lesson> lessons);
        void Remove(Lesson lesson);
    }
}