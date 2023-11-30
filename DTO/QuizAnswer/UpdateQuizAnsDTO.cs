using Cursus.Entities;

namespace Cursus.DTO.QuizAnswer
{
    public class UpdateQuizAnsDTO
    {
        public Guid Id { get; set; }

        public Guid QuizId { get; set; }

        public int Score { get; set; }

        public Guid UserID { get; set; }

        public List<QuestionAnswer> QuestionAnswer { get; set; }
    }
}
