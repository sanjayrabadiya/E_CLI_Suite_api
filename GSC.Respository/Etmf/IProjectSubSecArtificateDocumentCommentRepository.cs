using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using System.Collections.Generic;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentCommentRepository : IGenericRepository<ProjectSubSecArtificateDocumentComment>
    {
        IList<ProjectSubSecArtificateDocumentCommentDto> GetComments(int documentId);
    }
}