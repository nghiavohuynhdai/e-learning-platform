using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Entities
{
    public class Lesson : BaseEntity
    {
        public string Name { get; set; }
        public int No { get; set; }
        [Column(TypeName = "text")]
        public string Overview { get; set; }
        [Column(TypeName = "text")]
        public string Content { get; set; }
        public string VideoUrl { get; set; }
        public int LearningTime { get; set; }
        public Guid SectionID { get; set; }
        public Guid CourseID { get; set; }
        public Guid InstructorID { get; set; }
    }
}