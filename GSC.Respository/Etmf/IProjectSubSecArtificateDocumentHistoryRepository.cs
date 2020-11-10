using GSC.Common.GenericRespository;
using GSC.Data.Entities.Etmf;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentHistoryRepository : IGenericRepository<ProjectSubSecArtificateDocumentHistory>
    {
        void AddHistory(ProjectWorkplaceSubSecArtificatedocument projectWorkplaceSubSecArtificatedocument, int? ReviewId, int? ApproverId);
    }
}