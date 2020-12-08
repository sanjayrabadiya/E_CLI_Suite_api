using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;

namespace GSC.Respository.Project.Design
{
    public interface IProjectDesignPeriodRepository : IGenericRepository<ProjectDesignPeriod>
    {
        ProjectDesignPeriod GetPeriod(int id);
        IList<DropDownDto> GetPeriodDropDown(int projectDesignId);
        IList<DropDownWithSeqDto> GetPeriodByProjectIdDropDown(int projectId);

    }
}