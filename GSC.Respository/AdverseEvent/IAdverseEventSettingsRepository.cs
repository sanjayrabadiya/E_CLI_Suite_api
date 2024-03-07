using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.AdverseEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public interface IAdverseEventSettingsRepository : IGenericRepository<AdverseEventSettings>
    {
        IList<DropDownDto> GetVisitDropDownforAEReportingInvestigatorForm(int projectId);
        IList<DropDownDto> GetTemplateDropDownforPatientAEReporting(int projectId);
        IList<DropDownDto> GetTemplateDropDownforInvestigatorAEReporting(int visitId);
        IList<AdverseEventSettingsVariableValue> GetAdverseEventSettingsVariableValue(int projectDesignTemplateId);
        AdverseEventSettingsListDto GetData(int projectId);

        void RemoveExistingAdverseDetail(int id);

        bool IsvalidPatientTemplate(int projectDesignTemplateId);

    }
}
