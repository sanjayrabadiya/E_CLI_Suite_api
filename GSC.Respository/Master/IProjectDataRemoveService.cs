using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public interface IProjectDataRemoveService
    {

        Task<ProjectRemoveDataSuccess> AdverseEventRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> InformConsentRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> AttendenceDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> CTMSDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> LabManagementDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> MedraDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> ETMFDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> ScreeningDataRemove(ProjectRemoveDataDto obj);

        Task<ProjectRemoveDataSuccess> DesignDataRemove(ProjectRemoveDataDto obj);
    }
}
