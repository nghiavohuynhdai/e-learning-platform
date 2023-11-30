namespace Cursus.DTO.Admin;

public class CourseWithLearnerQuantityDTO
{
    public Guid ID { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public string Image { get; set; }
    public bool IsDeleted { get; set; }
    public Guid InstructorID { get; set; }
    public int LearnerQuantity { get; set; }
}