using Cursus.DTO;
using Cursus.DTO.Authorization;
using Cursus.Models.DTO;

namespace Cursus.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<ResultDTO<string>> UserRegistration(RegisterDTO model);
        public Task<ResultDTO<string>> InstructorRegistration(InstructorRegisterDTO model);
        public Task<ResultDTO<string>> AdminRegistration(RegisterDTO model);
        public Task<ResultDTO<LoginResponseDTO>> Login(LoginDTO model);
        public Task<ResultDTO<ChangePasswordDTO>> ChangePassword(ChangePasswordDTO model);
        public Task<ResultDTO<LoginResponseDTO>> GoogleLogin(GoogleRequest request);
    }
}
