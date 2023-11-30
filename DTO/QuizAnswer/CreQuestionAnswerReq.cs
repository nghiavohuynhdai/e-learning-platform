using Cursus.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace Cursus.DTO.QuizAnswer
{
    public class CreQuestionAnswerReq
    {    
        
        public Guid QuestionID { get; set; }
        public List<CreOptionAnswerReqDTO> OptionAnswers { get; set; }
    }
}
