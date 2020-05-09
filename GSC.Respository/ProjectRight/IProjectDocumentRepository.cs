using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;

namespace GSC.Respository.ProjectRight
{
    public interface IProjectDocumentRepository : IGenericRepository<ProjectDocument>
    {
        string Duplicate(ProjectDocument objSave);

        List<ProjectDocumentDto> GetDocument(int id);
    }
}