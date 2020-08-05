using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDocumentTypeRepository : IGenericRepository<DocumentType>
    {
        List<DropDownDto> GetDocumentDropDown();
        string Duplicate(DocumentType objSave);
        List<DocumentTypeGridDto> GetDocumentTypeList(bool isDeleted);
    }
}