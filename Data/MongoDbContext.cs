using Cursus.Data.Interface;
using Cursus.Entities;
using MongoDB.Driver;

namespace Cursus.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        public IMongoCollection<Quiz> Quiz { get; }
        public IMongoCollection<QuizAnswer> QuizAnswer { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("LocalMongoDb"));
            var database = client.GetDatabase("Cursus");

            Quiz = database.GetCollection<Quiz>(nameof(Quiz));
            QuizAnswer = database.GetCollection<QuizAnswer>(nameof(QuizAnswer));
        }
    }
}