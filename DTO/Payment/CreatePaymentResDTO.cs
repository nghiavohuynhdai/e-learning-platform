using Cursus.Constants;

namespace Cursus.DTO.Payment
{
    public class CreatePaymentResDTO
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public double TotalPrice { get; set; }
        public string PaymentUrl { get; set; }
        public OrderStatus Status { get; set; }
        public Guid UserID { get; set; }
        public List<Guid> courseId { get; set; }
    }
}
