using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cursus.DTO.Catalog;
using Cursus.Entities;
using Cursus.Repositories;
using AutoMapper;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Cursus.DTO;
using Cursus.DTO.Course;

namespace Cursus.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CatalogService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResultDTO<IEnumerable<CatalogDTO>>> GetAll()
        {
            try
            {
                var catalogs =
                    _mapper.Map<IEnumerable<CatalogDTO>>(await _unitOfWork.CatalogRepository.GetAllAsync());
                return ResultDTO<IEnumerable<CatalogDTO>>.Success(catalogs);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return ResultDTO<IEnumerable<CatalogDTO>>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<CatalogDTO>> AddCatalog(CatalogCreateDTO catalogRequest)
        {
            if (string.IsNullOrEmpty(catalogRequest.Name))
            {
                return ResultDTO<CatalogDTO>.Fail("Name is required.", 400);
            }

            var catalog = new Catalog
            {
                Name = catalogRequest.Name
            };
            try
            {
                await _unitOfWork.CatalogRepository.AddAsync(catalog);
                await _unitOfWork.CommitAsync();
                var catalogResponse = _mapper.Map<CatalogDTO>(catalog);
                return ResultDTO<CatalogDTO>.Success(catalogResponse);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<CatalogDTO>.Fail("Failed to add catalog");
            }
        }

        public async Task<ResultDTO<string>> DeleteCatalog(Guid ID)
        {
            var catalogExistsResult = await CatalogExists(ID);

            if (catalogExistsResult._data != null)
            {
                try
                {
                    _unitOfWork.CatalogRepository.Remove(catalogExistsResult._data);
                    var courseCatalogs = _unitOfWork.CourseCatalogRepository.GetQueryable()
                        .Where(cc => cc.CatalogID == ID).AsEnumerable();
                    _unitOfWork.CourseCatalogRepository.RemoveRange(courseCatalogs);
                    await _unitOfWork.CommitAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return ResultDTO<string>.Fail("Service is not available");
                }

                return ResultDTO<string>.Success("", statusCode: 204);
            }

            return ResultDTO<string>.Fail("Catalog not found.", 404);
        }

        public async Task<ResultDTO<Catalog>> CatalogExists(Guid id)
        {
            try
            {
                var catalog = await _unitOfWork.CatalogRepository.GetAsync(c => c.ID.Equals(id));

                if (catalog != null)
                {
                    return ResultDTO<Catalog>.Success(catalog);
                }

                return ResultDTO<Catalog>.Fail("Catalog not exists", 404);
            }
            catch (Exception ex)
            {
                return ResultDTO<Catalog>.Fail("Service is not available");
            }
        }

        public async Task<ResultDTO<string>> UpdateCatalog(CatalogDTO updateCatalog)
        {
            if (string.IsNullOrEmpty(updateCatalog.Name))
                return ResultDTO<string>.Fail("Catalog name is required", 400);

            try
            {
                var catalog = await _unitOfWork.CatalogRepository.GetAsync(c => c.ID.Equals(updateCatalog.ID));

                if (catalog == null)
                    return ResultDTO<string>.Fail("Catalog ID not found!", 404);

                if (updateCatalog.Name.Length < 3 || updateCatalog.Name.Length > 50)
                    return ResultDTO<string>.Fail("Name must be between 3 and 50 characters!");

                catalog.Name = updateCatalog.Name;
                await _unitOfWork.CommitAsync();
                return ResultDTO<string>.Success("", statusCode: 204);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ResultDTO<string>.Fail("Service is not available");
            }
        }
    }
}