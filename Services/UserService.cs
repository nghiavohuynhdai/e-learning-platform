using System.Security.Claims;
using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.User;
using Cursus.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Cursus.Constants;
using Cursus.Data;
using Cursus.Services.Interfaces;

namespace Cursus.Services
{
    public class UserService : IUserService
    {
        private readonly MyDbContext _context;
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly IRedisService _redisService;

        public UserService(MyDbContext context,
            IHttpContextAccessor httpContextAccessor, IRedisService redisService)
        {
            _context = context;
            _claimsPrincipal = httpContextAccessor.HttpContext.User;
            _redisService = redisService;
        }

        public async Task<User> GetCurrentUser()
        {
            var userIdClaim = _claimsPrincipal.Claims
                .FirstOrDefault(c => c.Type == "Id");

            if (userIdClaim is null)
                return null;
            try
            {
                var user = await _redisService.GetDataAsync<User>(CacheKeyPatterns.User + userIdClaim.Value);
                if (user is null)
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdClaim.Value);
                }

                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<ResultDTO<UserProfileDTO>> GetUserProfile()
        {
            var user = await GetCurrentUser();
            if (user is not null)
            {
                var role = await _context.Roles
                    .Where(r =>
                        _context.UserRoles
                            .Where(uRoles => uRoles.UserId == user.Id)
                            .Select(uRoles => uRoles.RoleId)
                            .Contains(r.Id)
                    )
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();
                return ResultDTO<UserProfileDTO>.Success(new UserProfileDTO()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Image = user.Image,
                    Gender = user.Gender,
                    Role = role
                });
            }

            return ResultDTO<UserProfileDTO>.Fail("Not Found", 404);
        }

        public async Task<ResultDTO<List<UserDTO>>> GetAll()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userRoles = await _context.UserRoles.Join(
                    _context.Roles.AsQueryable(),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new
                    {
                        UserId = ur.UserId,
                        RoleName = r.Name
                    }).ToListAsync();

                var userDTOs = users.Select(u =>
                    new UserDTO()
                    {
                        Id = Guid.Parse(u.Id),
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Username = u.UserName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Address = u.Address,
                        Image = u.Image,
                        Gender = u.Gender,
                        Role = userRoles.FirstOrDefault(ur => ur.UserId == u.Id)?.RoleName,
                        Status = u.Status,
                        CreatedDate = u.CreatedDate,
                        UpdatedDate = u.UpdatedDate
                    }
                ).ToList();

                return ResultDTO<List<UserDTO>>.Success(userDTOs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<List<UserDTO>>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UpdateUserProfile(UserProfileUpdateDTO updateUser)
        {
            try
            {
                const string PHONE_NUMBER_PATTERN = @"^(\+84|0)(3|5|7|8|9)([0-9]{8})$";

                // Check all fields are valid
                if (string.IsNullOrEmpty(updateUser.FirstName) ||
                    string.IsNullOrEmpty(updateUser.LastName) ||
                    string.IsNullOrEmpty(updateUser.PhoneNumber) ||
                    string.IsNullOrEmpty(updateUser.Address)
                   )
                {
                    return ResultDTO<string>.Fail("All fields are empty!", 400);
                }

                if (!Regex.IsMatch(updateUser.PhoneNumber, PHONE_NUMBER_PATTERN))
                    return ResultDTO<string>.Fail("Invalid phone number format!", 400);
                var userGender = "";
                if (!string.IsNullOrEmpty(updateUser.Gender))
                    if (!Enum.TryParse<UserGender>(updateUser.Gender, out var gender))
                        return ResultDTO<string>.Fail("Invalid gender!", 400);
                    else
                        userGender = Enum.GetName(gender);

                // Get user by ID
                var currentUser = await GetCurrentUser();

                if (currentUser is null)
                    return ResultDTO<string>.Fail("Not Found", 404);

                // Update user information
                currentUser.FirstName = updateUser.FirstName;
                currentUser.LastName = updateUser.LastName;
                currentUser.PhoneNumber = updateUser.PhoneNumber;
                currentUser.Address = updateUser.Address;
                currentUser.Image = updateUser.Image ?? "";
                currentUser.Gender = userGender;

                await Task.Run(() => _context.Users.Update(currentUser));
                await _context.SaveChangesAsync();

                return ResultDTO<string>.Success("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UpdateUserStatus(Guid id, string status)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id.ToString());
                if (user is null)
                    return ResultDTO<string>.Fail("User is not found", 404);

                if (!Enum.TryParse<UserStatus>(status, out var userStatus))
                    return ResultDTO<string>.Fail("Invalid user status", 400);

                user.Status = Enum.GetName(userStatus);
                await Task.Run(() => _context.Update(user));
                await _context.SaveChangesAsync();

                return ResultDTO<string>.Success("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }
    }
}