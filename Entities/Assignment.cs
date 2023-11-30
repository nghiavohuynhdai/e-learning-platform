using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Entities
{
    public class Assignment : BaseEntity
    {
        public int No { get; set; }
        public string Title { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        public int TimeTaken { get; set; }
        public Guid SectionID { get; set; }
        public Guid CourseID { get; set; }
        public Guid InstructorID { get; set; }
    }
}