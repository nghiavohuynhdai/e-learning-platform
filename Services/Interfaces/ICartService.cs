using Cursus.DTO;
using Cursus.DTO.Cart;

namespace Cursus.Services.Interfaces
{
    public interface ICartService
    {
        Task<ResultDTO<CartResponse>> GetByUserIdAsync(Guid userId);
        Task<ResultDTO<CartItem>> AddToCartAsync(Guid userId, Guid courseId);
        Task<ResultDTO> RemoveItemAsync(Guid userId, Guid courseId);
        Task<bool> RemoveManyItemsAsync(Guid userId, IEnumerable<Guid> courseIds);
        Task<bool> RemoveAllAsync(Guid userId);
    }
}