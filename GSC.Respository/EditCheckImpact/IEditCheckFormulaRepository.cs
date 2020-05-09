using GSC.Data.Dto.Project.EditCheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public interface IEditCheckFormulaRepository
    {
        EditCheckResult ValidateFormula(List<EditCheckValidate> editCheck);
        EditCheckResult ValidateFormulaReference(List<EditCheckValidate> editCheck);
    }
}
