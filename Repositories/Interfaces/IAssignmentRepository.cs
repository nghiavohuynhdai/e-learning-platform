using System.Linq.Expressions;
using Cursus.Entities;

namespace Cursus.Repositories.Interfaces
{
    public interface IAssignmentRepository
    {
        IQueryable<Assignment> Assignments { get; }

        Task<IEnumerable<Assignment>> GetManyAsync(Expression<Func<Assignment, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Assignment>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default);

        Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Assignment assignment, CancellationToken cancellationToken = default);
        void Update(Assignment assignment);
        void UpdateRange(IEnumerable<Assignment> assignments);
        void Remove(Assignment assignment);
    }
}