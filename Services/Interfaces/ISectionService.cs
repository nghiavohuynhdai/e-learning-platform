using Cursus.DTO.Section;
using Cursus.DTO;
using Cursus.Entities;

namespace Cursus.Services.Interfaces
{
    public interface ISectionService
    {
        Task<Section> GetAsync(Guid id, Guid instructorID);
        Task<Section> CreateSection(Section section);
        Task<Section> UpdateSection(Section section);
        Task<Section> Delete(Section section);

        Task<int> CalculateNewSectionItemNo(Guid SectionID);
        Task<int> UpdateSectionItemNoAfterDeleteItem(int itemNo, Guid sectionId);
    }
}
