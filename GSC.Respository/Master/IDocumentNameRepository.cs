using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDocumentNameRepository : IGenericRepository<DocumentName>
    {
        List<DropDownDto> GetDocumentDropDown(int documentId);
        string Duplicate(DocumentName objSave);
    }
}