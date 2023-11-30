using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Cursus.Entities
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid QuestionID { get; set; }

        public string QuestionName { get; set; }

        public bool IsMuti { get; set; }

        public List<Option> Options { get; set; }
    }
}
