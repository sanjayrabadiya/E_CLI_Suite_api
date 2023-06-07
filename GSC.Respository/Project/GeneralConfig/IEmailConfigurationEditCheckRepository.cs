﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Screening;
using System.Collections.Generic;

namespace GSC.Respository.Project.GeneralConfig
{
    public interface IEmailConfigurationEditCheckRepository : IGenericRepository<EmailConfigurationEditCheck>
    {
        List<EmailConfigurationEditCheckGridDto> GetEmailEditCheckList(int projectId, bool isDeleted);

        void DeleteEmailConfigEditCheckChild(int Id);

        EmailConfigurationEditCheck UpdateEditCheckEmailFormula(int id);

        EmailConfigurationEditCheckResult ValidatWithScreeningTemplate(ScreeningTemplate screeningTemplate);

        void SendEmailonEmailvariableConfiguration(ScreeningTemplate screeningTemplate);
    }
}