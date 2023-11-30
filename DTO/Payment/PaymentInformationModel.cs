using Cursus.Constants;

namespace payment.DTO;

public class PaymentInformationModel
{
    public double Amount { get; set; }
    public string OrderDescription { get; set; }
    public string Name { get; set; }
    public List<string> courseId { get; set; }  
}