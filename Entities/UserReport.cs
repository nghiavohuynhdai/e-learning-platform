using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Entities
{
    public class UserReport:BaseEntity
    {
        public Guid ReportID { get; set; }
        public Guid UserID { get; set; }

    }
}
