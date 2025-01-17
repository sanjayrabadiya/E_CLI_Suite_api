﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.LabManagement;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementConfigurationRepository : IGenericRepository<LabManagementConfiguration>
    {
        string Duplicate(Data.Entities.LabManagement.LabManagementConfiguration objSave);
        List<LabManagementConfigurationGridDto> GetConfigurationList(int ProjectId, bool isDeleted);
        T[] GetMappingData<T>(int LabManagementConfigurationId);
        List<ProjectDropDown> GetParentProjectDropDownForUploadLabData();
        IList<DropDownDto> GetVisitDropDownForUploadLabData(int projectDesignPeriodId);
        IList<DropDownDto> GetTemplateDropDownForUploadLabData(int projectDesignVisitId);
        // Get Project design variable id by lab management configuration Id
        int getProjectDesignVariableId(int LabManagementConfigurationId, string VariableName);
        List<DropDownDto> EmailUsers(int ProjectId);
    }
}