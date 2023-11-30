using AutoMapper;
using Cursus.Constants;
using Cursus.DTO;
using Cursus.DTO.Instructor;
using Cursus.Entities;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace Cursus.Services.Interfaces;

public class InstructorService : IInstructorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IRedisService _redisService;

    public InstructorService(IMapper mapper, IUnitOfWork unitOfWork, UserManager<User> userManager,
        IUserService userService, IRedisService redisService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _userService = userService;
        _mapper = mapper;
        _redisService = redisService;
    }

    public async Task<Instructor> GetCurrentInstructor()
    {
        var user = await _userService.GetCurrentUser();
        if (user is null)
            return null;

        try
        {
            var key = CacheKeyPatterns.Instructor + user.Id;
            var instructor = await _redisService.GetDataAsync<Instructor>(key);
            if (instructor is not null) return instructor;
            instructor = await _unitOfWork.InstructorRepository.GetAsync(i =>
                i.UserID.ToString() == user.Id
            );
            await _redisService.SetDataAsync(key, instructor, TimeSpan.FromDays(1));

            return instructor;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<ResultDTO<List<InstructorPublicProfileDTO>>> GetAllInstructorPublicProfile()
    {
        try
        {
            var instructors = await _unitOfWork.InstructorRepository.GetAllAsync();

            var instructorUserId = instructors.Select(instructor => instructor.UserID.ToString());
            var users = _userManager.Users.Where(
                    u => u.Status != Enum.GetName(UserStatus.Disable) &&
                         instructorUserId.Contains(u.Id))
                .ToList();

            var result = _userManager.Users.Where(u => u.Status == Enum.GetName(UserStatus.Enable))
                .Join(
                    _unitOfWork.InstructorRepository.GetQueryable(),
                    u => u.Id,
                    i => i.UserID.ToString(),
                    (u, i) => _mapper.Map(i, _mapper.Map<InstructorPublicProfileDTO>(u))
                ).ToList();

            return ResultDTO<List<InstructorPublicProfileDTO>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ResultDTO<List<InstructorPublicProfileDTO>>.Fail("Service is not available");
        }
    }

    public async Task<ResultDTO<InstructorPublicProfileDTO>> GetInstructorPublicProfile(Guid instructorId)
    {
        try
        {
            var instructor = await _unitOfWork.InstructorRepository.GetAsync(i => i.ID == instructorId);
            if (instructor is null)
                return ResultDTO<InstructorPublicProfileDTO>.Fail("Not found", 404);

            var user = _userManager.Users.FirstOrDefault(u => u.Id == instructor.UserID.ToString());
            if (user is null)
                return ResultDTO<InstructorPublicProfileDTO>.Fail("Not found", 404);

            var result = _mapper.Map(instructor, _mapper.Map<InstructorPublicProfileDTO>(user));
            return ResultDTO<InstructorPublicProfileDTO>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ResultDTO<InstructorPublicProfileDTO>.Fail("Service is not available");
        }
    }
}