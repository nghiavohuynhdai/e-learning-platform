namespace Cursus.DTO.Payment
{
    public class CreatePaymentReqDTO
    {
        public double Amount { get; set; }
        public List<Guid> courseId { get; set; }
    }
}
