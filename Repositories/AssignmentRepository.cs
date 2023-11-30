using System.Linq.Expressions;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly MyDbContext _context;

        public AssignmentRepository(MyDbContext context)
        {
            _context = context;
        }

        public IQueryable<Assignment> Assignments => _context.Assignments.AsQueryable();

        public async Task<IEnumerable<Assignment>> GetManyAsync(Expression<Func<Assignment, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _context.Assignments
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Assignment>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync(assig => assig.CourseID == courseId, cancellationToken);
        }

        public async Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Assignments
                .FirstOrDefaultAsync(assig => assig.ID == id, cancellationToken);
        }

        public async Task AddAsync(Assignment assignment, CancellationToken cancellationToken)
        {
            await _context.Assignments.AddAsync(assignment, cancellationToken);
        }

        public void Update(Assignment assignment)
        {
            _context.Assignments.Update(assignment);
        }

        public void UpdateRange(IEnumerable<Assignment> assignments)
        {
            _context.Assignments.UpdateRange(assignments);
        }

        public void Remove(Assignment assignment)
        {
            _context.Assignments.Remove(assignment);
        }
    }
}