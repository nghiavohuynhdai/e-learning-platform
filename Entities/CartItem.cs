using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class CartItem
{
    // [BsonElement("CourseId")]
    public Guid CourseId { get; set; }
    public DateTime CreatedDate { get; set; }
}
