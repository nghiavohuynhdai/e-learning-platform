namespace Cursus.Entities
{
    public class OrderDetail : BaseEntity
    {
        public Guid OrderID { get; set; }  
        public Guid CourseID { get; set; }
    }
}
