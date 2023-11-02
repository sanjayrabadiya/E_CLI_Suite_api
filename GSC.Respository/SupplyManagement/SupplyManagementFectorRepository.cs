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
using Microsoft.EntityFrameworkCore;
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
                    StartParens = z.StartParens,
                    EndParens = z.EndParens
                }).ToList()

            }).FirstOrDefault();

            return data;
        }

        public List<SupplyManagementFectorGridDto> GetListByProjectId(int projectId, bool isDeleted)
        {
            var data = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectId).
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
                    StartParens = z.StartParens,
                    EndParens = z.EndParens
                }).ToList().OrderBy(r => r.Id).ToList();

            var last = result.LastOrDefault();
            result.ForEach(x =>
            {
                var name = x.FactoreName;
                var operatorName = x.Operator.GetDescription();
                var collectionValue = x.collectionValueName;

                name = $"{x.StartParens}{"{"}{name.Trim()} {operatorName}{x.EndParens ?? ""} {collectionValue}";

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
            if (fector == Fector.Joint)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Knee";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "Low back";
                    }
                }
            }
            if (fector == Fector.Eligibility)
            {
                if (Collectionavalue != null)
                {
                    if (Collectionavalue == "1")
                    {
                        return "Yes";
                    }
                    if (Collectionavalue == "2")
                    {
                        return "No";
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
            if (fector == Fector.BMI || fector == Fector.Age || fector == Fector.Dose || fector == Fector.Weight)
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
                    InputValue = z.CollectionValue,
                    StartParens = z.StartParens,
                    EndParens = z.EndParens
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


                ruleStr = ruleStr + $"{r.StartParens}{colName} {r.Operator.GetDescription()} {singleQuote}{collectionValue}{singleQuote}";
                displayRule = displayRule + $"{r.StartParens}{fieldName} {r.Operator.GetDescription()} {collectionValue}";


                ruleStr = ruleStr + $"{r.EndParens} {r.LogicalOperator} ";

                displayRule = displayRule + $"{r.EndParens} {r.LogicalOperator} ";

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
            int projectid = 0;
            int siteId = 0;
            int manageSiteId = 0;
            var project = _context.Project.Where(x => x.Id == randomization.ProjectId).FirstOrDefault();
            if (project != null)
            {
                projectid = (int)project.ParentProjectId;
                siteId = project.Id;
                if (project.ManageSiteId > 0)
                    manageSiteId = (int)project.ManageSiteId;
            }
            var supplyManagementFector = _context.SupplyManagementFector.Where(x => x.ProjectId == projectid && x.DeletedDate == null).FirstOrDefault();
            var result = new FactorCheckResult();
            if (supplyManagementFector == null)
                return result;
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
                else if (x.Fector == Fector.Joint && randomization.Jointfactor != null)
                {
                    x.InputValue = randomization.Jointfactor.GetDescription();
                    x.dataType = DataType.Character;
                }
                else if (x.Fector == Fector.Eligibility && randomization.Eligibilityfactor != null)
                {
                    x.InputValue = randomization.Eligibilityfactor.GetDescription();
                    x.dataType = DataType.Character;
                }
                else if (x.Fector == Fector.BMI && !string.IsNullOrEmpty(randomization.BMIfactor))
                {
                    x.InputValue = randomization.BMIfactor;
                    x.dataType = DataType.Numeric2Decimal;
                }
                else if (x.Fector == Fector.Age && !string.IsNullOrEmpty(randomization.Agefactor))
                {
                    x.InputValue = randomization.Agefactor;
                    x.dataType = DataType.Numeric;
                }
                else if (x.Fector == Fector.Dose && !string.IsNullOrEmpty(randomization.Dosefactor))
                {
                    x.InputValue = randomization.Dosefactor;
                    x.dataType = DataType.Numeric;
                }
                else if (x.Fector == Fector.Weight && !string.IsNullOrEmpty(randomization.Weightfactor))
                {
                    x.InputValue = randomization.Weightfactor;
                    x.dataType = DataType.Numeric;
                }

            });



            result = ValidateFactorSubject(data, (int)projectid, siteId, manageSiteId);
            return result;
        }
        public FactorCheckResult ValidateFactorSubject(List<SupplyManagementFectorDetailDto> editCheck, int projectid, int siteId, int manageSiteId)
        {
            var dt = new DataTable();
            string ruleStr = "";
            string ruleStrRatio = "";
            string displayRule = "";
            string productType = "";
            string ratios = "";

            int i = 0;
            editCheck.ForEach(r =>
            {
                string singleQuote = SingleQuote(r.Operator, r.dataType);
                i += 1;
                string colName = "Col" + i.ToString();
                string fieldName = r.FactoreName;
                string collectionValue = r.collectionValueName;
                string colrandomizationName = "";

                ruleStr = ruleStr + $"{r.StartParens}{colName} {r.Operator.GetDescription()} {singleQuote}{collectionValue}{singleQuote}";
                displayRule = displayRule + $"{r.StartParens}{fieldName} {r.Operator.GetDescription()} {collectionValue}";


                ruleStr = ruleStr + $"{r.EndParens} {r.LogicalOperator} ";

                displayRule = displayRule + $"{r.EndParens} {r.LogicalOperator} ";

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

                if (!string.IsNullOrEmpty(r.ProductTypeCode) && productType != r.ProductTypeCode)
                    productType = productType + r.ProductTypeCode;
                if (r.LogicalOperator == "OR")
                    productType = productType + " OR ";


                if (r.Fector == Fector.Age)
                    colrandomizationName = "Agefactor";
                if (r.Fector == Fector.Gender)
                    colrandomizationName = "Genderfactor";
                if (r.Fector == Fector.Diatory)
                    colrandomizationName = "Diatoryfactor";
                if (r.Fector == Fector.BMI)
                    colrandomizationName = "BMIfactor";
                if (r.Fector == Fector.Joint)
                    colrandomizationName = "Jointfactor";
                if (r.Fector == Fector.Eligibility)
                    colrandomizationName = "Eligibilityfactor";
                if (r.Fector == Fector.Weight)
                    colrandomizationName = "Weightfactor";
                if (r.Fector == Fector.Dose)
                    colrandomizationName = "Dosefactor";

                ruleStrRatio = ruleStrRatio + $"{r.StartParens}{colrandomizationName} {r.Operator.GetDescription()} {singleQuote}{r.CollectionValue}{singleQuote}";
                ruleStrRatio = ruleStrRatio + $"{r.EndParens} {r.LogicalOperator} ";

                if (r.Ratio > 0)
                    ratios = ratios + r.Ratio;
                if (r.LogicalOperator == "OR")
                    ratios = ratios + " OR ";

            });

            ruleStr = ruleStr.Replace("  ", " ").Trim();
            if (!string.IsNullOrEmpty(ruleStrRatio))
                ruleStrRatio = ruleStrRatio.Replace("  ", " ").Trim();

            var result = ValidateDataTableFactor(dt, ruleStr);
            if (!string.IsNullOrEmpty(result.ErrorMessage) || !string.IsNullOrEmpty(result.Result))
                return result;

            result.SampleText = displayRule;

            if (ruleStr.Contains("OR"))
            {
                var splitRules = ruleStr.Split("OR").ToList();
                if (splitRules.Count > 0)
                {
                    for (int r = 0; r < splitRules.Count; r++)
                    {
                        var result1 = ValidateDataTableFactor(dt, splitRules[r]);
                        if (result1.IsValid)
                        {
                            if (!string.IsNullOrEmpty(productType))
                            {
                                if (productType.Contains("OR"))
                                {
                                    var ProducttypeArray = productType.Split("OR").ToArray();
                                    result.ProductType = ProducttypeArray[r].Trim();
                                    if (!string.IsNullOrEmpty(ratios))
                                        result = CheckRatio(result, ruleStrRatio, ratios, ProducttypeArray[r].Trim(), r, projectid, siteId, manageSiteId);

                                }
                                else
                                {
                                    result.ProductType = productType.Trim();
                                    if (!string.IsNullOrEmpty(ratios))
                                        result = CheckRatio(result, ruleStrRatio, ratios, result.ProductType, r, projectid, siteId, manageSiteId);
                                }
                                return result;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ratios))
                                    result = CheckRatio(result, ruleStrRatio, ratios, null, r, projectid, siteId, manageSiteId);
                            }


                        }
                    }
                }
            }
            else
            {
                var result1 = ValidateDataTableFactor(dt, ruleStr);
                if (result1.IsValid)
                {
                    if (!string.IsNullOrEmpty(productType))
                    {
                        result.ProductType = productType.Trim();
                        if (!string.IsNullOrEmpty(ratios))
                            result = CheckRatio(result, ruleStrRatio, ratios, result.ProductType, null, projectid, siteId, manageSiteId);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(ratios))
                            result = CheckRatio(result, ruleStrRatio, ratios, null, null, projectid, siteId, manageSiteId);
                    }
                }
            }

            return result;
        }
        public FactorCheckResult CheckRatio(FactorCheckResult result, string ratiostr, string ratios, string producttype, int? index, int projectid, int siteId, int manageSiteId)
        {
            var isRationOver = false;
            var products = "";
            var uploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == projectid && x.DeletedDate == null && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (uploadFile == null)
                return result;

            if (!string.IsNullOrEmpty(producttype))
            {
                if (producttype.Contains(','))
                {
                    var splitproduct = producttype.Split(',').ToList();
                    if (splitproduct.Count > 0)
                    {
                        if (index != null)
                        {
                            if (ratiostr.Contains("OR") && ratios.Contains("OR"))
                            {
                                var splitRules = ratiostr.Split("OR").ToList();
                                var splitRatio = ratios.Split("OR").ToList();
                                if (splitRules.Count > 0 && splitRatio.Count > 0)
                                {
                                    var rule = splitRules[(int)index].Trim();
                                    var ratio = splitRatio[(int)index].Trim();
                                    int count = 0;
                                    int rationcount = 0;

                                    foreach (var item in splitproduct)
                                    {
                                        string sqlqry = String.Empty;

                                        var treatment = "'" + item + "'";

                                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                                        {
                                            sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + treatment.ToString() + " AND " + rule + "";
                                        }
                                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                                        {
                                            sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND ProductCode = " + treatment.ToString() + " AND " + rule + "";
                                        }
                                        List<int> managesite = new List<int>();
                                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                        {
                                            var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();

                                            managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();


                                            sqlqry = @"select * from Randomization r
                                                        WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + treatment.ToString() + " AND " + rule + "";

                                        }

                                        var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                        {
                                            if (finaldata.Count > 0)
                                            {
                                                if (managesite.Count > 0)
                                                {
                                                    var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                                    finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                                                }
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                                        {
                                            rationcount++;
                                        }
                                        else
                                        {

                                            if (count == splitproduct.Count - 1)
                                                products = products + item;
                                            else
                                                products = products + item + ",";

                                        }
                                        count++;

                                    }
                                    if (rationcount == splitproduct.Count)
                                        isRationOver = true;


                                }
                            }
                        }
                        else
                        {
                            var rule = ratiostr;
                            var ratio = ratios;
                            int count = 0;
                            int rationcount = 0;


                            foreach (var item in splitproduct)
                            {
                                string sqlqry = String.Empty;
                                var treatment = "'" + item + "'";
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                                {
                                    sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + treatment.ToString() + " AND " + rule + "";
                                }
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                                {
                                    sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND ProductCode = " + treatment.ToString() + " AND " + rule + "";
                                }
                                List<int> managesite = new List<int>();
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                {
                                    var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();

                                    managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();


                                    sqlqry = @"select * from Randomization r
                                                        WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + treatment.ToString() + " AND " + rule + "";

                                }

                                var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                {
                                    if (finaldata.Count > 0)
                                    {
                                        if (managesite.Count > 0)
                                        {
                                            var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                            finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                                {
                                    rationcount++;
                                }
                                else
                                {
                                    if (count == splitproduct.Count - 1)
                                        products = products + item;
                                    else
                                        products = products + item + ",";

                                }
                                count++;
                            }
                            if (rationcount == splitproduct.Count)
                                isRationOver = true;


                        }

                    }
                }
                else
                {
                    if (index != null)
                    {
                        if (ratiostr.Contains("OR") && ratios.Contains("OR"))
                        {
                            var splitRules = ratiostr.Split("OR").ToList();
                            var splitRatio = ratios.Split("OR").ToList();
                            if (splitRules.Count > 0 && splitRatio.Count > 0)
                            {
                                var rule = splitRules[(int)index].Trim();
                                var ratio = splitRatio[(int)index].Trim();
                                var product = "'" + producttype + "'";

                                string sqlqry = String.Empty;
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                                {
                                    sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + product + " AND " + rule + "";
                                }
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                                {
                                    sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND ProductCode = " + product + " AND " + rule + "";
                                }
                                List<int> managesite = new List<int>();
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                {
                                    var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();
                                    managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();

                                    sqlqry = @"select * from Randomization r
                                                        WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + product + " AND " + rule + "";

                                }
                                var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                                if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                                {
                                    if (finaldata.Count > 0)
                                    {
                                        if (managesite.Count > 0)
                                        {
                                            var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                            finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                                        }
                                    }
                                }
                                if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                                {
                                    isRationOver = true;
                                }
                                else
                                {
                                    isRationOver = false;
                                    products = producttype;
                                }

                            }
                        }
                    }
                    else
                    {
                        var rule = ratiostr;
                        var ratio = ratios;
                        var product = "'" + producttype + "'";
                        string sqlqry = String.Empty;
                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                        {
                            sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + product + " AND " + rule + "";
                        }
                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                        {
                            sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND ProductCode = " + product + " AND " + rule + "";
                        }
                        List<int> managesite = new List<int>();
                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                        {
                            var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();

                            managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();

                            sqlqry = @"SELECT * FROM Randomization r
                                      WHERE  projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND ProductCode = " + product + " AND " + rule + "";

                        }
                        
                        var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                        if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                        {
                            if (finaldata.Count > 0)
                            {
                                if (managesite.Count > 0)
                                {
                                    var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                    finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                        {
                            isRationOver = true;
                        }
                        else
                        {
                            isRationOver = false;
                            products = producttype;
                        }


                    }
                }

            }
            else
            {
                if (index != null)
                {
                    if (ratiostr.Contains("OR") && ratios.Contains("OR"))
                    {
                        var splitRules = ratiostr.Split("OR").ToList();
                        var splitRatio = ratios.Split("OR").ToList();
                        if (splitRules.Count > 0 && splitRatio.Count > 0)
                        {
                            var rule = splitRules[(int)index].Trim();
                            var ratio = splitRatio[(int)index].Trim();

                            
                            string sqlqry = String.Empty;
                            if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                            {
                                sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND " + rule + "";
                            }
                            if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                            {
                                sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND " + rule + "";
                            }
                            List<int> managesite = new List<int>();
                            if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                            {
                                var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();

                                managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();

                                sqlqry = @"select * from Randomization 
                                           WHERE  projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND " + rule + "";

                            }

                            var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                            if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                            {
                                if (finaldata.Count > 0)
                                {
                                    if (managesite.Count > 0)
                                    {
                                        var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                        finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                            {
                                isRationOver = true;
                            }
                            else
                            {
                                isRationOver = false;
                            }

                        }
                    }
                }
                else
                {
                    var rule = ratiostr;
                    var ratio = ratios;


                    string sqlqry = String.Empty;
                    if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        sqlqry = @"select * from Randomization WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND " + rule + "";
                    }
                    if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                    {
                        sqlqry = @"select * from Randomization WHERE projectId =" + siteId + " AND " + rule + "";
                    }
                    List<int> managesite = new List<int>();
                    if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == manageSiteId).FirstOrDefault();

                        managesite = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.City.State.CountryId == site.City.State.CountryId && x.DeletedDate == null).Select(x => x.Id).ToList();
                        sqlqry = @"select * from Randomization r
                                                        
                                                        WHERE projectId IN(select Id from project where ParentProjectId=" + projectid + ") AND " + rule + "";

                    }
                    var finaldata = _context.FromSql<Randomization>(sqlqry).ToList();
                    if (uploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        if (finaldata.Count > 0)
                        {
                            if (managesite.Count > 0)
                            {
                                var projectids = _context.Project.Where(x => x.ParentProjectId == projectid && managesite.Contains((int)x.ManageSiteId) && x.DeletedDate == null).Select(x => x.Id).ToList();
                                finaldata = finaldata.Where(x => projectids.Contains(x.ProjectId)).ToList();
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(ratio) && finaldata.Count >= Convert.ToInt32(ratio))
                    {
                        isRationOver = true;
                    }
                    else
                    {
                        isRationOver = false;
                    }


                }

            }
            if (isRationOver)
            {
                result.ErrorMessage = "Factor Randomization limit is completed. You can not randomize!";
            }
            result.ProductType = products;

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
            if (collection1 == Fector.Gender || collection1 == Fector.Diatory || collection1 == Fector.Joint || collection1 == Fector.Eligibility)
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

        public bool CheckfactorrandomizationStarted(int projectId)
        {
            var randomization = _context.Randomization.Where(x => x.Project.ParentProjectId == projectId
            && x.RandomizationNumber != null).FirstOrDefault();

            if (randomization != null)
            {
                return false;
            }
            return true;
        }
        public bool CheckUploadRandomizationsheet(int projectId)
        {
            var randomization = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == projectId
            && x.Status == LabManagementUploadStatus.Approve && x.DeletedDate == null).FirstOrDefault();
            if (randomization != null)
            {
                return true;
            }
            return false;
        }
    }
}
