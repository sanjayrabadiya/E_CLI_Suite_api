using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;

namespace GSC.Respository.CTMS
{
    public interface IPassThroughCostActivityRepository : IGenericRepository<PassThroughCostActivity>
    {
        string Duplicate(PassThroughCostActivity objSave);
        List<PassThroughCostActivityGridDto> GetPassThroughCostActivityList(bool isDeleted);
        List<DropDownStudyDto> GetPassThroughCostActivityDropDown();
    }
}