using AutoMapper;
using Cursus.DTO.Admin;
using Cursus.DTO.Cart;
using Cursus.DTO.Assignment;
using Cursus.DTO.Catalog;
using Cursus.DTO.Course;
using Cursus.DTO.User;
using Cursus.DTO.CourseCatalog;
using Cursus.DTO.Lesson;
using Cursus.DTO.Instructor;
using Cursus.DTO.Order;
using Cursus.Entities;
using Cursus.DTO.Section;
using Cursus.DTO.Payment;
using Cursus.DTO.Quiz;

namespace Cursus.ObjectMapping
{
    public class CursusAutoMapperProfile : Profile
    {
        public CursusAutoMapperProfile()
        {
            CreateMap<Course, CourseDTO>();
            CreateMap<Course, CreateCourseResDTO>();
            CreateMap<Catalog, CatalogDTO>();
            CreateMap<CourseCatalog, CourseCatalogResDTO>();
            CreateMap<Course, UpdateCourseDTO>();
            CreateMap<Course, CartItemDto>()
                .ForMember(des => des.CourseID, options => options.MapFrom(src => src.ID))
                .ForMember(des => des.CourseName, options => options.MapFrom(src => src.Name));
            CreateMap<User, UserDTO>();
            CreateMap<User, UserProfileDTO>();
            CreateMap<User, InstructorPublicProfileDTO>();
            CreateMap<Instructor, InstructorPublicProfileDTO>()
                .ForMember(
                    dest => dest.Career,
                    options => options.MapFrom(source => source.Career)
                );
            CreateMap<Course, PublicCourseDetailDTO>();
            CreateMap<Section, PublicCourseSectionDTO>();
            CreateMap<Lesson, PublicCourseLessonDTO>();
            CreateMap<Assignment, PublicCourseAssignmentDTO>();
            CreateMap<Quiz, PublicCourseQuizDTO>();
            CreateMap<Course, CourseDetailDTO>();
            CreateMap<Section, CourseSectionDTO>();
            CreateMap<Lesson, CourseLessonDTO>();
            CreateMap<Assignment, CourseAssignmentDTO>();
            CreateMap<Quiz, CourseQuizDTO>();
            CreateMap<Lesson, LessonDTO>();
            CreateMap<CourseDetailDTO, PublicCourseDetailDTO>();
            CreateMap<CourseSectionDTO, PublicCourseSectionDTO>();
            CreateMap<CourseLessonDTO, PublicCourseLessonDTO>();
            CreateMap<CourseAssignmentDTO, PublicCourseAssignmentDTO>();
            CreateMap<CourseQuizDTO, PublicCourseQuizDTO>();
            CreateMap<Section, UpdateSectionDTO>();
            CreateMap<Order, CreatePaymentResDTO>().ReverseMap();
            CreateMap<Order, CreatePaymentReqDTO>().ReverseMap();
            CreateMap<Order, OrderDetailDto>();
            CreateMap<Quiz, UpdateQuizReq>().ReverseMap();
            CreateMap<Course, CourseWithLearnerQuantityDTO>();
        }
    }
}