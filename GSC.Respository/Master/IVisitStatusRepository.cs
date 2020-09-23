using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using System.Collections.Generic;

namespace GSC.Respository.Master
{
    public interface IVisitStatusRepository : IGenericRepository<VisitStatus>
    {
        string Duplicate(VisitStatus objSave);
        List<DropDownDto> GetVisitStatusDropDown();
        List<VisitStatusGridDto> GetVisitStatusList(bool isDeleted);
        List<DropDownDto> GetAutoVisitStatusDropDown();
    }
}