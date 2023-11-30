using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;

namespace Cursus.Repositories
{
    public class CourseFeedbackRepository : BaseRepository<CourseFeedback>, ICourseFeedbackRepository
    {
        public CourseFeedbackRepository(MyDbContext context) : base(context)
        {
        }
    }
}