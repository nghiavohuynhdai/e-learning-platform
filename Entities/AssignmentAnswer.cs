namespace Cursus.Entities
{
    public class AssignmentAnswer : BaseEntity
    {
        public Guid AssignmentID { get; set; }
        public Guid AnswerID { get; set; }
        public int Score { get; set; }
        public string Feedback { get; set; }
    }
}