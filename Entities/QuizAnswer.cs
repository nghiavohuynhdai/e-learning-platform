using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cursus.Entities
{
    public class QuizAnswer
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
                 
        public Guid QuizId { get; set; }
       
        public double Score { get; set; }
        
        public Guid UserID { get; set; }
 
        public List<QuestionAnswer> QuestionAnswer { get; set; }
    }
}
