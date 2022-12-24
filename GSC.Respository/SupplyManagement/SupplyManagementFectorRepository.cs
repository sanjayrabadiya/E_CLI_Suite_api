using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementFectorRepository : GenericRespository<SupplyManagementFector>, ISupplyManagementFectorRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementFectorRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public SupplyManagementFectorDto GetById(int id)
        {
            var data = _context.SupplyManagementFector.Where(x => x.Id == id && x.DeletedDate == null).Select(x => new SupplyManagementFectorDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectCode = x.Project.ProjectCode,
                Formula = x.Formula,
                Children = x.FectorDetailList.Where(x => x.DeletedDate == null).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = z.CollectionValue,
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                }).ToList()

            }).FirstOrDefault();

            return data;
        }

        public List<SupplyManagementFectorGridDto> GetListByProjectId(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<SupplyManagementFectorGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();


            return data;
        }


        public void DeleteChild(int Id)
        {
            var list = _context.SupplyManagementFectorDetail.Where(x => x.Id == Id).ToList();
            _context.SupplyManagementFectorDetail.RemoveRange(list);
            _context.Save();
        }

        public SupplyManagementFector UpdateFactorFormula(int id)
        {
            var factor = Find(id);
            factor.SourceFormula = GetfactorFormula(id);
            factor.CheckFormula = factor.SourceFormula;
            var verifyResult = CheckFactorParens(factor.Id);
            if (verifyResult != null)
            {
                factor.SampleResult = verifyResult.SampleText;
                factor.ErrorMessage = verifyResult.ErrorMessage;
            }
            Update(factor);
            _context.Save();
            return factor;
        }
        private string GetfactorFormula(int id)
        {
            var variableValues = _context.SupplyManagementFectorDetail.
                Where(x => x.SupplyManagementFectorId == id
                && x.DeletedDate == null).Select(r => r.CollectionValue).ToList();

            var result = _context.SupplyManagementFectorDetail.Where(x => x.SupplyManagementFectorId == id
                                                            && x.DeletedDate == null)
                .Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    SupplyManagementFectorId = z.SupplyManagementFectorId,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                }).ToList().OrderBy(r => r.Id).ToList();

            var last = result.LastOrDefault();
            result.ForEach(x =>
            {
                var name = x.FactoreName;
                var operatorName = x.Operator.GetDescription();
                var collectionValue = x.collectionValueName;

                name = $"{"{"}{name.Trim()} {operatorName}{""} {collectionValue}";

                if (x.Equals(last))
                    name = $"{name}{"}"}";
                else
                    name = $"{name}{"}"} {x.LogicalOperator}";

                x.QueryFormula = name;
            });

            return string.Join(" ", result.Select(r => r.QueryFormula));
        }

        public static string GetCollectionValue(Fector fector, string Collectionavalue)
        {
            if (fector == Fector.Gender)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Male";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "Female";
                    }
                }
            }
            if (fector == Fector.Diatory)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Veg";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "Non-veg";
                    }
                }
            }
            if (fector == Fector.BMI || fector == Fector.Age)
            {
                return Collectionavalue;
            }
            return "";

        }

        FactorCheckResult CheckFactorParens(int factorId)
        {

            var data = _context.SupplyManagementFectorDetail.
                Where(x => x.DeletedDate == null &&
                x.SupplyManagementFectorId == factorId).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    SupplyManagementFectorId = z.SupplyManagementFectorId,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                    IsDeleted = z.DeletedDate != null ? true : false,
                    ProjectCode = z.SupplyManagementFector.Project.ProjectCode,
                    InputValue = z.CollectionValue
                }).ToList();



            return ValidateFactor(data);


        }
        public FactorCheckResult ValidateFactor(List<SupplyManagementFectorDetailDto> editCheck)
        {
            var dt = new DataTable();
            string ruleStr = "";
            string displayRule = "";

            int i = 0;
            editCheck.ForEach(r =>
            {
                string singleQuote = SingleQuote(r.Operator, r.dataType);
                i += 1;
                string colName = "Col" + i.ToString();
                string fieldName = r.FactoreName;
                string collectionValue = r.collectionValueName;


                ruleStr = ruleStr + $"{null}{colName} {r.Operator.GetDescription()} {singleQuote}{collectionValue}{singleQuote}";
                displayRule = displayRule + $"{null}{fieldName} {r.Operator.GetDescription()} {collectionValue}";


                ruleStr = ruleStr + $"{null} {r.LogicalOperator} ";
                displayRule = displayRule + $"{null} {r.LogicalOperator} ";

                var col = new DataColumn();
                col.DefaultValue = r.InputValue ?? "";

                decimal value;
                decimal.TryParse(r.InputValue, out value);


                var isnumeri = IsNumeric(r.Fector, r.dataType);
                if (isnumeri && value == 0)
                    col.DefaultValue = 0;

                if ((value != 0 || isnumeri) && string.IsNullOrEmpty(singleQuote))
                    col.DataType = Type.GetType("System.Decimal");


                col.ColumnName = colName;
                dt.Columns.Add(col);
            });

            ruleStr = ruleStr.Replace("  ", " ").Trim();

            var result = ValidateDataTableFactor(dt, ruleStr);

            result.SampleText = displayRule;
            return result;
        }

        FactorCheckResult ValidateDataTableFactor(DataTable dt, string ruleStr)
        {
            var result = new FactorCheckResult();
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            try
            {
                var foundDt = dt.Select(ruleStr);
                result.IsValid = true;

                if (foundDt == null || foundDt.Count() == 0)
                {
                    result.Result = "Factor Configuration : Input value not verified!";
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Factor Configuration : " + ex.Message;
            }

            return result;
        }

        public FactorCheckResult ValidateSubjecWithFactor(Randomization randomization)
        {
            var projectid = _context.Project.Where(x => x.Id == randomization.ProjectId).FirstOrDefault().ParentProjectId;
            var supplyManagementFector = _context.SupplyManagementFector.Where(x => x.ProjectId == projectid).FirstOrDefault();
            var result = new FactorCheckResult();
            var data = _context.SupplyManagementFectorDetail.
                Where(x => x.DeletedDate == null &&
                x.SupplyManagementFectorId == supplyManagementFector.Id).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    SupplyManagementFectorId = z.SupplyManagementFectorId,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = GetCollectionValue(z.Fector, z.CollectionValue),
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                    IsDeleted = z.DeletedDate != null ? true : false,
                    ProjectCode = z.SupplyManagementFector.Project.ProjectCode,
                    InputValue = z.CollectionValue
                }).ToList();
            data.ForEach(x =>
            {
                if (x.Fector == Fector.Gender && randomization.Genderfactor != null)
                {
                    x.InputValue = randomization.Genderfactor.GetDescription();
                    x.dataType = DataType.Character;
                }
                else if (x.Fector == Fector.Diatory && randomization.Diatoryfactor != null)
                {
                    x.InputValue = randomization.Diatoryfactor.GetDescription();
                    x.dataType = DataType.Character;
                }
                else if (x.Fector == Fector.BMI && !string.IsNullOrEmpty(randomization.BMIfactor))
                {
                    x.InputValue = randomization.BMIfactor;
                    x.dataType = DataType.Numeric2Decimal;
                }
                else
                {
                    x.InputValue = randomization.Agefactor;
                    x.dataType = DataType.Numeric;
                }

            });
            result = ValidateFactor(data);
            return result;
        }
        string SingleQuote(FectorOperator? _operator, DataType? dataType)
        {
            if (_operator == null && dataType == null)
                return "";
            if (dataType != DataType.Character && (_operator == FectorOperator.Greater || _operator == FectorOperator.GreaterEqual ||
                _operator == FectorOperator.Lessthen || _operator == FectorOperator.LessthenEqual))
                return "";

            if (dataType != null && dataType != DataType.Character)
                return "";

            return "'";
        }
        bool IsNumeric(Fector? collection1, DataType? dataType)
        {
            var collection = new CollectionSources();
            if (collection1 == Fector.Gender || collection1 == Fector.Diatory)
            {
                collection = CollectionSources.ComboBox;
            }
            else
            {
                collection = CollectionSources.TextBox;
            }
            if (collection == CollectionSources.TextBox && dataType != null && dataType != DataType.Character)
                return true;

            return false;
        }
    }
}
