namespace Cursus.DTO.Course
{
    public class CourseRequest
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public bool Status { get; set; }
        public double Price { get; set; }
        public Guid InstructorID { get; set; }
    }
}
