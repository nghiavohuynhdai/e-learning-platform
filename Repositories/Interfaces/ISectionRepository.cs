using System.Linq.Expressions;
using Cursus.Entities;

namespace Cursus.Repositories.Interfaces
{
    public interface ISectionRepository
    {
        IQueryable<Section> Sections { get; }

        Task<IEnumerable<Section>> GetManyAsync(Expression<Func<Section, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Section>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default);

        Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAsync(Section section, CancellationToken cancellationToken = default);
        void Update(Section section);
        void UpdateRange(IEnumerable<Section> sections);
        void Remove(Section section);
    }
}