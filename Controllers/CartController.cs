using Amazon.Runtime.Internal;
using Cursus.DTO.Cart;
using Cursus.Entities;
using Cursus.Repositories.Interfaces;
using Cursus.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cursus.DTO;
using Cursus.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Cursus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly IUserService _userService;

        public CartController(ICartService service, IUserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet("")]
        public async Task<ActionResult<CartResponse>> Get()
        {
            var user = await _userService.GetCurrentUser();
            if (user is null)
                return NotFound(ResultDTO.Fail(new[] { "User not found" }));
            var result = await _service.GetByUserIdAsync(Guid.Parse(user.Id));
            return StatusCode(result._statusCode, result);
        }

        [HttpPut("add-to-cart")]
        public async Task<ActionResult<AddOrRemoveCartRequest>> AddToCart(AddOrRemoveCartRequest request)
        {
            var user = await _userService.GetCurrentUser();
            if (user is null)
                return NotFound(ResultDTO.Fail(new[] { "User not found" }));

            if (string.IsNullOrEmpty(request.CourseID))
                return BadRequest(ResultDTO.Fail(new[] { "Course ID is required" }));

            var addToCart = await _service.AddToCartAsync(Guid.Parse(user.Id), Guid.Parse(request.CourseID));
            if (!addToCart._isSuccess)
            {
                return NotFound(addToCart);
            }

            return Ok(addToCart);
        }

        [HttpDelete("remove-item")]
        public async Task<ActionResult<AddOrRemoveCartRequest>> RemoveItem(AddOrRemoveCartRequest request)
        {
            var user = await _userService.GetCurrentUser();
            if (user is null)
                return NotFound(ResultDTO.Fail(new[] { "User not found" }));

            if (string.IsNullOrEmpty(request.CourseID))
                return BadRequest(ResultDTO.Fail(new[] { "Course ID is required" }));
            var remove = await _service.RemoveItemAsync(Guid.Parse(user.Id), Guid.Parse(request.CourseID));
            if (!remove.IsSuccess)
            {
                return NotFound(remove);
            }

            return Ok(remove);
        }
    }
}