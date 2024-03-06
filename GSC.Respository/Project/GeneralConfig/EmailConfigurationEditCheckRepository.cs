using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
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
using System.Globalization;
using System.Linq;


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
                    var audit = _context.AuditReason.Where(a => a.Id == x.AuditReasonId).FirstOrDefault();
                    if (audit != null)
                        x.ReasonName = audit.ReasonName;
                }
                if (x.EditCheckRoleAuditReasonId > 0)
                {
                    var audit = _context.AuditReason.Where(a => a.Id == x.EditCheckRoleAuditReasonId).FirstOrDefault();
                    if (audit != null)
                        x.EditCheckRoleAuditReasonName = audit.ReasonName;
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
            var data = _emailConfigurationEditCheckDetailRepository.All.Include(s => s.EmailConfigurationEditCheck).Include(s => s.ProjectDesignVariable).Include(s => s.ProjectDesignTemplate).AsNoTracking().
                Where(x => x.DeletedDate == null &&
                x.EmailConfigurationEditCheckId == editCheckId).Select(r => new EmailConfigurationEditCheckDetailDto
                {
                    ProjectId = r.EmailConfigurationEditCheck.ProjectId,
                    startParens = r.startParens,
                    InputValue = r.CollectionValue,
                    Operator = r.Operator,
                    FieldName = r.ProjectDesignVariable.Annotation ?? r.ProjectDesignVariable.VariableName ?? r.VariableAnnotation,
                    LogicalOperator = r.LogicalOperator,
                    OperatorName = r.Operator.GetDescription(),
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    Id = r.Id,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                    ProjectDesignVisitId = r.ProjectDesignTemplateId > 0 ? r.ProjectDesignTemplate.ProjectDesignVisitId : 0,
                    dataType = r.ProjectDesignVariable.DataType,
                    CollectionSource = r.ProjectDesignVariable != null ? r.ProjectDesignVariable.CollectionSource : CollectionSources.TextBox,
                    CheckBy = r.CheckBy,
                    VariableAnnotation = r.VariableAnnotation

                }).ToList();

            data.ForEach(x =>
            {

                x.ProjectDesignId = _context.ProjectDesign.Where(s => s.ProjectId == x.ProjectId).Select(s => s.Id).FirstOrDefault();

                if (x.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                {
                    var variableAnnotation = _context.ProjectDesignVariable.Include(t => t.Values).Where(a => a.Annotation == x.VariableAnnotation
                               && a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == x.ProjectDesignId).FirstOrDefault();
                    x.CollectionSource = variableAnnotation?.CollectionSource;
                    x.dataType = variableAnnotation?.DataType;
                }


            });


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
            int id = 0;
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

                if (r.EmailConfigurationEditCheckId > 0 && id > 0 && r.EmailConfigurationEditCheckId != id)
                {
                    ruleStr = ruleStr + $" OR ";

                }

                if (r.CollectionSource == CollectionSources.NumericScale)
                {
                    if (!string.IsNullOrEmpty(r.CollectionValue))
                    {
                        ruleStr = ruleStr + $"{r.startParens}{colName} {r.OperatorName} {Convert.ToInt32(r.CollectionValue)}";
                    }
                    else
                    {
                        ruleStr = ruleStr + $"{r.startParens}{colName} {r.OperatorName} {r.CollectionValue}";
                    }
                }
                else
                {
                    ruleStr = ruleStr + $"{r.startParens}{colName} {r.OperatorName} {singleQuote}{r.CollectionValue}{singleQuote}";

                }
                displayRule = displayRule + $"{r.startParens}{fieldName} {r.OperatorName} {collectionValue}";


                ruleStr = ruleStr + $"{r.endParens} {r.LogicalOperator} ";
                displayRule = displayRule + $"{r.endParens} {r.LogicalOperator} ";


                id = r.EmailConfigurationEditCheckId;
                var col = new DataColumn();
                if (r.CollectionSource == CollectionSources.NumericScale)
                {
                    if (!string.IsNullOrEmpty(r.InputValue))
                    {
                        col.DefaultValue = Convert.ToInt32(r.InputValue);
                    }
                }
                else
                {
                    col.DefaultValue = r.InputValue ?? "";
                }


                if (r.CollectionSource == CollectionSources.Date || r.CollectionSource == CollectionSources.DateTime || r.CollectionSource == CollectionSources.Time)
                {
                    if (!string.IsNullOrEmpty(r.InputValue) && !(r.Operator == Operator.NotNull || r.Operator == Operator.Null))
                    {
                        DateTime createdDate;
                        var isSucess = DateTime.TryParse(r.InputValue, CultureInfo.InvariantCulture, out createdDate);
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

                if (foundDt == null || !foundDt.Any())
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

            var result = _context.EmailConfigurationEditCheckDetail.Include(s => s.EmailConfigurationEditCheck).Where(x => x.EmailConfigurationEditCheckId == id && x.DeletedDate == null)
                .Select(r => new EmailConfigurationEditCheckDetailDto
                {
                    Id = r.Id,
                    ProjectId = r.EmailConfigurationEditCheck.ProjectId,
                    PeriodName = GetPeriodName(r),
                    TemplateName = r.ProjectDesignTemplateId > 0 ? r.ProjectDesignTemplate.TemplateName : "",
                    VariableName = string.IsNullOrEmpty(r.ProjectDesignVariable.Annotation) ? r.ProjectDesignVariable.VariableName : r.ProjectDesignVariable.Annotation,
                    VisitName = GetVisitName(r),
                    Operator = r.Operator,
                    LogicalOperator = r.LogicalOperator,
                    startParens = r.startParens,
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    CheckBy = r.CheckBy,
                    VariableAnnotation = r.VariableAnnotation

                }).AsEnumerable().OrderBy(r => r.Id).ToList();

            var last = result.LastOrDefault();
            result.ForEach(x =>
            {
                x.ProjectDesignId = _context.ProjectDesign.Where(s => s.ProjectId == x.ProjectId).Select(s => s.Id).FirstOrDefault();

                if (x.CheckBy == EditCheckRuleBy.ByVariableAnnotation)
                {
                    var variableAnnotation = _context.ProjectDesignVariable.Include(t => t.Values).Where(a => a.Annotation == x.VariableAnnotation
                               && a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == x.ProjectDesignId).FirstOrDefault();
                    x.CollectionSource = variableAnnotation?.CollectionSource;
                    x.dataType = variableAnnotation?.DataType;
                }

                var name = x.CheckBy == EditCheckRuleBy.ByVariableAnnotation
                            ? x.VariableAnnotation : x.PeriodName + "." + x.VisitName + "." +
                             x.TemplateName + "." + x.VariableName;

                var operatorName = x.Operator.GetDescription();

                var collectionValue = x.CollectionValue;


                if (x.Operator.CheckMathOperator())
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

        private string GetPeriodName(EmailConfigurationEditCheckDetail r)
        {
            if (r.ProjectDesignVariable != null)
            {
                return r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName;
            }
            if (r.ProjectDesignTemplate != null)
            {
                return r.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName;
            }

            return "";
        }
        private string GetVisitName(EmailConfigurationEditCheckDetail r)
        {
            if (r.ProjectDesignVariable != null && r.ProjectDesignVariable.ProjectDesignTemplate != null && r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit != null)
            {
                return r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName;
            }
            if (r.ProjectDesignTemplate != null)
            {
                return r.ProjectDesignTemplate.ProjectDesignVisit.DisplayName;
            }

            return "";
        }
        private string GetVariableName(EmailConfigurationEditCheckDetail r)
        {
            if (r.ProjectDesignVariable != null && string.IsNullOrEmpty(r.ProjectDesignVariable.Annotation))
            {
                return r.ProjectDesignVariable.VariableName;
            }
            else if (r.ProjectDesignVariable != null)
            {
                return r.ProjectDesignVariable.Annotation;
            }
            else return "";

        }
        public EmailConfigurationEditCheckResult ValidatWithScreeningTemplate(ScreeningTemplate screeningTemplate)
        {
            var projectDesignTemplate = _context.ProjectDesignTemplate.Include(s => s.ProjectDesignVisit).ThenInclude(s => s.ProjectDesignPeriod).ThenInclude(s => s.ProjectDesign).Where(s => s.Id == screeningTemplate.ProjectDesignTemplateId).FirstOrDefault();
            var annotationlist = _context.ProjectDesignVariable.Where(s => s.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId && s.Annotation != null && s.Annotation != null).ToList();
            var data = _emailConfigurationEditCheckDetailRepository.All.Include(s => s.EmailConfigurationEditCheck).Include(s => s.ProjectDesignVariable).Include(s => s.ProjectDesignTemplate).AsNoTracking().
                Where(x => x.DeletedDate == null
                && x.EmailConfigurationEditCheck.ProjectId == projectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId
                && (x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId ||
                (annotationlist != null && annotationlist.Count > 0 && annotationlist.Select(s => s.Annotation).Contains(x.VariableAnnotation)))).Select(r => new EmailConfigurationEditCheckDetailDto
                {

                    startParens = r.startParens,
                    Operator = r.Operator,
                    FieldName = r.ProjectDesignVariableId > 0 ? r.ProjectDesignVariable.VariableName : r.VariableAnnotation,
                    LogicalOperator = r.LogicalOperator,
                    OperatorName = r.Operator.GetDescription(),
                    endParens = r.endParens,
                    CollectionValue = r.CollectionValue,
                    Id = r.Id,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                    ProjectDesignVisitId = r.ProjectDesignTemplateId > 0 ? r.ProjectDesignTemplate.ProjectDesignVisitId : 0,
                    dataType = r.ProjectDesignVariableId > 0 ? r.ProjectDesignVariable.DataType : null,
                    CollectionSource = r.ProjectDesignVariableId > 0 ? r.ProjectDesignVariable.CollectionSource : CollectionSources.TextBox,
                    PeriodName = GetPeriodName(r),
                    TemplateName = r.ProjectDesignTemplateId > 0 ? r.ProjectDesignTemplate.TemplateName : "",
                    VariableName = GetVariableName(r),
                    VisitName = GetVisitName(r),
                    CheckBy = r.CheckBy,
                    VariableAnnotation = r.VariableAnnotation,
                    EmailConfigurationEditCheckId = r.EmailConfigurationEditCheckId

                }).ToList();

            data.ForEach(x =>
            {
                if (screeningTemplate.ProjectDesignTemplateId == x.ProjectDesignTemplateId && x.CheckBy == EditCheckRuleBy.ByVariable)
                {
                    var data = screeningTemplate.ScreeningTemplateValues.FirstOrDefault(s => s.ProjectDesignVariableId == x.ProjectDesignVariableId && s.DeletedDate == null);
                    if (data != null && !string.IsNullOrEmpty(data.Value))
                    {
                        if (data.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton || data.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox || data.ProjectDesignVariable.CollectionSource == CollectionSources.NumericScale)
                        {
                            var child = _context.ProjectDesignVariableValue.Where(s => s.Id == Convert.ToInt32(data.Value)).FirstOrDefault();
                            if (child != null)
                            {
                                x.InputValue = child.ValueName.ToLower();
                                x.CollectionSource = data.ProjectDesignVariable.CollectionSource;
                            }
                        }
                        else
                        {
                            x.InputValue = data.Value.ToLower();
                            x.CollectionSource = data.ProjectDesignVariable.CollectionSource;
                        }
                    }
                }
                if (x.CheckBy == EditCheckRuleBy.ByVariableAnnotation && annotationlist.Count > 0 && annotationlist.Exists(s => s.Annotation == x.VariableAnnotation))
                {
                    var annotation = annotationlist.Find(s => s.Annotation == x.VariableAnnotation);
                    if (annotation != null)
                    {
                        var data = screeningTemplate.ScreeningTemplateValues.FirstOrDefault(s => s.ProjectDesignVariableId == annotation.Id && s.DeletedDate == null);
                        if (data != null && !string.IsNullOrEmpty(data.Value))
                        {
                            if (data.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton || data.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox || data.ProjectDesignVariable.CollectionSource == CollectionSources.NumericScale)
                            {
                                var child = _context.ProjectDesignVariableValue.Where(s => s.Id == Convert.ToInt32(data.Value)).FirstOrDefault();
                                if (child != null)
                                {
                                    x.InputValue = child.ValueName.ToLower();

                                }
                            }
                            else
                            {
                                x.InputValue = data.Value.ToLower();

                            }
                        }
                        x.CollectionSource = annotation.CollectionSource;
                    }
                }
            });


            return EmailEditCheckValidateRule(data);
        }
        public EmailConfigurationEditCheckSendEmailResult SendEmailonEmailvariableConfiguration(ScreeningTemplate screeningTemplate)
        {
            EmailConfigurationEditCheckSendEmailResult finaldata = new EmailConfigurationEditCheckSendEmailResult();
            List<EmailList> emails = new List<EmailList>();
            EmailConfigurationEditCheckSendEmail emaildata = new EmailConfigurationEditCheckSendEmail();
            var annotationlist = _context.ProjectDesignVariable.Where(s => s.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId && s.Annotation != null).ToList();
            var projectDesignTemplate = _context.ProjectDesignTemplate.Include(s => s.ProjectDesignVisit).ThenInclude(s => s.ProjectDesignPeriod).ThenInclude(s => s.ProjectDesign).Where(s => s.Id == screeningTemplate.ProjectDesignTemplateId).FirstOrDefault();
            var data = _emailConfigurationEditCheckDetailRepository.All.
                Include(s => s.ProjectDesignVariable).
                Include(s => s.ProjectDesignTemplate).
                ThenInclude(s => s.ProjectDesignVisit)
                .AsNoTracking().
                Where(x => x.DeletedDate == null && (x.ProjectDesignTemplateId == screeningTemplate.ProjectDesignTemplateId ||
                (annotationlist != null && annotationlist.Count > 0 && annotationlist.Select(s => s.Annotation).Contains(x.VariableAnnotation)))).ToList();
            if (data != null && data.Count > 0)
            {
                var emailconfig = All.Where(s => s.Id == data.FirstOrDefault().EmailConfigurationEditCheckId).FirstOrDefault();
                var emmailrole = _context.EmailConfigurationEditCheckRole.Include(s => s.Role).Where(s => s.DeletedDate == null && s.EmailConfigurationEditCheckId == data.FirstOrDefault().EmailConfigurationEditCheckId).ToList();
                if (emmailrole.Count > 0)
                {
                    var patient = emmailrole.Find(s => s.Role.RoleName == "Patient");
                    if (patient != null && screeningTemplate.ScreeningVisit != null && screeningTemplate.ScreeningVisit.ScreeningEntry != null && screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization != null && screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User != null)
                    {
                        EmailList obj = new EmailList();
                        obj.Email = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User.Email;
                        obj.UserId = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.UserId;
                        obj.Phone = screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.User.Phone;

                        var patientRole = _context.UserRole.Where(s => s.UserId == screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.UserId).FirstOrDefault();
                        if (patientRole != null)
                            obj.RoleId = patientRole.UserRoleId;

                        emails.Add(obj);

                    }

                    var roles = emmailrole.Where(s => s.Role.RoleName != "Patient").Select(s => s.RoleId).ToList();

                    if (roles.Count > 0)
                    {
                        var roleusers = _context.ProjectRight.Include(s => s.User).Where(s => s.DeletedDate == null && roles.Contains(s.RoleId) && s.ProjectId == screeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.ProjectId).ToList();
                        if (roleusers.Count > 0)
                        {
                            foreach (var item in roleusers)
                            {

                                EmailList obj = new EmailList();
                                obj.Email = item.User.Email;
                                obj.UserId = item.User.Id;
                                obj.Phone = item.User.Phone;
                                obj.RoleId = item.RoleId;
                                emails.Add(obj);

                            }
                        }
                    }
                    if (emailconfig != null)
                    {
                        emaildata.Subject = emailconfig.Subject;
                        emaildata.EmailBody = emailconfig.EmailBody;
                        emaildata.IsSMS = emailconfig.IsSMS;
                        emaildata.EmailConfigurationEditCheckId = emailconfig.Id;
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
                        if (projectDesignTemplate != null)
                        {
                            emaildata.VisitName = projectDesignTemplate.ProjectDesignVisit.DisplayName;
                            emaildata.TemplateName = projectDesignTemplate.TemplateName;
                        }
                        emaildata.CurrentDate = DateTime.Now.Date.ToString("dddd, dd MMMM yyyy");
                        if (_jwtTokenAccesser.CompanyId > 0)
                            emaildata.CompanyName = _context.Company.Where(s => s.Id == _jwtTokenAccesser.CompanyId).FirstOrDefault().CompanyName;
                        if (data.Count == 1)
                        {
                            emaildata.VariableName = data.FirstOrDefault().ProjectDesignVariable != null ? data.FirstOrDefault().ProjectDesignVariable.VariableName : "";
                        }
                        if (emails.Count > 0)
                        {
                            foreach (var item in emails)
                            {
                                _emailSenderRespository.SendEmailonEmailvariableConfiguration(emaildata, (int)item.UserId, item.Email, item.Phone);

                                EmailConfigurationEditCheckSendMailHistory obj = new EmailConfigurationEditCheckSendMailHistory();
                                obj.RoleId = item.RoleId;
                                obj.UserId = (int)item.UserId;
                                obj.EmailConfigurationEditCheckId = emailconfig.Id;
                                _context.EmailConfigurationEditCheckSendMailHistory.Add(obj);

                            }
                            _context.Save();
                        }

                    }
                }
            }

            finaldata.emails = emails;
            finaldata.emaildata = emaildata;
            finaldata.EmailMessage = _emailSenderRespository.ConfigureEmail("PROPHASESTUDY", "");
            return finaldata;

        }

        public async void SendEmailonEmailvariableConfigurationSMS(EmailConfigurationEditCheckSendEmailResult result)
        {
            if (result.emails.Count > 0)
            {
                foreach (var item in result.emails)
                {
                    await _emailSenderRespository.SendEmailonEmailvariableConfigurationSMS(result.emaildata, result.EmailMessage, (int)item.UserId, item.Email, item.Phone);
                }
            }
        }

        public List<EmailConfigurationEditCheckMailHistoryGridDto> GetEmailConfigurationEditCheckSendMailHistory(int Id)
        {
            var data = _context.EmailConfigurationEditCheckSendMailHistory.Where(x => x.EmailConfigurationEditCheckId == Id).
                    ProjectTo<EmailConfigurationEditCheckMailHistoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return data;
        }
    }
}
