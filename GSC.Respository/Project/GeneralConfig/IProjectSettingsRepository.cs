using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Generalconfig;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface IProjectSettingsRepository : IGenericRepository<ProjectSettings>
    {
        List<ProjectDropDown> GetParentProjectDropDownEicf();
    }
}
