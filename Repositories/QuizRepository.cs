using System.Linq.Expressions;
using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Cursus.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly IMongoDbContext _context;
        private readonly IMongoCollection<Quiz> _collection;

        public QuizRepository(IMongoDbContext context)
        {
            _context = context;
            _collection = context.Quiz;
        }

        public IMongoQueryable<Quiz> Quizzes => _collection.AsQueryable();

        public async Task<IEnumerable<Quiz>> GetAllAsync()
        {
            return await Quizzes.ToListAsync();
        }

        public async Task<IEnumerable<Quiz>> GetManyAsync(Expression<Func<Quiz, bool>> filter,
            CancellationToken cancellationToken)
        {
            return await Quizzes
                .Where(filter)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Quiz>> GetManyByCourseIdAsync(Guid courseId,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync(quiz => quiz.CourseID == courseId, cancellationToken);
        }


        public async Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await Quizzes.FirstOrDefaultAsync(les => les.ID == id, cancellationToken);
        }

        public async Task CreateAsync(Quiz quiz, CancellationToken cancellationToken)
        {
            await _collection.InsertOneAsync(quiz, cancellationToken);
        }

        public void Update(Expression<Func<Quiz, bool>> filter, Quiz quiz)
        {
            _collection.ReplaceOne(filter, quiz);
        }

        public async Task RemoveAsync(Expression<Func<Quiz, bool>> filter, Quiz quiz,
            CancellationToken cancellationToken)
        {
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }

        public async Task<bool> DeleteQuestionAsync(Guid quizId, Guid questionId, CancellationToken cancellationToken)
        {
            try
            {
                var filter = Builders<Quiz>.Filter.Where(q => q.ID == quizId);
                var update = Builders<Quiz>.Update.PullFilter(q => q.Questions, q => q.QuestionID == questionId);

                var result = await _collection.UpdateOneAsync(filter, update, new UpdateOptions(), cancellationToken);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}