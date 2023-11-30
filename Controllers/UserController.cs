using Cursus.Constants;
using Cursus.DTO.User;
using Cursus.Services;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRedisService _cacheService;

        public UserController(IUserService userService, IRedisService cacheService)
        {
            _userService = userService;
            _cacheService = cacheService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAll();
            return StatusCode(result._statusCode, result);
        }

        [HttpGet("get-user-profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var result = await _userService.GetUserProfile();
            return StatusCode(result._statusCode, result);
        }

        [HttpPut("update-user-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfileUpdateDTO user)
        {
            var result = await _userService.UpdateUserProfile(user);
            if (result._isSuccess)
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.User + User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            return StatusCode(result._statusCode, result);
        }

        [HttpPut("update-user-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateUserStatusDTO user)
        {
            var result = await _userService.UpdateUserStatus(user.Id, user.Status);
            if (result._isSuccess)
                await _cacheService.RemoveDataAsync(CacheKeyPatterns.User + User.Claims.FirstOrDefault(c => c.Type == "Id").Value);
            return StatusCode(result._statusCode, result);
        }
    }
}