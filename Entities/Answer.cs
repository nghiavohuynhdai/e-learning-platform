using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Entities
{
    public class Answer : BaseEntity
    {
        public string Title { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        public string AnswerFile { get; set; }
        public int Score { get; set; }
        [Column(TypeName = "text")]
        public string Feedback { get; set; }
        public Guid UserID { get; set; }
        public Guid AssignmentID { get; set; }
    }
}
