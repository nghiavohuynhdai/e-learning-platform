using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Order;
using Cursus.DTO.Payment;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using payment.DTO;
using payment.Services;

namespace Cursus.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICartService _cartService;

        public VnPayService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration,
            IUserService userService, ICartService cartService)
        {
            _configuration = configuration;
            _userService = userService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public async Task<ResultDTO<CreatePaymentResDTO>> CreatePaymentUrl(CreatePaymentReqDTO model,
            HttpContext context)
        {
            var courses = await _unitOfWork.CourseRepository.GetManyAsync(c => model.courseId.Contains(c.ID));
            if (courses.Any(c => c.IsDeleted))
                return ResultDTO<CreatePaymentResDTO>.Fail("Some courses have been deleted");

            if (model.Amount < 5000 || model.Amount > 1000000000)
            {
                return ResultDTO<CreatePaymentResDTO>.Fail("Amount must be between 5000 and 1000000000");
            }

            try
            {
                var user = await _userService.GetCurrentUser();
                var userId = Guid.Parse(user.Id);
                var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
                var tick = DateTime.Now.Ticks.ToString();
                var pay = new VnPayLibrary();
                var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];


                pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
                pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
                pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
                pay.AddRequestData("vnp_Amount", ((double)model.Amount * 100).ToString());
                pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
                pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
                pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
                pay.AddRequestData("vnp_OrderInfo",
                    $"{user.FirstName} {user.LastName} da thanh toan so tien {model.Amount} voi hoa don {tick}");
                pay.AddRequestData("vnp_OrderType", "250000");
                pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
                pay.AddRequestData("vnp_TxnRef", tick);

                var paymentUrl =
                    pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

                var _payment = new Order
                {
                    ID = Guid.NewGuid(),
                    Code = tick,
                    PaymentUrl = paymentUrl,
                    TotalPrice = model.Amount,
                    Status = Enum.GetName(OrderStatus.Pending),
                    UserID = userId,
                };


                _unitOfWork.OrderDetailRepository.AddRangeAsync(courses.Select(c =>
                    new OrderDetail
                    {
                        CourseID = c.ID,
                        OrderID = _payment.ID,
                    }
                ));

                _unitOfWork.OrderRepository.AddAsync(_payment);
                await _unitOfWork.CommitAsync();

                await _cartService.RemoveManyItemsAsync(userId, model.courseId);
                var payment = _mapper.Map<CreatePaymentResDTO>(_payment);
                payment.courseId = model.courseId;
                return ResultDTO<CreatePaymentResDTO>.Success(payment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<CreatePaymentResDTO>.Fail("service is not available");
            }
        }

        public async Task<ResultDTO<List<OrderDetailDto>>> GetMyPayments()
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                var userId = Guid.Parse(user.Id);

                var orders = await _unitOfWork.OrderRepository.GetAllAsync(c => c.UserID.Equals(userId));


                if (orders.Any())
                {
                    var orderIds = orders.Select(o => o.ID).ToList();

                    var orderDetails = await _unitOfWork.OrderDetailRepository
                        .GetAllAsync(detail => orderIds.Contains(detail.OrderID));

                    var courseIds = orderDetails.Select(detail => detail.CourseID).ToList();

                    var orderDetailDtos = orders.Select(order =>
                    {
                        var orderDetailDto = _mapper.Map<OrderDetailDto>(order);
                        orderDetailDto.CourseId = orderDetails
                            .Where(detail => detail.OrderID == order.ID)
                            .Select(detail => detail.CourseID)
                            .ToList();

                        return orderDetailDto;
                    }).ToList();

                    orderDetailDtos.ForEach(dto => dto.CourseId = courseIds);

                    return ResultDTO<List<OrderDetailDto>>.Success(orderDetailDtos);
                }
                else
                {
                    return ResultDTO<List<OrderDetailDto>>.Fail("You don't have any orders yet.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMyPayments: {ex.Message}");
                return ResultDTO<List<OrderDetailDto>>.Fail("An unexpected error occurred.");
            }
        }


        public async Task<ResultDTO<OrderDetailDto>> GetOrderByCode(string code)
        {
            var order = await _unitOfWork.OrderRepository.GetAsync(c => c.Code.Equals(code));
            if (order == null)
            {
                return ResultDTO<OrderDetailDto>.Fail("Order not found");
            }

            var courseIds = (await _unitOfWork.OrderDetailRepository.GetAllAsync(detail =>
                detail.OrderID == order.ID
            )).Select(detail => detail.CourseID);

            var orderDetailDto = _mapper.Map<OrderDetailDto>(order);
            orderDetailDto.CourseId = courseIds;

            return ResultDTO<OrderDetailDto>.Success(orderDetailDto);
        }

        public async Task<ResultDTO<PaymentResponseModel>> PaymentExecute(IQueryCollection collections)
        {
            try
            {
                var pay = new VnPayLibrary();
                var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

                if (response.VnPayResponseCode.Equals("00"))
                {
                    var order = _unitOfWork.OrderRepository.Get(c => c.Code == response.OrderId);

                    if (order != null)
                    {
                        order.Status = Enum.GetName(OrderStatus.Completed);
                        _unitOfWork.OrderRepository.Update(order);
                        _unitOfWork.Commit();
                        return ResultDTO<PaymentResponseModel>.Success(response);
                    }

                    return ResultDTO<PaymentResponseModel>.Fail("Update status order fail");
                }

                return ResultDTO<PaymentResponseModel>.Fail("order payment failed");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<PaymentResponseModel>.Fail("service is not available");
            }
        }
    }
}