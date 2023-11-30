using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;

namespace Cursus.Repositories
{
    public class CourseCatalogRepository : BaseRepository<CourseCatalog>, ICourseCatalogRepository
    {
        public CourseCatalogRepository(MyDbContext context) : base(context)
        {
        }
    }
}