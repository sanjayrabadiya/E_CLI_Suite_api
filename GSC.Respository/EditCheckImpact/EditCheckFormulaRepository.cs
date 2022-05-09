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
        public EditCheckResult ValidateFormula(List<EditCheckValidate> editCheck)
        {
            var result = ValidateFormulaReference(editCheck.Where(r => !r.IsTarget).ToList());

            var refCount = editCheck.Where(x => !x.IsTarget && x.InputValue != "" && x.InputValue != null && x.InputValue != "0").Count();

            if (result.IsValid && editCheck.Any(r => r.IsTarget && r.IsFormula == true))
            {
                result.Target = new List<EditCheckResult>();

                editCheck.Where(r => r.IsTarget).ToList().ForEach(r =>
                {
                    var targetResult = new EditCheckResult();
                    targetResult.Id = r.Id;
                    targetResult.Result = result.Result;
                    targetResult.ResultMessage = result.ResultMessage;
                    targetResult.IsValid = result.IsValid;
                    targetResult.SampleText = result.SampleText;

                    if (r.Operator == Operator.SquareRoot)
                        targetResult.Result = Math.Sqrt(Convert.ToDouble(targetResult.Result)).ToString();
                    else if (r.Operator == Operator.Avg)
                        targetResult.Result = (Convert.ToDouble(targetResult.Result) / refCount).ToString();

                    int round = 1;

                    if (r.DataType != null)
                        round = (int)r.DataType;

                    if (round == 1) round = 0;
                    if (round > 3)
                        round = round - 3;

                    if (r.DataType != DataType.Character)
                    {
                        decimal value;
                        decimal.TryParse(targetResult.Result, out value);
                        targetResult.Result = Decimal.Round(value, round, MidpointRounding.AwayFromZero).ToString();
                    }


                    if (r.Operator == Operator.SquareRoot)
                        targetResult.ResultMessage = $"{targetResult.Result} {"->"}{"Sqrt("}{targetResult.ResultMessage}{")"}";
                    else if (r.Operator == Operator.Avg)
                        targetResult.ResultMessage = $"{targetResult.Result} {"->"}{"Avg("}{targetResult.ResultMessage}{")"}";

                    result.Target.Add(targetResult);
                });
            }
            return result;
        }


        public EditCheckResult ValidateFormulaReference(List<EditCheckValidate> editCheck)
        {
            string ruleStr = "";
            var result = new EditCheckResult();
            DateTime? lastDate = null;
            try
            {

                TimeSpan t1 = TimeSpan.Parse("00:00");
                string timeDiff = "";
                Operator? lastOperator = Operator.Minus;
                editCheck.Where(x => !x.IsTarget).ToList().ForEach(r =>
                {
                    if (string.IsNullOrEmpty(r.InputValue))
                        r.InputValue = "0";

                    if (r.CollectionSource == CollectionSources.Time && !string.IsNullOrEmpty(r.InputValue) && r.InputValue != "0" && r.InputValue != "1")
                    {
                        DateTime dt = DateTime.ParseExact(r.InputValue, "MM/dd/yyyy HH:mm:ss", null);
                        if (lastDate != null && dt != null)
                        {
                            if (lastOperator == Operator.Plus)
                            {
                                TimeSpan duration = t1.Add(TimeSpan.Parse(dt.ToString(@"HH\:mm")));
                                if (duration.Days > 0)
                                {
                                    timeDiff = $"{duration.Hours + (24 * duration.Days)}:{duration.Minutes.ToString().PadLeft(2, '0')}";
                                }
                                else
                                    timeDiff = $"{duration.Hours}:{duration.Minutes.ToString().PadLeft(2, '0')}";
                            }
                            else
                            {

                                TimeSpan duration = t1 - TimeSpan.Parse(dt.ToString(@"HH\:mm"));
                                timeDiff = duration.ToString(@"hh\:mm");
                            }
                            t1 = TimeSpan.Parse(timeDiff);
                        }
                        lastDate = dt;
                        lastOperator = r.Operator;
                        if (string.IsNullOrEmpty(timeDiff))
                            t1 = TimeSpan.Parse(dt.ToString(@"HH\:mm"));
                    }

                    if (!string.IsNullOrEmpty(r.CollectionValue) && r.CollectionValue == "0")
                        r.CollectionValue = "";

                    if (!string.IsNullOrEmpty(r.CollectionValue2) && r.CollectionValue2 == "0")
                        r.CollectionValue2 = "";

                    if (r.Operator == Operator.SquareRoot)
                        ruleStr = ruleStr + $"{r.CollectionValue2}{r.StartParens}{"sqrt("}{r.InputValue}{")"}{r.EndParens}{r.CollectionValue}";
                    else
                        ruleStr = ruleStr + $"{r.CollectionValue2}{r.StartParens}{r.InputValue}{r.OperatorName}{r.EndParens}{r.CollectionValue}";
                });

                if (string.IsNullOrEmpty(ruleStr))
                {
                    result.SampleText = "Not defined reference!";
                    result.IsValid = false;
                    return result;
                }


                if (ruleStr.Contains("^") || ruleStr.Contains("sqrt"))
                    result.Result = MathEvaluateExpression.EvaluateExpression(ruleStr).ToString();
                else if (!string.IsNullOrEmpty(timeDiff))
                    result.Result = timeDiff;
                else
                    result.Result = Convert.ToDouble(new DataTable().Compute(ruleStr, null)).ToString();

                result.IsValid = true;
                result.ResultMessage = result.Result + " -> " + ruleStr;
                result.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.IsValid = false;
                if (lastDate != null)
                    result.Result = "0";
            }
            result.SampleText = ruleStr;
            return result;
        }

    }
}
