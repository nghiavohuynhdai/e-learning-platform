using Cursus.Entities;
using MongoDB.Driver;

namespace Cursus.Data.Interface
{
    public interface IMongoDbContext
    {
        IMongoCollection<Quiz> Quiz { get; }
        IMongoCollection<QuizAnswer> QuizAnswer { get; }
    }
}
