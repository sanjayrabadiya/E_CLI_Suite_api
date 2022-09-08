using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentGlossaryRepository : IGenericRepository<EconsentGlossary>
    {
        List<DropDownDto> GetEconsentDocumentWordDropDown(int documentId);
        IList<EconsentGlossaryGridDto> GetGlossaryList(bool isDeleted, int EconsentSetupId);
    }
}
