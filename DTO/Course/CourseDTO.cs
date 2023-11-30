namespace Cursus.DTO.Course
{
    public class CourseDTO
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public string Outcome { get; set; }
        public string VideoIntroduction { get; set; }
        public List<Guid> CatalogIDs { get; set; }
        public double AvgRate { get; set; }
        public InstructorDTO Instructor { get; set; }
        public int LearnerQuantity { get; set; }
        public int LessonQuantity { get; set; }
        public int TotalTimeTaken { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InstructorDTO
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Image { get; set; }
    }
}