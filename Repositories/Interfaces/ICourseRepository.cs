using System.Linq.Expressions;
using Cursus.Entities;

namespace Cursus.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        IQueryable<Course> Courses { get; }

        Task<IEnumerable<Course>> GetManyAsync(Expression<Func<Course, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<Course>> GetUserPaidCoursesAsync(Guid userId, CancellationToken cancellationToken = default);

        IQueryable<CourseUser> GetCourseUserJoin();

        Task AddAsync(Course course, CancellationToken cancellationToken = default);

        void Update(Course course);

        void UpdateRange(IEnumerable<Course> courses);

        void Remove(Course course);
    }
}