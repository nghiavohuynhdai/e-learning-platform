using Cursus.DTO;
using Cursus.DTO.Instructor;
using Cursus.Entities;

namespace Cursus.Services.Interfaces;

public interface IInstructorService
{
    Task<Instructor> GetCurrentInstructor();
    Task<ResultDTO<List<InstructorPublicProfileDTO>>> GetAllInstructorPublicProfile();
    Task<ResultDTO<InstructorPublicProfileDTO>> GetInstructorPublicProfile(Guid instructorId);
}