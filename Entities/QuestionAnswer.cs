using MongoDB.Bson.Serialization.Attributes;

namespace Cursus.Entities
{
    public class QuestionAnswer
    {
        
        public Guid QuestionID { get; set; }
        
        public List<OptionAnswer> OptionAnswers { get; set; }
    }

    
}
