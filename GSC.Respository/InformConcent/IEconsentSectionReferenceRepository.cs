using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentSectionReferenceRepository : IGenericRepository<EconsentSectionReference>
    {
        List<DropDownDto> GetEconsentDocumentSectionDropDown(int documentId);
    }
}
