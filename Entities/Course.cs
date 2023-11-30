using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Entities
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        [Column(TypeName = "text")]
        public string Outcome { get; set; }
        public string VideoIntroduction { get; set; }
        public bool IsDeleted { get; set; }
        public Guid InstructorID { get; set; }
    }
}