using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDesignTrialRepository : IGenericRepository<DesignTrial>
    {
        List<DropDownDto> GetDesignTrialDropDown();
        List<DropDownDto> GetDesignTrialDropDownByTrialType(int id);
        string Duplicate(DesignTrial objSave);
        List<DesignTrialGridDto> GetDesignTrialList(bool isDeleted);
    }
}