using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Screening;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckImpactRepository
    {
        void CheckValidation(ScreeningTemplate screeningTemplate, List<ScreeningTemplateValue> values, int projectDesignId, int domainId);
        EditCheckResult ValidateEditCheckReference(List<EditCheckValidate> editCheck);
        EditCheckResult ValidateEditCheck(List<EditCheckValidate> editCheck);
    }
}
