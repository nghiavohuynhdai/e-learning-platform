using Cursus.DTO.Course;
using Cursus.DTO;
using Cursus.Entities;
using Cursus.DTO.Catalog;

namespace Cursus.Services.Interfaces
{
    public interface ICatalogService
    {
        Task<ResultDTO<IEnumerable<CatalogDTO>>> GetAll();
        Task<ResultDTO<CatalogDTO>> AddCatalog(CatalogCreateDTO catalogRequest);
        Task<ResultDTO<string>> DeleteCatalog(Guid ID);
        Task<ResultDTO<Catalog>> CatalogExists(Guid id);
        Task<ResultDTO<string>> UpdateCatalog(CatalogDTO updateCatalog);
    }
}