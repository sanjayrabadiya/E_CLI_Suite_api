using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IPassThroughCostRepository : IGenericRepository<PassThroughCost>
    {
        IList<PassThroughCostGridDto> GetpassThroughCostGrid(bool isDeleted, int studyId);
        string Duplicate(PassThroughCostDto passThroughCostDto);
        List<DropDownPassThroughCostDto> GetCountriesDropDown(int projectId);
        PassThroughCost ConvertIntoGlobuleCurrency(PassThroughCost passThroughCost);
    }
}
