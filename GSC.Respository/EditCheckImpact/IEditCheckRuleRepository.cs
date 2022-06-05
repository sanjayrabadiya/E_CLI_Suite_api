using GSC.Data.Dto.Project.EditCheck;
using System.Collections.Generic;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckRuleRepository
    {
        EditCheckResult ValidateEditCheckReference(List<EditCheckValidate> editCheck);
        EditCheckResult ValidateEditCheck(List<EditCheckValidate> editCheck);
        EditCheckResult ValidateRuleReference(List<EditCheckValidate> editCheck, bool isFromValidate);
    }
}
