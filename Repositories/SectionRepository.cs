using System.Linq.Expressions;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private readonly MyDbContext _context;

        public SectionRepository(MyDbContext context)
        {
            _context = context;
        }

        public IQueryable<Section> Sections => _context.Sections.AsQueryable();

        public async Task<IEnumerable<Section>> GetManyAsync(Expression<Func<Section, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _context.Sections
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Section>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync(sec => sec.CourseID == courseId, cancellationToken);
        }

        public async Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Sections
                .FirstOrDefaultAsync(sec => sec.ID == id, cancellationToken);
        }

        public async Task AddAsync(Section section, CancellationToken cancellationToken)
        {
            await _context.Sections.AddAsync(section, cancellationToken);
        }

        public void Update(Section section)
        {
            _context.Sections.Update(section);
        }

        public void UpdateRange(IEnumerable<Section> sections)
        {
            _context.Sections.UpdateRange(sections);
        }

        public void Remove(Section section)
        {
            _context.Sections.Remove(section);
        }
    }
}