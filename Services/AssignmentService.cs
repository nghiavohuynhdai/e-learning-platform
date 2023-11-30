using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.Assignment;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;

namespace Cursus.Services
{
    public class AssignmentService : IAssignmentService

    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISectionService _sectionService;

        public AssignmentService(IUnitOfWork unitOfWork, ISectionService sectionService)
        {
            _unitOfWork = unitOfWork;
            _sectionService = sectionService;
        }

        public async Task<Assignment> GetAsync(Guid id, Guid instructorId)
        {
            try
            {
                var assignment = await _unitOfWork.AssignmentRepository.GetByIdAsync(id);
                if (assignment is null || assignment.InstructorID != instructorId)
                    return null;
                return assignment;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<Assignment> CreateAsync(Assignment assignment)
        {
            try
            {
                var assignmentNo = await _sectionService.CalculateNewSectionItemNo(assignment.SectionID);
                if (assignmentNo == -1)
                    throw new Exception("Fail to calculate new section item No");
                assignment.No = assignmentNo;
                await _unitOfWork.AssignmentRepository.AddAsync(assignment);
                await _unitOfWork.CommitAsync();
                return assignment;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<Assignment> UpdateAsync(Assignment assignment)
        {
            try
            {
                _unitOfWork.AssignmentRepository.Update(assignment);
                await _unitOfWork.CommitAsync();
                return assignment;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public async Task<Assignment> DeleteAsync(Assignment assignment)
        {
            try
            {
                _unitOfWork.AssignmentRepository.Remove(assignment);
                var num = await _sectionService.UpdateSectionItemNoAfterDeleteItem(assignment.No, assignment.SectionID);
                if (num == 0) return null;
                await _unitOfWork.CommitAsync();
                return assignment;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}