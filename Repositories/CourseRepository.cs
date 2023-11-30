using System.Linq.Expressions;
using Cursus.Constants;
using Cursus.Data;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly MyDbContext _context;


        public CourseRepository(MyDbContext context)
        {
            _context = context;
        }

        public IQueryable<Course> Courses => _context.Courses.AsQueryable();

        public async Task<IEnumerable<Course>> GetManyAsync(Expression<Func<Course, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _context.Courses
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.ID == id, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<Course>> GetUserPaidCoursesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var orderIds = _context.Orders.Where(order =>
                    order.UserID == userId && order.Status == Enum.GetName(OrderStatus.Completed))
                .Select(order => order.ID);

            var courseIds = _context.OrderDetails.Where(detail => orderIds.Contains(detail.OrderID))
                .Select(detail => detail.CourseID);

            var courses = await GetManyAsync(c => courseIds.Contains(c.ID), cancellationToken);
            return courses;
        }

        public IQueryable<CourseUser> GetCourseUserJoin()
        {
            var courseUserJoin = _context.Courses
                .GroupJoin(
                    _context.OrderDetails,
                    c => c.ID,
                    detail => detail.CourseID,
                    (c, details) => new
                    {
                        Course = c,
                        Details = details
                    }
                ).SelectMany(
                    c => c.Details.DefaultIfEmpty(),
                    (c, detail) => new
                    {
                        c.Course,
                        OrderId = detail != null ? detail.OrderID : Guid.Empty
                    }
                )
                .GroupJoin(
                    _context.Orders,
                    c => c.OrderId,
                    order => order.ID,
                    (c, orders) => new
                    {
                        c.Course,
                        Orders = orders
                    }
                ).SelectMany(
                    c => c.Orders.DefaultIfEmpty(),
                    (c, order) => new
                    {
                        c.Course,
                        userId = order != null ? order.UserID : Guid.Empty
                    }
                )
                .GroupJoin(
                    _context.Users,
                    c => c.userId.ToString(),
                    user => user.Id,
                    (c, users) => new
                    {
                        c.Course,
                        Users = users
                    }
                ).SelectMany(
                    c => c.Users.DefaultIfEmpty(),
                    (c, user) => new CourseUser
                    {
                        Course = c.Course,
                        User = user
                    }
                );

            return courseUserJoin;
        }

        public async Task AddAsync(Course course, CancellationToken cancellationToken)
        {
            await _context.Courses.AddAsync(course, cancellationToken);
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
        }

        public void UpdateRange(IEnumerable<Course> courses)
        {
            _context.Courses.UpdateRange(courses);
        }

        public void Remove(Course course)
        {
            _context.Courses.Remove(course);
        }
    }
}