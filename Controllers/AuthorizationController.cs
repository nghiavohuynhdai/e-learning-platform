using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Authorization;
using Cursus.Models.DTO;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthorizationController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-user")]
        public async Task<IActionResult> Registration([FromBody] RegisterDTO register)
        {
            var result = await _authService.UserRegistration(register);
            return StatusCode(result._statusCode, result);
        }

        [HttpPost("register-instructor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegistrationInstructor([FromBody] InstructorRegisterDTO register)
        {
            var result = await _authService.InstructorRegistration(register);
            return StatusCode(result._statusCode, result);
        }

        // [HttpPost("register-admin")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> RegistrationAdmin([FromBody] RegisterDTO register)
        // {
        //     var result = await _authService.AdminRegistration(register);
        //     return StatusCode(result._statusCode, result);
        // }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var result = await _authService.Login(model);
            return StatusCode(result._statusCode, result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            var result = await _authService.ChangePassword(model);
            return StatusCode(result._statusCode, result);
        }
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleRequest token)
        {
            var result = await _authService.GoogleLogin(token);
            return StatusCode(result._statusCode, result);
        }
    }
}