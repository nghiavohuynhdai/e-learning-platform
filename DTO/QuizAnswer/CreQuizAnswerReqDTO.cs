using Cursus.Entities;

namespace Cursus.DTO.QuizAnswer
{
    public class CreQuizAnswerReqDTO
    {
        public Guid QuizID { get; set; }
        //public int Score { get; set; }
        public List<CreQuestionAnswerReq> QuestionAnswer { get; set; }
    }
    
}
