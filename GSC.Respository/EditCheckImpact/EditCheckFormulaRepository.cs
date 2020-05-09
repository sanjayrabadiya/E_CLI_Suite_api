using GSC.Data.Dto.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GSC.Respository.EditCheckImpact
{
    public class EditCheckFormulaRepository : IEditCheckFormulaRepository
    {

        public EditCheckFormulaRepository()
        {

        }

        public EditCheckResult ValidateFormula(List<EditCheckValidate> editCheck)
        {
            var result = ValidateFormulaReference(editCheck.Where(r => !r.IsTarget).ToList());

            if (result.IsValid && editCheck.Any(r => r.IsTarget && r.IsFormula == true))
            {

                var dataType = editCheck.FirstOrDefault(r => r.IsTarget && r.DataType != null);
                if (dataType != null)
                {

                    if (dataType.Operator == Operator.SquareRoot)
                    {
                        
                        result.Result = Math.Sqrt(Convert.ToDouble(result.Result)).ToString();
                        result.ResultMessage = result.Result + " -> " + result.ReferenceString;
                    }

                    int round = (int)dataType.DataType;

                    if (round == 1) round = 0;
                    if (round > 3)
                        round = round - 3;

                    result.Result = Decimal.Round(Convert.ToDecimal(result.Result), round, MidpointRounding.AwayFromZero).ToString();
                }
            }
            return result;
        }


        public EditCheckResult ValidateFormulaReference(List<EditCheckValidate> editCheck)
        {
            string ruleStr = "";
            var result = new EditCheckResult();
            try
            {
                editCheck.Where(x => !x.IsTarget).ForEach(r =>
                {
                    if (r.Operator == Operator.SquareRoot)
                        ruleStr = ruleStr + $"{r.CollectionValue2}{r.StartParens}{"sqrt("}{r.Input1}{")"}{r.EndParens}{r.CollectionValue}";
                    else
                        ruleStr = ruleStr + $"{r.CollectionValue2}{r.StartParens}{r.Input1}{r.OperatorName}{r.EndParens}{r.CollectionValue}";
                });

                if (string.IsNullOrEmpty(ruleStr))
                {
                    result.ResultMessage = "Not defined reference!";
                    result.IsValid = false;
                    return result;
                }
                double editCheckResult;
                if (ruleStr.Contains("^") || ruleStr.Contains("sqrt"))
                {
                    editCheckResult = MathEvaluateExpression.EvaluateExpression(ruleStr);
                }
                else
                    editCheckResult = Convert.ToDouble(new DataTable().Compute(ruleStr, null));


                result.Result = editCheckResult.ToString();
                result.IsValid = true;
                result.ResultMessage = result.Result + " -> " + ruleStr;
                result.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.IsValid = false;
                result.ResultMessage = ruleStr;
            }
            result.ReferenceString = ruleStr;
            return result;
        }

    }
}
