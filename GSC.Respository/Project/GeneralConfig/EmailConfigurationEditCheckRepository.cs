using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class EmailConfigurationEditCheckRepository : GenericRespository<EmailConfigurationEditCheck>, IEmailConfigurationEditCheckRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IEmailConfigurationEditCheckDetailRepository _emailConfigurationEditCheckDetailRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public EmailConfigurationEditCheckRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IEmailConfigurationEditCheckDetailRepository emailConfigurationEditCheckDetailRepository,
            IEmailSenderRespository emailSenderRespository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _emailConfigurationEditCheckDetailRepository = emailConfigurationEditCheckDetailRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        public List<EmailConfigurationEditCheckGridDto> GetEmailEditCheckList(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<EmailConfigurationEditCheckGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.AuditReasonId > 0)
                {
                    x.ReasonName = _context.AuditReason.Where(a => a.Id == x.AuditReasonId).FirstOrDefault().ReasonName;
                }
                if (x.EditCheckRoleAuditReasonId > 0)
                {
                    x.EditCheckRoleAuditReasonName = _context.AuditReason.Where(a => a.Id == x.EditCheckRoleAuditReasonId).FirstOrDefault().ReasonName;
                }
                var roles = _context.EmailConfigurationEditCheckRole.Where(s => s.EmailConfigurationEditCheckId == x.Id).Select(a => a.Role.RoleName).ToList();
                if (roles.Count > 0)
                    x.Roles = string.Join(",", roles.Distinct());
            });

            return data;
        }

        public void DeleteEmailConfigEditCheckChild(int Id)
        {
            var list = _context.EmailConfigurationEditCheckDetail.Where(x => x.EmailConfigurationEditCheckId == Id).ToList();
            _context.EmailConfigurationEditCheckDetail.RemoveRange(list);

            var role = _context.EmailConfigurationEditCheckRole.Where(x => x.EmailConfigurationEditCheckId == Id).ToList();
            _context.EmailConfigurationEditCheckRole.RemoveRange(role);

            _context.Save();
        }

        public EmailConfigurationEditCheck UpdateEditCheckEmailFormula(int id)
        {
            var factor = Find(id);
            factor.SourceFormula = GetEditCheckEmailFormula(id);
            factor.CheckFormula = factor.SourceFormula;
            var verifyResult = CheckEditCheckEmailParens(factor.Id);
            if (verifyResult != null)
            {
                factor.SampleResult = verifyResult.SampleText;
                factor.ErrorMessage = verifyResult.ErrorMessage;
            }
            Update(factor);
            _context.Save();
            return factor;
        }
        EmailConfigurationEditCheckResult CheckEditCheckEmailParens(int editCheckId)
        {
            var data = _emailConfigurationEditCheckDetailRepository.All.Include(s => s.ProjectDesignVariable).Include(s => s.ProjectDesignTemplate).AsNoTracking().
                Where(x => x.DeletedDate == null &&
                x.EmailConfigurationEditCheckId == editCheckId).Select(r => new EmailConfigurationEditCheckDetailDto
                {

                    startParens = r.startParens,
                    InputValue = r.CollectionValue,
                    Operator = r.Operator,
                    FieldName = r.ProjectDesignVariable.VariableName,
                    LogicalOperator = r.LogicalOperator,
                    OperatorName = r.Operator.GetDescription(),
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    Id = r.Id,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                    ProjectDesignVisitId = r.ProjectDesignTemplate.ProjectDesignVisitId,
                    dataType = r.ProjectDesignVariable.DataType,
                    CollectionSource = r.ProjectDesignVariable.CollectionSource

                }).ToList();
            return EmailEditCheckValidateRule(data);
        }
        string SingleQuote(Operator? _operator, DataType? dataType)
        {
            if (_operator == null && dataType == null)
                return "";
            if (dataType != DataType.Character && (_operator == Operator.Greater || _operator == Operator.GreaterEqual ||
                _operator == Operator.Lessthen || _operator == Operator.LessthenEqual))
                return "";

            if (dataType != null && dataType != DataType.Character)
                return "";

            return "'";
        }
        public EmailConfigurationEditCheckResult EmailEditCheckValidateRule(List<EmailConfigurationEditCheckDetailDto> editCheck)
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
                string fieldName = r.FieldName;
                string collectionValue = r.CollectionValue;

                r.OperatorName = r.OperatorName.Replace(Operator.NotEqual.GetDescription(), "<>").
                   Replace(Operator.NotNull.GetDescription(), "<>").
                   Replace(Operator.Null.GetDescription(), "=");

                if (r.CollectionSource == CollectionSources.NumericScale)
                {
                    ruleStr = ruleStr + $"{r.startParens}{colName} {r.OperatorName} {r.CollectionValue}";
                }
                else
                {
                    ruleStr = ruleStr + $"{r.startParens}{colName} {r.OperatorName} {singleQuote}{r.CollectionValue}{singleQuote}";

                }
                displayRule = displayRule + $"{r.startParens}{fieldName} {r.OperatorName} {collectionValue}";


                ruleStr = ruleStr + $"{r.endParens} {r.LogicalOperator} ";
                displayRule = displayRule + $"{r.endParens} {r.LogicalOperator} ";

                var col = new DataColumn();
                col.DefaultValue = r.InputValue ?? "";

                if (r.CollectionSource == CollectionSources.Date || r.CollectionSource == CollectionSources.DateTime || r.CollectionSource == CollectionSources.Time)
                {
                    if (!string.IsNullOrEmpty(r.InputValue) && !(r.Operator == Operator.NotNull || r.Operator == Operator.Null))
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


                    var isnumeri = IsNumeric(r.CollectionSource, r.dataType);
                    if (isnumeri && value == 0)
                        col.DefaultValue = 0;

                    if ((value != 0 || isnumeri) && string.IsNullOrEmpty(singleQuote))
                        col.DataType = Type.GetType("System.Decimal");
                }


                col.ColumnName = colName;
                dt.Columns.Add(col);
            });


            ruleStr = ruleStr.Replace("  ", " ").Trim();

            var result = ValidateDataTableEmailEditCheck(dt, ruleStr);

            result.SampleText = displayRule;
            return result;
        }

        EmailConfigurationEditCheckResult ValidateDataTableEmailEditCheck(DataTable dt, string ruleStr)
        {
            var result = new EmailConfigurationEditCheckResult();
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            try
            {
                var foundDt = dt.Select(ruleStr);
                result.IsValid = true;

                if (foundDt == null || foundDt.Count() == 0)
                {
                    result.Result = "Email Variable Configuration : Input value not verified!";
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = "Email Variable Configuration : " + ex.Message;
            }

            return result;
        }
        bool IsNumeric(CollectionSources? collection, DataType? dataType)
        {
            if (collection != null && collection == CollectionSources.TextBox && dataType != null && dataType != DataType.Character)
                return true;

            if (collection != null && collection == CollectionSources.HorizontalScale)
                return true;

            return false;
        }
        private string GetEditCheckEmailFormula(int id)
        {
            var variableValues = _context.EmailConfigurationEditCheckDetail.
                Where(x => x.EmailConfigurationEditCheckId == id
                && x.DeletedDate == null).Select(r => r.CollectionValue).ToList();

            var result = _context.EmailConfigurationEditCheckDetail.Where(x => x.EmailConfigurationEditCheckId == id && x.DeletedDate == null)
                .Select(r => new EmailConfigurationEditCheckDetailDto
                {
                    Id = r.Id,
                    PeriodName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName : "",
                    TemplateName = r.ProjectDesignTemplate.TemplateName,
                    VariableName = string.IsNullOrEmpty(r.ProjectDesignVariable.Annotation) ? r.ProjectDesignVariable.VariableName : r.ProjectDesignVariable.Annotation,
                    VisitName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName : "",
                    Operator = r.Operator,
                    LogicalOperator = r.LogicalOperator,
                    startParens = r.startParens,
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    ProjectDesignId = r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId,

                }).ToList().OrderBy(r => r.Id).ToList();

            var last = result.LastOrDefault();
            result.ForEach(x =>
            {
                var name = x.PeriodName + "." + x.VisitName + "." +
                             x.TemplateName + "." + x.VariableName;

                var operatorName = x.Operator.GetDescription();

                var collectionValue = x.CollectionValue;


                if (((Operator)x.Operator).CheckMathOperator())
                {
                    if (x.Equals(last))
                        name = $"{x.startParens}{"{"}{name.Trim()}{"}"}{x.endParens ?? ""} {collectionValue}";
                    else
                        name = $"{x.startParens}{"{"}{name.Trim()}{"}"} {operatorName}{x.endParens ?? ""} {collectionValue}";
                }

                else
                {
                    name = $"{x.startParens}{"{"}{name.Trim()} {operatorName}{x.endParens ?? ""} {collectionValue}";

                    if (x.Equals(last))
                        name = $"{name}{"}"}";
                    else
                        name = $"{name}{"}"} {x.LogicalOperator}";
                }

                x.QueryFormula = name;

            });

            return string.Join(" ", result.Select(r => r.QueryFormula));
        }

        public EmailConfigurationEditCheckResult ValidatWithScreeningTemplate(ScreeningTemplate screeningTemplate)
        {
            var data = _emailConfigurationEditCheckDetailRepository.All.Include(s => s.ProjectDesignVariable).Include(s => s.ProjectDesignTemplate).AsNoTracking().
                Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId).Select(r => new EmailConfigurationEditCheckDetailDto
                {

                    startParens = r.startParens,
                    Operator = r.Operator,
                    FieldName = r.ProjectDesignVariable.VariableName,
                    LogicalOperator = r.LogicalOperator,
                    OperatorName = r.Operator.GetDescription(),
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    Id = r.Id,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                    ProjectDesignVisitId = r.ProjectDesignTemplate.ProjectDesignVisitId,
                    dataType = r.ProjectDesignVariable.DataType,
                    CollectionSource = r.ProjectDesignVariable.CollectionSource,
                    PeriodName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod
                             .DisplayName : "",
                    TemplateName = r.ProjectDesignTemplate.TemplateName,
                    VariableName = string.IsNullOrEmpty(r.ProjectDesignVariable.Annotation) ? r.ProjectDesignVariable.VariableName : r.ProjectDesignVariable.Annotation,
                    VisitName = r.ProjectDesignVariable != null
                         ? r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName
                         : r.ProjectDesignTemplate != null ? r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName : ""

                }).ToList();

            data.ForEach(x =>
            {
                if (screeningTemplate.ProjectDesignTemplateId == x.ProjectDesignTemplateId)
                {
                    var data = screeningTemplate.ScreeningTemplateValues.Where(s => s.ProjectDesignVariableId == x.ProjectDesignVariableId && s.DeletedDate == null).FirstOrDefault();
                    if (data != null)
                    {
                        if (data.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton || data.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox || data.ProjectDesignVariable.CollectionSource == CollectionSources.NumericScale)
                        {
                            var child = _context.ProjectDesignVariableValue.Where(s => s.Id == Convert.ToInt32(data.Value)).FirstOrDefault();
                            if (child != null)
                                x.InputValue = child.ValueName.ToLower();
                        }
                        else
                        {
                            x.InputValue = data.Value.ToLower();
                        }
                    }
                }
            });


            return EmailEditCheckValidateRule(data);
        }
        public void SendEmailonEmailvariableConfiguration(ScreeningTemplate screeningTemplate)
        {
            List<EmailList> emails = new List<EmailList>();
            List<string> mobile = new List<string>();
            EmailConfigurationEditCheckSendEmail emaildata = new EmailConfigurationEditCheckSendEmail();
            var data = _emailConfigurationEditCheckDetailRepository.All.
                Include(s => s.ProjectDesignVariable).
                Include(s => s.ProjectDesignTemplate).
                ThenInclude(s => s.ProjectDesignVisit)
                .AsNoTracking().
                Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId).ToList();
            if (data != null && data.Count > 0)
            {
                var emailconfig = All.Where(s => s.Id == data.FirstOrDefault().EmailConfigurationEditCheckId).FirstOrDefault();
                var emmailrole = _context.EmailConfigurationEditCheckRole.Include(s => s.Role).Where(s => s.DeletedDate == null && s.EmailConfigurationEditCheckId == data.FirstOrDefault().EmailConfigurationEditCheckId).ToList();
                if (emmailrole.Count > 0)
                {
                    var patient = emmailrole.Where(s => s.Role.RoleName == "Patient").FirstOrDefault();
                    if (patient != null)
                    {
                        if (screeningTemplate.ScreeningVisit != null && screeningTemplate.ScreeningVisit.ScreeningEntry != null && screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization != null)
                        {
                            if (screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User != null)
                            {
                                EmailList obj = new EmailList();
                                obj.Email = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User.Email;
                                obj.UserId = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.UserId;
                                obj.Phone = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User.Phone;
                                emails.Add(obj);

                            }
                        }
                    }

                    var roles = emmailrole.Where(s => s.Role.RoleName != "Patient").Select(s => s.RoleId).ToList();

                    if (roles.Count > 0)
                    {
                        var roleusers = _context.ProjectRight.Where(s => s.DeletedDate == null && roles.Contains(s.RoleId) && s.ProjectId == screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ProjectId).Select(s => s.User).ToList();
                        if (roleusers.Count > 0)
                        {
                            foreach (var item in roleusers)
                            {

                                EmailList obj = new EmailList();
                                obj.Email = item.Email;
                                obj.UserId = item.Id;
                                obj.Phone = item.Phone;
                                emails.Add(obj);

                            }
                        }
                    }
                    if (emailconfig != null)
                    {
                        emaildata.Subject = emailconfig.Subject;
                        emaildata.EmailBody = emailconfig.EmailBody;
                        var screeningdata = _context.ScreeningEntry.Include(x => x.Randomization).ThenInclude(x => x.Project).ThenInclude(x => x.ManageSite).Include(x => x.Project).Where(x => x.Id == screeningTemplate.ScreeningVisit.ScreeningEntryId).FirstOrDefault();
                        if (screeningdata != null)
                        {
                            emaildata.ScreeningNo = screeningdata.Randomization.ScreeningNumber;
                            emaildata.RandomizationNo = screeningdata.Randomization.RandomizationNumber;
                            if (screeningdata.Project != null)
                            {
                                emaildata.SiteCode = screeningdata.Project.ProjectCode;
                                var projectdata = _context.Project.Where(x => x.Id == screeningdata.Project.ParentProjectId).FirstOrDefault();
                                if (projectdata != null)
                                {
                                    emaildata.StudyCode = projectdata.ProjectCode;
                                    if (screeningdata.Project != null)
                                    {
                                        var managesite = _context.ManageSite.Where(x => x.DeletedDate == null && x.Id == screeningdata.Project.ManageSiteId).FirstOrDefault();
                                        if (managesite != null)
                                            emaildata.SiteName = screeningdata.Project.ProjectCode + " - " + managesite.SiteName;
                                    }
                                }
                            }
                        }
                        emaildata.VisitName = data.FirstOrDefault().ProjectDesignTemplate.ProjectDesignVisit.DisplayName;
                        emaildata.TemplateName = data.FirstOrDefault().ProjectDesignTemplate.TemplateName;
                        if (data.Count == 1)
                        {
                            emaildata.VariableName = data.FirstOrDefault().ProjectDesignVariable.VariableName;
                        }
                        if (emails.Count > 0)
                        {
                            foreach (var item in emails)
                            {
                                _emailSenderRespository.SendEmailonEmailvariableConfiguration(emaildata, (int)item.UserId, item.Email, item.Phone);
                            }
                        }

                    }
                }
            }

        }
    }
}
