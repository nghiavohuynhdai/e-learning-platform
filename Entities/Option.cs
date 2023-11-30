using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Cursus.Entities
{
    public class Option
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Option_ID { get; set; }

        public string OptionText { get; set; }

        public bool Iscorrect { get; set; }
    }
}
