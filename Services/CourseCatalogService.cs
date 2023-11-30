using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cursus.DTO.CourseCatalog;
using Cursus.Entities;
using Cursus.Repositories;
using AutoMapper;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Cursus.DTO;

namespace Cursus.Services
{
    public class CourseCatalogService : ICourseCatalogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseCatalogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseCatalogResDTO>> GetAll()
        {
            var courseCatalogs = await _unitOfWork.CourseCatalogRepository.GetAllAsync();
            var courseCatalogResponses = _mapper.Map<IEnumerable<CourseCatalogResDTO>>(courseCatalogs);
            return courseCatalogResponses;
        }

        public async Task<ResultDTO<CourseCatalogResDTO>> AddCourseCatalog(CourseCatalogReqDTO courseCatalogRequest)
        {
            var courseCatalog = new CourseCatalog
            {
                CatalogID = courseCatalogRequest.CatalogID,
                CourseID = courseCatalogRequest.CourseID
            };

            try
            {
                _unitOfWork.CourseCatalogRepository.Add(courseCatalog);
                await _unitOfWork.CommitAsync();

                var courseCatalogResponse = _mapper.Map<CourseCatalogResDTO>(courseCatalog);
                return ResultDTO<CourseCatalogResDTO>.Success(courseCatalogResponse);
            }
            catch (Exception ex)
            {
                return ResultDTO<CourseCatalogResDTO>.Fail("Failed to add Course Catalog: " + ex.Message);
            }
        }


        public async Task<bool> DeleteCatalogByID(Guid id)
        {
            var courseCatalog = await _unitOfWork.CourseCatalogRepository.GetAsync(x => x.ID == id);

            if (courseCatalog == null)
            {
                return false; // Course catalog not found.
            }

            _unitOfWork.CourseCatalogRepository.Remove(courseCatalog);
            await _unitOfWork.CommitAsync();

            return true; // Course catalog deleted successfully.
        }

        public async Task<CourseCatalogResDTO> GetCatalogByID(Guid id)
        {
            var courseCatalog = await _unitOfWork.CourseCatalogRepository.GetAsync(x => x.ID == id);
            var courseCatalogResponse = _mapper.Map<CourseCatalogResDTO>(courseCatalog);
            return courseCatalogResponse;
        }
    }
}
