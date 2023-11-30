using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Entities
{
    public class CourseFeedback : BaseEntity
    {
        public Guid CourseID { get; set; }
        public Guid UserID { get; set; }
        public string Description {  get; set; }
        public int Rate { get; set; }
    }
}
