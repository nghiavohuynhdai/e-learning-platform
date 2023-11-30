

using Cursus.Data.Interface;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Cursus.Repositories
{
    public class QuizAnswerRepository : IQuizAnswerRepository
    {
        private readonly IMongoDbContext _context;
        private readonly IMongoCollection<QuizAnswer> _collection;

        public QuizAnswerRepository(IMongoDbContext context)
        {
            _context = context;
            _collection = context.QuizAnswer;

        }
        public void Create(QuizAnswer quiz)
        {
            _collection.InsertOne(quiz);
        }

        public QuizAnswer GetAsync(Expression<Func<QuizAnswer, bool>> filter)
        {
            return _collection.Find(filter).FirstOrDefault();
        }

        public List<QuizAnswer> GetAll()
        {
            return _collection.Find(FilterDefinition<QuizAnswer>.Empty).ToList();
        }

        public void Remove(Expression<Func<QuizAnswer, bool>> filter)
        {
            _collection.DeleteOne(filter);
        }

        public void Update(Expression<Func<QuizAnswer, bool>> filter, QuizAnswer quiz)
        {
            _collection.ReplaceOne(filter, quiz);
        }
    }
}
