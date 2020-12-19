using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public interface IAppScreenPatientRightsRepository : IGenericRepository<AppScreenPatientRights>
    {
       List<AppScreenPatientRightsGridDto> GetAppScreenPatientList(int projectid);
        //List<AppScreenPatientRightsDto> GetAppScreenPatientModules(int projectid);
    }
}
