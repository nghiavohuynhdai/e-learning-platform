using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cursus.Entities
{
    public class Report : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid UserID { get; set; }
    }
}