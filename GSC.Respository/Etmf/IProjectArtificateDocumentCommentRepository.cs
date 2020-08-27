using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectArtificateDocumentCommentRepository : IGenericRepository<ProjectArtificateDocumentComment>
    {
        IList<ProjectArtificateDocumentCommentDto> GetComments(int documentId);
    }
}
