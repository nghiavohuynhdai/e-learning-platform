using Cursus.DTO;
using Cursus.DTO.CourseCatalog;

namespace Cursus.Services.Interfaces
{
    public interface ICourseCatalogService
    {
        Task<IEnumerable<CourseCatalogResDTO>> GetAll();
        Task<ResultDTO<CourseCatalogResDTO>> AddCourseCatalog(CourseCatalogReqDTO courseCatalogRequest);

        Task<bool> DeleteCatalogByID(Guid id);

        Task<CourseCatalogResDTO> GetCatalogByID(Guid id);
    }
}
