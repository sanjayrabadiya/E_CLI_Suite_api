using GSC.Data.Dto.Project.EditCheck;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace GSC.Respository.EditCheckImpact
{
    public class EditCheckRuleRepository : IEditCheckRuleRepository
    {
        private readonly IEditCheckFormulaRepository _editCheckFormulaRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public EditCheckRuleRepository(
            IEditCheckFormulaRepository editCheckFormulaRepository,
            IJwtTokenAccesser jwtTokenAccesser
            )
        {
            _editCheckFormulaRepository = editCheckFormulaRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public EditCheckResult ValidateEditCheck(List<EditCheckValidate> editCheck)
        {
            var dateDiff = editCheck.Any(x => x.Operator == Operator.Different);
            if (!dateDiff && editCheck.Any(x => x.IsFormula) && editCheck.Any(x => x.CheckBy == EditCheckRuleBy.ByVariableRule))
            {
                var result = ValidateRule(editCheck.Where(x => x.CheckBy == EditCheckRuleBy.ByVariableRule).ToList(), true);
                result.ResultSkip = true;
                result.SampleText = "Rule not verifed";
                if (result.IsValid)
                    return _editCheckFormulaRepository.ValidateFormula(editCheck.Where(x => x.CheckBy != EditCheckRuleBy.ByVariableRule).ToList());
                else
                    return result;
            }
            else if (!dateDiff && editCheck.Any(x => x.IsFormula))
                return _editCheckFormulaRepository.ValidateFormula(editCheck);
            else
                return ValidateRule(editCheck, true);
        }


        public EditCheckResult ValidateEditCheckReference(List<EditCheckValidate> editCheck)
        {
            var dateDiff = editCheck.Any(x => x.Operator == Operator.Different);
            if (!dateDiff && editCheck.Any(x => x.IsFormula) && editCheck.Any(x => x.CheckBy == EditCheckRuleBy.ByVariableRule))
            {
                var result = ValidateRule(editCheck.Where(x => x.CheckBy == EditCheckRuleBy.ByVariableRule).ToList(), false);
                result.ResultSkip = true;
                if (result.IsValid)
                    return _editCheckFormulaRepository.ValidateFormula(editCheck.Where(x => x.CheckBy != EditCheckRuleBy.ByVariableRule).ToList());
                else
                    return result;
            }
            else if (!dateDiff && editCheck.Any(x => x.IsFormula))
                return _editCheckFormulaRepository.ValidateFormulaReference(editCheck.Where(x => x.CheckBy != EditCheckRuleBy.ByVariableRule).ToList());
            else
                return ValidateRule(editCheck, false);
        }

        EditCheckResult ValidateRule(List<EditCheckValidate> editCheck, bool isFromValidate)
        {
            if (editCheck.Any(r => r.Operator == Operator.Different))
                return GetDifferentValue(editCheck, isFromValidate);
            else if (editCheck.Count(r => !r.IsTarget) == 0)
            {
                var editCheckResult = new EditCheckResult();
                editCheckResult.Target = new List<EditCheckResult>();
                editCheckResult.IsValid = true;
                editCheck.ForEach(c =>
                {
                    var targetResult = new EditCheckResult();
                    if (c.Operator == Operator.HardFetch || c.Operator == Operator.SoftFetch)
                    {
                        targetResult.IsValid = true;
                        targetResult.Result = editCheck.Where(x => !x.IsTarget).FirstOrDefault()?.InputValue;
                    }
                    else if (IsSkipOperator(c.Operator))
                        targetResult.IsValid = true;
                    else
                    {
                        targetResult = ValidateRuleReference(new List<EditCheckValidate> { c }, isFromValidate);
                        targetResult.ResultMessage = targetResult.SampleText;
                    }
                    targetResult.Id = c.Id;
                    targetResult.Result = targetResult.IsValid ? "Passed" : "Failed";
                    editCheckResult.Target.Add(targetResult);
                });
                return editCheckResult;
            }
            else if (editCheck.Any(r => r.IsReferenceValue) || editCheck.Any(r => r.Operator == Operator.Percentage))
            {
                var reference = editCheck.FirstOrDefault(r => !r.IsTarget);
                editCheck = editCheck.Where(r => r.IsTarget).ToList();
                var editCheckResult = new EditCheckResult();
                editCheckResult.IsValid = true;
                editCheckResult.Target = new List<EditCheckResult>();
                editCheck.ForEach(c =>
                {
                    if (string.IsNullOrEmpty(c.OperatorName))
                    {
                        c.OperatorName = reference?.OperatorName;
                        c.Operator = reference?.Operator;
                    }

                    if (c.Operator == Operator.Percentage)
                        PercentageOperator(c, reference);

                    if (isFromValidate)
                        c.CollectionValue = reference?.InputValue;
                    else
                    {
                        c.CollectionValue = "1";
                        c.InputValue = "1";
                        c.RefernceFieldName = reference?.FieldName;
                    }

                    var result = ValidateRuleReference(new List<EditCheckValidate> { c }, isFromValidate);
                    result.Id = c.Id;
                    result.Result = result.IsValid ? "Passed" : "Failed";
                    if (isFromValidate && string.IsNullOrEmpty(c.CollectionValue))
                        editCheckResult.IsValid = result.IsValid;
                    editCheckResult.SampleText = result.SampleText;
                    editCheckResult.ErrorMessage = result.ErrorMessage;
                    editCheckResult.Target.Add(result);
                });

                return editCheckResult;
            }
            else
            {
                return TargetAndReference(editCheck, isFromValidate);
            }
        }

        private EditCheckResult TargetAndReference(List<EditCheckValidate> editCheck, bool isFromValidate)
        {
            var result = ValidateRuleReference(editCheck.Where(x => !x.IsTarget).ToList(), isFromValidate);
            result.Target = new List<EditCheckResult>();

            editCheck.Where(x => x.IsTarget).ToList().ForEach(r =>
            {
                if (!isFromValidate)
                    r.InputValue = "1";


                if (IsSkipOperator(r.Operator))
                {
                    var editCheckResult = new EditCheckResult();
                    editCheckResult.Id = r.Id;
                    if (result.IsValid)
                    {
                        editCheckResult.IsValid = true;
                        editCheckResult.Result = "Passed";
                    }
                    else
                    {
                        editCheckResult.IsValid = false;
                        editCheckResult.Result = "Failed";
                    }
                    result.Target.Add(editCheckResult);
                }
                else if (r.Operator == Operator.HardFetch || r.Operator == Operator.SoftFetch)
                {
                    var editCheckResult = new EditCheckResult();
                    editCheckResult.Id = r.Id;
                    editCheckResult.IsValid = true;
                    editCheckResult.Result = editCheck.Where(x => !x.IsTarget).FirstOrDefault()?.InputValue;
                    result.Target.Add(editCheckResult);
                }
                else
                {
                    var target = ValidateRuleReference(new List<EditCheckValidate> { r }, isFromValidate);
                    if (result.IsValid && target.IsValid)
                        target.Result = "Passed";
                    else if (!result.IsValid && !target.IsValid)
                        target.Result = "Not Processed";
                    else
                        target.Result = "Failed";
                    target.Id = r.Id;
                    result.Target.Add(target);
                }
            });

            if (!isFromValidate && result.Target != null && result.Target.Any(r => !r.IsValid))
            {
                result.IsValid = false;
                result.ErrorMessage += " syntax error in target";
            }

            return result;
        }

        void PercentageOperator(EditCheckValidate editCheckValidate, EditCheckValidate refEditCheck)
        {
            double collectionValue;
            double.TryParse(editCheckValidate.CollectionValue, out collectionValue);

            double refValue;
            double.TryParse(refEditCheck.InputValue, out refValue);
            var totalValue = refValue + (refValue * collectionValue / 100);
            refEditCheck.InputValue = totalValue.ToString();

            editCheckValidate.Operator = refEditCheck.Operator;
            editCheckValidate.OperatorName = refEditCheck.OperatorName;

        }

        bool IsSkipOperator(Operator? _operator)
        {
            return _operator == Operator.Enable ||
                _operator == Operator.Required ||
                _operator == Operator.Warning ||
                _operator == Operator.Optional ||
                _operator == Operator.Visible;
        }

        private EditCheckResult GetDifferentValue(List<EditCheckValidate> editCheck, bool isFromValidate)
        {
            var result = new EditCheckResult();
            var from = editCheck.FirstOrDefault(r => r.Operator != null && !r.IsTarget);
            var to = editCheck.FirstOrDefault(r => r.Operator == null && !r.IsTarget);
            result.SampleText = from?.FieldName + "-" + to?.FieldName;
            if (from == null || to == null)
            {
                result.IsValid = false;
                return result;
            }

            if (!isFromValidate)
            {
                from.InputValue = _jwtTokenAccesser.GetClientDate().ToString();
                to.InputValue = _jwtTokenAccesser.GetClientDate().ToString();
            }

            result.IsValid = true;
            result.Target = new List<EditCheckResult>();
            var targetResult = new EditCheckResult();

            var targetEditCheck = editCheck.FirstOrDefault(r => r.IsTarget);

            if (editCheck.Any(x => x.IsFormula) && targetEditCheck == null)
                targetEditCheck = new EditCheckValidate();

            if (targetEditCheck == null) return result;

            targetResult.Id = targetEditCheck.Id;
            targetResult.SampleText = $"{from?.FieldName} {"-"} {to?.FieldName}";

            if (!string.IsNullOrEmpty(from.InputValue) && !string.IsNullOrEmpty(to.InputValue))
            {
                targetResult.IsValid = true;
                DateTime startDate = Convert.ToDateTime(from.InputValue);
                DateTime endDate = Convert.ToDateTime(to.InputValue);
                if (endDate < startDate)
                {
                    endDate = Convert.ToDateTime(from.InputValue);
                    startDate = Convert.ToDateTime(to.InputValue);
                }

                targetResult.ResultMessage = $"{startDate.ToString("dd-MMM-yyyy")} {"-"} {endDate.ToString("dd-MMM-yyyy")}";
                var ruleResult = 0;
                if (from.CollectionValue.ToUpper().Contains("M"))
                {
                    var ts = startDate - endDate;
                    ruleResult = Math.Abs(Convert.ToInt32(ts.TotalDays / 30));
                }
                else if (from.CollectionValue.ToUpper().Contains("D"))
                {
                    var ts = startDate - endDate;
                    ruleResult = Math.Abs(Convert.ToInt32(ts.TotalDays));
                }
                else
                {

                    var age = endDate.Year - startDate.Year;

                    if (startDate.Date > endDate.AddYears(-age)) age--;
                    ruleResult = Math.Abs(age);
                }

                if (!string.IsNullOrEmpty(to.CollectionValue))
                {
                    ruleResult = Convert.ToInt32(new DataTable().Compute($"{ruleResult} {to.CollectionValue}", null));
                }

                targetResult.Result = ruleResult.ToString();
            }
            else
            {
                targetResult.ErrorMessage = "From and To Date required";
            }

            result.Target.Add(targetResult);
            return result;

        }

        string SingleQuote(Operator? _operator, DataType? dataType)
        {
            if (_operator == null && dataType == null)
                return "";

            if (dataType != DataType.Character && (_operator == Operator.Between || _operator == Operator.NotBetween))
                return "";

            if (dataType != DataType.Character && (_operator == Operator.Greater || _operator == Operator.GreaterEqual ||
                _operator == Operator.Lessthen || _operator == Operator.LessthenEqual))
                return "";

            if (dataType != null && dataType != DataType.Character)
                return "";

            return "'";
        }

        bool IsNumeric(CollectionSources? collection, DataType? dataType)
        {
            if (collection != null && collection == CollectionSources.TextBox && dataType != null && dataType != DataType.Character)
                return true;

            if (collection != null && collection == CollectionSources.HorizontalScale)
                return true;

            return false;
        }

        EditCheckResult ValidateRuleReference(List<EditCheckValidate> editCheck, bool isFromValidate)
        {
            var dt = new DataTable();
            string ruleStr = "";
            string displayRule = "";

            int i = 0;
            editCheck.ForEach(r =>
            {
                string singleQuote = SingleQuote(r.Operator, r.DataType);
                i += 1;
                string colName = "Col" + i.ToString();
                string fieldName = isFromValidate ? r.InputValue : r.FieldName;
                string collectionValue = !r.IsReferenceValue ? r.CollectionValue : r.RefernceFieldName ?? r.CollectionValue;

                InNotInOperator(r);

                r.OperatorName = r.OperatorName.Replace(Operator.NotEqual.GetDescription(), "<>").
                   Replace(Operator.NotNull.GetDescription(), "<>").
                   Replace(Operator.Null.GetDescription(), "=");


                if (r.Operator == Operator.In || (r.CollectionSource == CollectionSources.MultiCheckBox && r.Operator == Operator.Equal))
                {
                    ruleStr = ruleStr + $"{r.StartParens}{colName} {"LIKE '%"}{r.CollectionValue}{"%'"}";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue}";
                }
                else if (r.Operator == Operator.NotIn || (r.CollectionSource == CollectionSources.MultiCheckBox && r.Operator == Operator.NotEqual))
                {
                    ruleStr = ruleStr + $"{r.StartParens}{colName} {"NOT LIKE '%"}{r.CollectionValue}{"%'"}";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue}";
                }
                else if (r.Operator == Operator.Between)
                {
                    ruleStr = ruleStr + $"{r.StartParens}{colName} {">="}{singleQuote}{r.CollectionValue}{singleQuote}{" AND "}{colName} {"<="}{singleQuote}{r.CollectionValue2}{singleQuote}";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue} {"AND"} {r.CollectionValue2}";
                }
                else if (r.Operator == Operator.NotBetween)
                {
                    ruleStr = ruleStr + $"({r.StartParens}{colName} {"<="}{singleQuote}{r.CollectionValue}{singleQuote}{" OR "}{colName} {">="}{singleQuote}{r.CollectionValue2}{singleQuote})";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue} {"AND"} {r.CollectionValue2}";
                }
                else if (r.Operator == Operator.NotNull || r.Operator == Operator.Null)
                {
                    ruleStr = ruleStr + $"{r.StartParens}{colName} {r.OperatorName} '{r.CollectionValue}'";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue}";
                }
                else
                {
                    ruleStr = ruleStr + $"{r.StartParens}{colName} {r.OperatorName} {singleQuote}{r.CollectionValue}{singleQuote}";
                    displayRule = displayRule + $"{r.StartParens}{fieldName} {r.OperatorName} {collectionValue}";
                }

                ruleStr = ruleStr + $"{r.EndParens} {r.LogicalOperator} ";
                displayRule = displayRule + $"{r.EndParens} {r.LogicalOperator} ";

                var col = new DataColumn();
                col.DefaultValue = r.InputValue ?? "";

                if (r.CollectionSource == CollectionSources.Date || r.CollectionSource == CollectionSources.DateTime || r.CollectionSource == CollectionSources.Time)
                {
                    if (!string.IsNullOrEmpty(r.InputValue))
                    {
                        DateTime createdDate;
                        var isSucess = DateTime.TryParse(r.InputValue, out createdDate);
                        if (isSucess)
                            col.DataType = Type.GetType("System.DateTime");
                    }

                }
                else if (r.Operator != Operator.NotNull && r.Operator != Operator.Null)
                {
                    decimal value;
                    decimal.TryParse(r.InputValue, out value);
                    var isnumeri = IsNumeric(r.CollectionSource, r.DataType);
                    if (isnumeri && value == 0)
                        col.DefaultValue = 0;

                    if ((value != 0 || isnumeri) && string.IsNullOrEmpty(singleQuote))
                        col.DataType = Type.GetType("System.Decimal");
                }


                col.ColumnName = colName;
                dt.Columns.Add(col);
            });


            ruleStr = ruleStr.Replace("  ", " ").Trim();

            var result = ValidateDataTable(dt, ruleStr, isFromValidate, editCheck.Any(r => r.IsTarget));

            result.SampleText = displayRule;
            return result;
        }

        EditCheckResult ValidateDataTable(DataTable dt, string ruleStr, bool isFromValidate, bool isTarget)
        {
            var result = new EditCheckResult();
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            try
            {
                var foundDt = dt.Select(ruleStr);
                result.IsValid = true;

                if (foundDt == null || foundDt.Count() == 0)
                {
                    result.Result = "Input value not verified!";
                    if (isFromValidate && !isTarget)
                        result.IsValid = false;
                }




                if (isFromValidate && !string.IsNullOrEmpty(result.Result))
                    result.IsValid = false;

            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        void InNotInOperator(EditCheckValidate editCheckValidate)
        {
            if (editCheckValidate.Operator == Operator.In || editCheckValidate.Operator == Operator.NotIn)
            {

                string inputValue = editCheckValidate.InputValue;
                editCheckValidate.InputValue = "," + editCheckValidate.CollectionValue + ",";
                editCheckValidate.CollectionValue = "," + inputValue + ",";
                editCheckValidate.InputValue = editCheckValidate.InputValue.Replace(", ", ",").Replace(" ,", ",");
                editCheckValidate.CollectionValue = editCheckValidate.CollectionValue.Replace(", ", ",").Replace(" ,", ",");
            }


        }
    }
}
