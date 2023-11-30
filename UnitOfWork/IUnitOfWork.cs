using Cursus.Repositories.Interfaces;

namespace Cursus.UnitOfWork
{
    public interface IUnitOfWork
    {
        IAnswerRepository AnswerRepository { get; }
        IAssignmentRepository AssignmentRepository { get; }
        ICatalogRepository CatalogRepository { get; }
        ICourseCatalogRepository CourseCatalogRepository { get; }
        ICourseFeedbackRepository CourseFeedbackRepository { get; }
        ICourseReportRepository CourseReportRepository { get; }
        ICourseRepository CourseRepository { get; }
        IInstructorRepository InstructorRepository { get; }
        ILessonRepository LessonRepository { get; }
        INotificationRepository NotificationRepository { get; }
        IOrderRepository OrderRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        IReportRepository ReportRepository { get; }
        ISectionRepository SectionRepository { get; }
        IUserReportRepository UserReportRepository { get; }
        IUserRepository UserRepository { get; }
        void Commit();
        void Rollback();
        Task CommitAsync();
        Task RollbackAsync();
    }
}