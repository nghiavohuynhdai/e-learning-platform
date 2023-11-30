namespace Cursus.DTO.Quiz
{
    public class CreateQuizReq
    {
        public string Name { get; set; }
        public int TimeTaken { get; set; }
        public List<CreateQuestionReq> Questions { get; set; }
    }
}
