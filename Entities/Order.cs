using Cursus.Constants;

namespace Cursus.Entities
{
    public class Order : BaseEntity
    {
        public string Code { get; set; }
        public double TotalPrice { get; set; }
        public string PaymentUrl { get; set; }
        public string Status { get; set; }
        public Guid UserID { get; set; }
    }
}
