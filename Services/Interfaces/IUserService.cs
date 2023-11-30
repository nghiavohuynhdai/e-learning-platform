using Cursus.DTO;
using Cursus.DTO.User;
using Cursus.Entities;

namespace Cursus.Services
{
    public interface IUserService
    {
        public Task<User> GetCurrentUser();
        public Task<ResultDTO<UserProfileDTO>> GetUserProfile();
        public Task<ResultDTO<List<UserDTO>>> GetAll();
        Task<ResultDTO<string>> UpdateUserProfile(UserProfileUpdateDTO updateUser);
        Task<ResultDTO<string>> UpdateUserStatus(Guid id, string status);
    }
}
