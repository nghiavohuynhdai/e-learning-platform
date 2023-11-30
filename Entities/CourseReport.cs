using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Entities
{
    public class CourseReport : BaseEntity
    {
        public Guid ReportID { get; set; }
        public Guid CourseID { get; set; }
    }
}
