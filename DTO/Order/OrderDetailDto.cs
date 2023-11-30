namespace Cursus.DTO.Order;

public class OrderDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public double TotalPrice { get; set; }
    public string PaymentUrl { get; set; }
    public string Status { get; set; }
    public IEnumerable<Guid> CourseId { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CourseDto
{
}