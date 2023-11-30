using Cursus.DTO;
using Cursus.DTO.Assignment;
using Cursus.Entities;

namespace Cursus.Services.Interfaces
{
    public interface IAssignmentService
    {
        Task<Assignment> GetAsync(Guid id, Guid instructorId);
        Task<Assignment> CreateAsync(Assignment assignment);
        Task<Assignment> UpdateAsync(Assignment assignment);
        Task<Assignment> DeleteAsync(Assignment assignment);
    }
}
