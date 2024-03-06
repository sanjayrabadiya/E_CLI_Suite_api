using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IProcedureRepository : IGenericRepository<Procedure>
    {
        string Duplicate(Procedure objSave);
        List<ProcedureGridDto> GetProcedureList(bool isDeleted);
        List<DropDownProcedureDto> GetParentProjectDropDown();
    }
}