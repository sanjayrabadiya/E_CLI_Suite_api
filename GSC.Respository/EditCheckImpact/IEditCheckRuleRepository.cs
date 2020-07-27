using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Screening;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckRuleRepository
    {
        EditCheckResult ValidateEditCheckReference(List<EditCheckValidate> editCheck);
        EditCheckResult ValidateEditCheck(List<EditCheckValidate> editCheck);
    }
}
