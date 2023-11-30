using Cursus.Entities;
using System.Linq.Expressions;
using MongoDB.Driver.Linq;

namespace Cursus.Repositories.Interfaces
{
    public interface IQuizRepository
    {
        public IMongoQueryable<Quiz> Quizzes { get; }
        Task<IEnumerable<Quiz>> GetAllAsync();

        Task<IEnumerable<Quiz>> GetManyAsync(Expression<Func<Quiz, bool>> filter,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Quiz>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default);

        Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task CreateAsync(Quiz quiz, CancellationToken cancellationToken = default);
        void Update(Expression<Func<Quiz, bool>> filter, Quiz quiz);
        Task RemoveAsync(Expression<Func<Quiz, bool>> filter, Quiz quiz, CancellationToken cancellationToken = default);
        Task<bool> DeleteQuestionAsync(Guid quizId, Guid questionId, CancellationToken cancellationToken = default);
    }
}