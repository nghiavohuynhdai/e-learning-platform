using System.Linq.Expressions;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly MyDbContext _context;

        public LessonRepository(MyDbContext context)
        {
            _context = context;
        }

        public IQueryable<Lesson> Lessons => _context.Lessons.AsQueryable();

        public async Task<IEnumerable<Lesson>> GetManyAsync(Expression<Func<Lesson, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _context.Lessons
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Lesson>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync(les => les.CourseID == courseId, cancellationToken);
        }

        public async Task<Lesson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Lessons
                .FirstOrDefaultAsync(les => les.ID == id, cancellationToken);
        }

        public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken)
        {
            await _context.Lessons.AddAsync(lesson, cancellationToken);
        }

        public void Update(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
        }

        public void UpdateRange(IEnumerable<Lesson> lessons)
        {
            _context.Lessons.UpdateRange(lessons);
        }

        public void Remove(Lesson lesson)
        {
            _context.Lessons.Remove(lesson);
        }
    }
}