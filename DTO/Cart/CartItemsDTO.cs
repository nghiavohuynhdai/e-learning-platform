namespace Cursus.DTO.Cart
{
    public class CartItemDto
    {
        public string CourseID { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public Guid InstructorID { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
