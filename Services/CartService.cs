using System.Text;
using System.Text.Json;
using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.Cart;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using StackExchange.Redis;

namespace Cursus.Services
{
    public class CartService : ICartService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IRedisService _redisService;

        public CartService(IUnitOfWork unitOfWork,
            IUserService userService, IRedisService redisService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _redisService = redisService;
        }

        public async Task<ResultDTO<CartResponse>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var cartResponse = new CartResponse
                {
                    Items = new List<CartItemDto>(),
                    UserID = userId.ToString()
                };
                var data = await _redisService.RedisDb.HashGetAllAsync($"cart:{userId}");
                if (data is null) return ResultDTO<CartResponse>.Success(cartResponse);
                var courses = await _unitOfWork.CourseRepository
                    .GetManyAsync(c => data.Select(entry => Guid.Parse(entry.Name.ToString()))
                        .Contains(c.ID));

                async Task<CartItemDto> GetCartItemAsync(Course course)
                {
                    var cartItem = _mapper.Map<CartItemDto>(course);
                    var value = data.FirstOrDefault(entry => entry.Name.ToString() == course.ID.ToString()).Value;
                    await using (Stream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                    {
                        var deserializedValue = await JsonSerializer.DeserializeAsync<CartItem>(memoryStream);
                        cartItem.CreateDate = deserializedValue.CreatedDate;
                    }

                    return cartItem;
                }

                var cartItems = await Task.WhenAll(courses.Select(c => GetCartItemAsync(c)));
                cartResponse.Items = cartItems.ToList();

                return ResultDTO<CartResponse>.Success(cartResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<CartResponse>.Fail("Failed to get cart");
            }
        }

        public async Task<ResultDTO<CartItem>> AddToCartAsync(Guid userId, Guid courseId)
        {
            try
            {
                var course =
                    await _unitOfWork.CourseRepository.GetByIdAsync(courseId);

                if (course is null || course.IsDeleted)
                {
                    return ResultDTO<CartItem>.Fail("Course is not Existed");
                }

                var courses = await _unitOfWork.CourseRepository.GetUserPaidCoursesAsync(userId);

                if (courses.Any(course => course.ID == courseId))
                    return ResultDTO<CartItem>.Fail("You owned this course");

                var cartItem = new CartItem
                {
                    CourseId = course.ID,
                    CreatedDate = DateTime.UtcNow
                };
                string serializedValue;
                await using (Stream memoryStream = new MemoryStream())
                {
                    await JsonSerializer.SerializeAsync(memoryStream, cartItem);
                    memoryStream.Position = 0;
                    using (var streamReader = new StreamReader(memoryStream))
                    {
                        serializedValue = await streamReader.ReadToEndAsync();
                    }
                }

                await _redisService.RedisDb.HashSetAsync($"cart:{userId}", courseId.ToString(),
                    serializedValue);

                return ResultDTO<CartItem>.Success(cartItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<CartItem>.Fail("Failed to add to cart");
            }
        }

        public async Task<ResultDTO> RemoveItemAsync(Guid userId, Guid courseId)
        {
            try
            {
                await _redisService.RedisDb.HashDeleteAsync($"cart:{userId}", courseId.ToString());
                return ResultDTO.Success();
            }
            catch (Exception ex)
            {
                return ResultDTO.Fail(new[] { "Failed to remove: " + ex.Message });
            }
        }

        public async Task<bool> RemoveManyItemsAsync(Guid userId, IEnumerable<Guid> courseIds)
        {
            try
            {
                await _redisService.RedisDb.HashDeleteAsync($"cart:{userId}",
                    courseIds.Select(id => new RedisValue(id.ToString())).ToArray());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> RemoveAllAsync(Guid userId)
        {
            try
            {
                await _redisService.RedisDb.KeyDeleteAsync($"cart:{userId}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}