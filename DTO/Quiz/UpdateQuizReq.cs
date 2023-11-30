using Cursus.Entities;

namespace Cursus.DTO.Quiz
{
    public class UpdateQuizReq
    {
        public string Name { get; set; }
        public int TimeTaken { get; set; }
        public List<Question> Questions { get; set; }
    }
}
