using Cursus.Entities;

namespace Cursus.DTO.Quiz
{
    public class CreateQuestionReq
    {
        public string QuestionName { get; set; }
        public bool IsMuti { get; set; }

        public List<CreateOptionReq> Options { get; set; }
    }
}
