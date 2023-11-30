using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Cursus.Entities
{
    public class Quiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid ID { get; set; }

        public int No { get; set; }

        public string Name { get; set; }

        public Guid IntructorID { get; set; }

        public Guid CourseID { get; set; }

        public int TimeTaken { get; set; }

        public string Status { get; set; }

        public Guid SectionID { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public List<Question> Questions { get; set; }
    }
}
