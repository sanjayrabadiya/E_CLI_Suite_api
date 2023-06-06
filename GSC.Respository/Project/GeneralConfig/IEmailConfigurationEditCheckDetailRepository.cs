using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using System.Collections.Generic;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface IEmailConfigurationEditCheckDetailRepository : IGenericRepository<EmailConfigurationEditCheckDetail>
    {
        EmailConfigurationEditCheckDto GetDetailList(int id);
        EmailConfigurationEditCheckDetailDto GetDetail(int id);


    }
}