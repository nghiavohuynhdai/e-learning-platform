using System.ComponentModel.DataAnnotations;
using Cursus.Constants;
using Cursus.Data;
using Cursus.DTO;
using Cursus.DTO.Authorization;
using Cursus.Entities;
using Cursus.Models.DTO;
using Cursus.Repositories;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Cursus.DTO.Cart;
using Cursus.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Services
{
    public class AuthService : IAuthService
    {
        const string PHONE_NUMBER_PATTERN = @"^(\+84|0)(3|5|7|8|9)([0-9]{8})$";

        // Expire time in minute
        private const int TOKEN_EXPIRE_TIME = 60 * 24 * 30; // 1 months
        private const int ADMIN_TOKEN_EXPIRE_TIME = 60; // 1 hours

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;
        private readonly IGoogleService _googleService;

        public AuthService(IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            ICartService cartService,
            IUserService userService,
            IGoogleService googleService
        )
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _cartService = cartService;
            _userService = userService;
            _googleService = googleService;
        }

        public async Task<ResultDTO<ChangePasswordDTO>> ChangePassword(ChangePasswordDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                    return ResultDTO<ChangePasswordDTO>.Fail("Current password is required", 400);

                if (string.IsNullOrEmpty(model.NewPassword))
                    return ResultDTO<ChangePasswordDTO>.Fail("New password is required", 400);

                if (string.IsNullOrEmpty(model.ConfirmNewPassword))
                    return ResultDTO<ChangePasswordDTO>.Fail("Confirm new password is required", 400);

                // lets find the user
                var user = await _userService.GetCurrentUser();
                if (user is null)
                    throw new Exception("Get current user result is null");

                // check current password
                if (!await _userManager.CheckPasswordAsync(user, model.CurrentPassword))
                {
                    return ResultDTO<ChangePasswordDTO>.Fail("Password Is Wrong", 400);
                }

                //check CurrentPass and NewPass
                if (model.CurrentPassword.Equals(model.NewPassword))
                {
                    return ResultDTO<ChangePasswordDTO>.Fail("Password Duplicate", 400);
                    
                }

                if (!model.ConfirmNewPassword.Equals(model.NewPassword))
                    return ResultDTO<ChangePasswordDTO>.Fail("Confirm New Password does not match New Password", 400);

                // change password here
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    return ResultDTO<ChangePasswordDTO>.Fail(result.Errors.Select(err => err.Description));
                }

                return ResultDTO<ChangePasswordDTO>.Success(null, "Password has changed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<ChangePasswordDTO>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UserRegistration(RegisterDTO model)
        {
            var result = await Registration(model, "User");

            if (!result._isSuccess)
                return ResultDTO<string>.Fail(result._message, result._statusCode);

            return ResultDTO<string>.Success("", result._message[0]);
        }

        public async Task<ResultDTO<string>> InstructorRegistration(InstructorRegisterDTO model)
        {
            var result = await Registration(model, "Instructor");

            if (!result._isSuccess)
                return ResultDTO<string>.Fail(result._message, result._statusCode);

            try
            {
                var instructor = new Instructor()
                {
                    Bio = model.Bio ?? "",
                    Career = model.Career ?? "",
                    UserID = Guid.Parse((ReadOnlySpan<char>)result._data)
                };
                await _unitOfWork.InstructorRepository.AddAsync(instructor);
                await _unitOfWork.CommitAsync();
                return ResultDTO<string>.Success("", result._message[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> AdminRegistration(RegisterDTO model)
        {
            var result = await Registration(model, "Admin");

            if (!result._isSuccess)
                return ResultDTO<string>.Fail(result._message, result._statusCode);

            return ResultDTO<string>.Success("", result._message[0]);
        }

        public async Task<ResultDTO<LoginResponseDTO>> Login(LoginDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    return ResultDTO<LoginResponseDTO>.Fail("Email is required", 400);

                if (string.IsNullOrEmpty(model.Password))
                    return ResultDTO<LoginResponseDTO>.Fail("Password is required", 400);

                var user = await _userManager.Users.Where(u => u.Status == Enum.GetName(UserStatus.Enable))
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
                    return ResultDTO<LoginResponseDTO>.Fail("Email or password is wrong");

                var userClaim = new Claim("Id", user.Id);
                var userRoles = await _userManager.GetRolesAsync(user);
                var expireTime = userRoles.Contains("Admin") ? ADMIN_TOKEN_EXPIRE_TIME : TOKEN_EXPIRE_TIME;
                var token = _tokenService.GetToken(userClaim, userRoles, expireTime);
                var LoginRes = new LoginResponseDTO
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    Expire = token.ValidTo
                };
                return ResultDTO<LoginResponseDTO>.Success(LoginRes);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<LoginResponseDTO>.Fail("Service is not available");
            }
        }

        private async Task<ResultDTO<string>> Registration(RegisterDTO register, string role)
        {
            if (string.IsNullOrEmpty(register.Username))
                return ResultDTO<string>.Fail("Username is required", 400);

            if (string.IsNullOrEmpty(register.Email))
                return ResultDTO<string>.Fail("Email is required", 400);

            if (!new EmailAddressAttribute().IsValid(register.Email))
                return ResultDTO<string>.Fail("Invalid email", 400);

            if (string.IsNullOrEmpty(register.Password))
                return ResultDTO<string>.Fail("Password is required", 400);

            if (!string.IsNullOrEmpty(register.PhoneNumber) && !Regex.IsMatch(register.PhoneNumber, PHONE_NUMBER_PATTERN))
                return ResultDTO<string>.Fail("Invalid phone number format", 400);

            try
            {
                var userExits = await _userManager.Users.Where(u => u.Status == Enum.GetName(UserStatus.Enable))
                    .ToListAsync();
                if (userExits.FirstOrDefault(u => u.Email == register.Email) is not null)
                {
                    return ResultDTO<string>.Fail("Email had been registered");
                }

                // if (userExits.FirstOrDefault(u => u.UserName == register.Username) is not null)
                // {
                //     return ResultDTO<string>.Fail("Username had been used");
                // }

                var user = new User
                {
                    UserName = register.Username,
                    FirstName = register.FirstName ?? "",
                    LastName = register.LastName ?? "",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Email = register.Email,
                    Address = register.Address ?? "",
                    PhoneNumber = register.PhoneNumber ?? "",
                    Status = Enum.GetName(UserStatus.Enable),
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    // Image = register.Image,
                    Gender = ""
                };
                var result = await _userManager.CreateAsync(user, register.Password);

                if (!result.Succeeded)
                {
                    return ResultDTO<string>.Fail(result.Errors.Select(err => err.Description));
                }

                if (!await _roleManager.RoleExistsAsync(role))                  
                    return ResultDTO<string>.Fail("Role is not exist");

                if (await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }

                return ResultDTO<string>.Success(user.Id, "Successfully registered");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<LoginResponseDTO>> GoogleLogin(GoogleRequest request)
        {
            try
            {
                var validPayload = await _googleService.VerifyGoogleTokenAsync(request.Token);

                if (validPayload != null)
                {
                    // Check if the user already exists in your database based on email

                    var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Email == validPayload.Email);
                    //check if existingUser is exit in db 
                    if (existingUser == null)
                    {
                        var user = new User
                        {
                            UserName = validPayload.Email,
                            FirstName = validPayload.GivenName,
                            LastName = validPayload.FamilyName,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            Email = validPayload.Email,
                            Address = "",
                            PhoneNumber = "",
                            Status = Enum.GetName(UserStatus.Enable),
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            Image = validPayload.Picture,
                            Gender = ""
                        };
                        
                        
                        var result = await _userManager.CreateAsync(user);
                        if (!result.Succeeded)
                        {
                            return ResultDTO<LoginResponseDTO>.Fail("invalid condition");
                        }
                        await _userManager.AddToRoleAsync(user, "User");
                        existingUser = user;

                    }
                    //check disable
                    var enUser = await _userManager.Users.Where(u => u.Status == Enum.GetName(UserStatus.Disable))
                         .FirstOrDefaultAsync(u => u.Email == validPayload.Email);

                    if (enUser != null)
                    {
                        return ResultDTO<LoginResponseDTO>.Fail("user disable");
                    }

                    //generate JWT token 
                    var userClaim = new Claim("Id", existingUser.Id);
                    var userRoles = await _userManager.GetRolesAsync(existingUser);
                    var expireTime = userRoles.Contains("Admin") ? ADMIN_TOKEN_EXPIRE_TIME : TOKEN_EXPIRE_TIME;
                    var token = _tokenService.GetToken(userClaim, userRoles, expireTime);
                    var LoginRes = new LoginResponseDTO
                    {
                        AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                        Expire = token.ValidTo
                    };

                    return ResultDTO<LoginResponseDTO>.Success(LoginRes);
                }

                return ResultDTO<LoginResponseDTO>.Fail("Invalid Google token");
            }
            catch (Exception ex)
            {
                return ResultDTO<LoginResponseDTO>.Fail("Server fail");
            }
        }
    }
}