using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentSectionReferenceRepository : IGenericRepository<EconsentSectionReference>
    {
        IList<EconsentSectionReferenceDto> GetSectionReferenceList(bool isDeleted, int documentId);
        List<DropDownDto> GetEconsentDocumentSectionDropDown(int documentId);
        EconsentSectionReferenceDocumentType GetEconsentSectionReferenceDocument(int id);    
        IList<EconcentSectionRefrenceDetailListDto> GetSetionRefefrenceDetailList(int documentId, int sectionNo);
        EconsentSectionReferenceDocumentType GetEconsentSectionReferenceDocumentNew(int id);
        List<EconsentSectionReferenceDocument> GetEconsentSectionReferenceDocumentByUser();
    }
}
