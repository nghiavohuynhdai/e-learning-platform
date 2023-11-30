namespace Cursus.DTO.Course
{
    public class CreateCourseResDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public double Price { get; set; }
        public Guid InstructorID { get; set; }
    }
}
