using Cursus.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using payment.DTO;
using payment.Services;

namespace payment.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public PaymentController(IVnPayService vnPayService, IConfiguration configuration)
        {
            _vnPayService = vnPayService;
            _configuration = configuration;
        }
        [HttpPost("create-payment-url")]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentReqDTO model)
        {
            if(model == null) { 
            return NoContent();
            }
            var payment = await _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (!payment._isSuccess)
            {
                return NotFound(payment);
            }
            return Ok(payment);
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = await _vnPayService.PaymentExecute(Request.Query);
            if (!response._isSuccess)
            {
                return Redirect($"https://expertmind-aca.vercel.app/payment-status?status=unsuccess&id={response._data.OrderId}");
            }
            return Redirect($"https://expertmind-aca.vercel.app/payment-status?status=success&id={response._data.OrderId}");
        }
        [HttpGet("get-payment-by-id")]
        public async Task<IActionResult> GetOrder(string code)
        {
            var response = await _vnPayService.GetOrderByCode(code);
            if (!response._isSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        [HttpGet("my-payment")]
        public async Task<IActionResult> GetAllOrder()
        {
            var response = await _vnPayService.GetMyPayments();
            if (!response._isSuccess)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}
