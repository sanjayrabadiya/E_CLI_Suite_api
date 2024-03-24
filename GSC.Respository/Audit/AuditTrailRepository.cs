using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using GSC.Common.Common;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Respository.Audit
{
    public class AuditTrailRepository : GenericRespository<AuditTrail>, IAuditTrailRepository
    {
        private readonly IGSCContext _context;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public AuditTrailRepository(IGSCContext context,
             IUploadSettingRepository uploadSettingRepository,
             IJobMonitoringRepository jobMonitoringRepository,
             IJwtTokenAccesser jwtTokenAccesser,
             IUserRepository userRepository,
             IEmailSenderRespository emailSenderRespository) : base(context)
        {
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
            _jobMonitoringRepository = jobMonitoringRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _emailSenderRespository = emailSenderRespository;
        }

        public IList<AuditTrailDto> Search(AuditTrailDto search)
        {
            //if (search.TableName == nameof(_context.ProjectDesign))
            //     SearchProjectDesign(search);

            var query = All.AsQueryable();

            if (search.TableName?.Length > 0)
            {
                query = query.Where(x => x.TableName == search.TableName);
                if (search.TableName == "Randomization" && search.ProjectId != null)
                {
                    var randomizationIds = _context.Randomization.Where(x => x.ProjectId == search.ProjectId && x.DeletedDate == null)
                        .Select(s => s.Id).ToList();

                    query = query.Where(x => randomizationIds.Contains(x.RecordId));
                }
            }
            if (search.RecordId > 0)
                query = query.Where(x => x.RecordId == search.RecordId);
            if (!string.IsNullOrEmpty(search.ColumnName))
                query = query.Where(x =>
                    x.ColumnName != null && x.ColumnName.ToLower().Contains(search.ColumnName.ToLower()));
            if (!string.IsNullOrEmpty(search.OldValue))
                query = query.Where(x =>
                    x.OldValue != null && x.OldValue.ToLower().Contains(search.OldValue.ToLower()));
            if (!string.IsNullOrEmpty(search.NewValue))
                query = query.Where(x =>
                    x.NewValue != null && x.NewValue.ToLower().Contains(search.NewValue.ToLower()));
            if (search.ParentId > 0 && search.TableName == "TaskMaster")
            {
                var data = _context.TaskMaster.Where(s => s.DeletedDate == null && s.TaskTemplateId == search.ParentId).Select(s => s.Id).ToList();
                if (data.Count > 0)
                    query = query.Where(x => data.Contains(x.RecordId));
            }

            if (!string.IsNullOrEmpty(search.ReasonName))
            {
                query = query.Where(x => x.Reason == search.ReasonName);
            }

            if (search.UserId > 0)
                query = query.Where(x => x.UserId == search.UserId);

            if (!string.IsNullOrEmpty(search.UserRoleName))
                query = query.Where(x => x.UserRole == search.UserRoleName);

            var result = GetItems(query);

            return result;
        }



        public void SearchProjectDesign(ProjectDatabaseSearchDto search)
        {

            var Project = _context.Project.Find(search.ParentProjectId);
            var designId = _context.ProjectDesign.Where(x => x.ProjectId == Project.Id).FirstOrDefault().Id;
            var periodIds = _context.ProjectDesignPeriod.Where(t => t.ProjectDesignId == designId)
                .Select(s => s.Id).ToList();

            var visitIds = _context.ProjectDesignVisit.Where(t => periodIds.Contains(t.ProjectDesignPeriodId) && (search.VisitIds == null || search.VisitIds.Length == 0 || search.VisitIds.Contains(t.Id)))
                .Select(s => s.Id).ToList();

            var templateIds = _context.ProjectDesignTemplate.Where(t => visitIds.Contains(t.ProjectDesignVisitId) && (search.TemplateIds == null || search.TemplateIds.Length == 0 || search.TemplateIds.Contains(t.Id)))
                .Select(s => s.Id).ToList();

            var variableIds = _context.ProjectDesignVariable.Where(t => templateIds.Contains(t.ProjectDesignTemplateId) && (search.VariableIds == null || search.VariableIds.Length == 0 || search.VariableIds.Contains(t.Id)))
                .Select(s => s.Id).ToList();

            var variableValueIds = _context.ProjectDesignVariableValue
                .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();
            var variableRemarksIds = _context.ProjectDesignVariableRemarks
                .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();
            var projectDesignVisitStatusIds = _context.ProjectDesignVisitStatus
              .Where(t => visitIds.Contains(t.ProjectDesignVisitId)).Select(s => s.Id).ToList();

            var templateNoteIds = _context.ProjectDesignTemplateNote
              .Where(t => templateIds.Contains(t.ProjectDesignTemplateId)).Select(s => s.Id).ToList();

            var visitLanguageIds = _context.VisitLanguage
              .Where(t => visitIds.Contains(t.ProjectDesignVisitId)).Select(s => s.Id).ToList();
            var templateLanguageIds = _context.TemplateLanguage
              .Where(t => templateIds.Contains(t.ProjectDesignTemplateId)).Select(s => s.Id).ToList();
            var templateNoteLanguageIds = _context.TemplateNoteLanguage
              .Where(t => templateNoteIds.Contains(t.ProjectDesignTemplateNoteId)).Select(s => s.Id).ToList();
            var variableLanguageIds = _context.VariableLanguage
              .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();
            var variableNoteLanguageIds = _context.VariableNoteLanguage
              .Where(t => variableIds.Contains(t.ProjectDesignVariableId)).Select(s => s.Id).ToList();
            var variableValueLanguageIds = _context.VariableValueLanguage
              .Where(t => variableValueIds.Contains(t.ProjectDesignVariableValueId)).Select(s => s.Id).ToList();

            var displayLists = new List<ProjectDesignAuditReportDto>();
            var Period = GetDesignItems("ProjectDesignPeriod", periodIds, Project.ProjectCode);
            if (Period != null)
            {
                var keys = Period.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignPeriod.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Period = t.DisplayName
                }).ToList();

                foreach (var x in Period)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var Visit = GetDesignItems("ProjectDesignVisit", visitIds, Project.ProjectCode);
            if (Visit != null || Visit.Count() > 0)
            {
                var keys = Visit.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignVisit.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Visit = t.DisplayName,
                    Period = t.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in Visit)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var Template = GetDesignItems("ProjectDesignTemplate", templateIds, Project.ProjectCode);
            if (Template != null || Template.Count() > 0)
            {
                var keys = Template.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignTemplate.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Template = t.TemplateCode + '-' + t.TemplateName,
                    Visit = t.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in Template)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var Variable = GetDesignItems("ProjectDesignVariable", variableIds, Project.ProjectCode);
            if (Variable != null || Variable.Count() > 0)
            {
                var keys = Variable.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignVariable.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Variable = t.VariableCode + '-' + t.VariableName,
                    Template = t.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in Variable)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var Value = GetDesignItems("ProjectDesignVariableValue", variableValueIds, Project.ProjectCode);
            if (Value != null || Value.Count() > 0)
            {
                var keys = Value.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignVariableValue.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Variable = t.ProjectDesignVariable.VariableCode + '-' + t.ProjectDesignVariable.VariableName,
                    Template = t.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in Value)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var VisitLanguageData = GetDesignItems("VisitLanguage", visitLanguageIds, Project.ProjectCode);
            if (VisitLanguageData != null || VisitLanguageData.Count() > 0)
            {
                var keys = VisitLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.VisitLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Visit = t.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in VisitLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var TemplateLanguageData = GetDesignItems("TemplateLanguage", templateLanguageIds, Project.ProjectCode);
            if (TemplateLanguageData != null || TemplateLanguageData.Count() > 0)
            {
                var keys = TemplateLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.TemplateLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Template = t.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in TemplateLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var TemplateNoteData = GetDesignItems("ProjectDesignTemplateNote", templateNoteIds, Project.ProjectCode);
            if (TemplateNoteData != null || TemplateNoteData.Count() > 0)
            {
                var keys = TemplateNoteData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignTemplateNote.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Template = t.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in TemplateNoteData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var TemplateNoteLanguageData = GetDesignItems("TemplateNoteLanguage", templateNoteLanguageIds, Project.ProjectCode);
            if (TemplateNoteLanguageData != null || TemplateNoteLanguageData.Count() > 0)
            {
                var keys = TemplateNoteLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.TemplateNoteLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Template = t.ProjectDesignTemplateNote.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignTemplateNote.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignTemplateNote.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in TemplateNoteLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var VariableLanguageData = GetDesignItems("VariableLanguage", variableLanguageIds, Project.ProjectCode);
            if (VariableLanguageData != null || VariableLanguageData.Count() > 0)
            {
                var keys = VariableLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.VariableLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Variable = t.ProjectDesignVariable.VariableCode + '-' + t.ProjectDesignVariable.VariableName,
                    Template = t.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in VariableLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var VariableNoteLanguageData = GetDesignItems("VariableNoteLanguage", variableNoteLanguageIds, Project.ProjectCode);
            if (VariableNoteLanguageData != null || VariableNoteLanguageData.Count() > 0)
            {
                var keys = VariableNoteLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.VariableNoteLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Variable = t.ProjectDesignVariable.VariableCode + '-' + t.ProjectDesignVariable.VariableName,
                    Template = t.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in VariableNoteLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var VariableValueLanguageData = GetDesignItems("VariableValueLanguage", variableValueLanguageIds, Project.ProjectCode);
            if (VariableValueLanguageData != null || VariableValueLanguageData.Count() > 0)
            {
                var keys = VariableValueLanguageData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.VariableValueLanguage.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Variable = t.ProjectDesignVariableValue.ProjectDesignVariable.VariableCode + '-' + t.ProjectDesignVariableValue.ProjectDesignVariable.VariableName,
                    Template = t.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode + '-' + t.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    Visit = t.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVariableValue.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

                foreach (var x in VariableValueLanguageData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            displayLists = new List<ProjectDesignAuditReportDto>();
            var VisitStatusData = GetDesignItems("ProjectDesignVisitStatus", projectDesignVisitStatusIds, Project.ProjectCode);
            if (VisitStatusData != null || VisitStatusData.Count() > 0)
            {
                var keys = VisitStatusData.Select(t => t.Key).Distinct().ToList();
                displayLists = _context.ProjectDesignVisitStatus.Where(x => keys.Contains(x.Id)).Select(t => new ProjectDesignAuditReportDto
                {
                    Key = t.Id,
                    Visit = t.ProjectDesignVisit.DisplayName,
                    Period = t.ProjectDesignVisit.ProjectDesignPeriod.DisplayName,
                    Template = t.ProjectDesignVariable.ProjectDesignTemplate.TemplateCode + "-" + t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    Variable = t.ProjectDesignVariable.VariableCode + "-" + t.ProjectDesignVariable.VariableName
                }).ToList();

                foreach (var x in VisitStatusData)
                {
                    var nameDetail = displayLists.FirstOrDefault(t => t.Key == x.Key);
                    x.Period = nameDetail.Period;
                    x.Visit = nameDetail.Visit;
                    x.Template = nameDetail.Template;
                    x.Variable = nameDetail.Variable;
                }
            }

            #region Job Monitoring Save - Inprocess Status
            JobMonitoring jobMonitoring = new JobMonitoring();
            jobMonitoring.JobName = JobNameType.ProjectDesignAudit;
            jobMonitoring.JobDescription = search.SelectedProject;
            jobMonitoring.JobType = JobTypeEnum.Excel;
            jobMonitoring.JobStatus = JobStatusType.InProcess;
            jobMonitoring.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoring.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            _jobMonitoringRepository.Add(jobMonitoring);
            _context.Save();
            #endregion

            #region Excel Report Design
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet;

                #region ProjectDesignPeriod sheet
                worksheet = workbook.Worksheets.Add("Period");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "IP Address";
                worksheet.Cell(1, 5).Value = "Action";
                worksheet.Cell(1, 6).Value = "Field Name";
                worksheet.Cell(1, 7).Value = "Old Value";
                worksheet.Cell(1, 8).Value = "New Value";
                worksheet.Cell(1, 9).Value = "Reason";
                worksheet.Cell(1, 10).Value = "Comment";
                worksheet.Cell(1, 11).Value = "User";
                worksheet.Cell(1, 12).Value = "Role";
                worksheet.Cell(1, 13).Value = "DateTime";
                worksheet.Cell(1, 14).Value = "TimeZone";

                var j = 2;

                Period.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(5).SetValue(d.Action);
                    worksheet.Row(j).Cell(6).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(7).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(8).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(9).SetValue(d.Reason);
                    worksheet.Row(j).Cell(10).SetValue(d.Comment);
                    worksheet.Row(j).Cell(11).SetValue(d.User);
                    worksheet.Row(j).Cell(12).SetValue(d.Role);
                    worksheet.Row(j).Cell(13).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(14).SetValue(d.TimeZone);
                    j++;
                });

                #endregion ProjectDesignPeriod sheet

                #region Visit sheet
                worksheet = workbook.Worksheets.Add("Visit");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "IP Address";
                worksheet.Cell(1, 6).Value = "Action";
                worksheet.Cell(1, 7).Value = "Field Name";
                worksheet.Cell(1, 8).Value = "Old Value";
                worksheet.Cell(1, 9).Value = "New Value";
                worksheet.Cell(1, 10).Value = "Reason";
                worksheet.Cell(1, 11).Value = "Comment";
                worksheet.Cell(1, 12).Value = "User";
                worksheet.Cell(1, 13).Value = "Role";
                worksheet.Cell(1, 14).Value = "DateTime";
                worksheet.Cell(1, 15).Value = "TimeZone";

                j = 2;
                Visit.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(6).SetValue(d.Action);
                    worksheet.Row(j).Cell(7).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(8).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(9).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(10).SetValue(d.Reason);
                    worksheet.Row(j).Cell(11).SetValue(d.Comment);
                    worksheet.Row(j).Cell(12).SetValue(d.User);
                    worksheet.Row(j).Cell(13).SetValue(d.Role);
                    worksheet.Row(j).Cell(14).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(15).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Visit sheet

                #region Visit Status
                worksheet = workbook.Worksheets.Add("Visit Status");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                VisitStatusData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Visit Status

                #region template
                worksheet = workbook.Worksheets.Add("Template");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "IP Address";
                worksheet.Cell(1, 7).Value = "Action";
                worksheet.Cell(1, 8).Value = "Field Name";
                worksheet.Cell(1, 9).Value = "Old Value";
                worksheet.Cell(1, 10).Value = "New Value";
                worksheet.Cell(1, 11).Value = "Reason";
                worksheet.Cell(1, 12).Value = "Comment";
                worksheet.Cell(1, 13).Value = "User";
                worksheet.Cell(1, 14).Value = "Role";
                worksheet.Cell(1, 15).Value = "DateTime";
                worksheet.Cell(1, 16).Value = "TimeZone";

                j = 2;
                Template.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(7).SetValue(d.Action);
                    worksheet.Row(j).Cell(8).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(11).SetValue(d.Reason);
                    worksheet.Row(j).Cell(12).SetValue(d.Comment);
                    worksheet.Row(j).Cell(13).SetValue(d.User);
                    worksheet.Row(j).Cell(14).SetValue(d.Role);
                    worksheet.Row(j).Cell(15).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(16).SetValue(d.TimeZone);
                    j++;
                });
                #endregion template 

                #region Variable
                worksheet = workbook.Worksheets.Add("Variable");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                Variable.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Variable

                #region Variable Value
                worksheet = workbook.Worksheets.Add("Variable Value");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                Value.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Variable Value

                #region Add Visit language sheet
                worksheet = workbook.Worksheets.Add("Visit Language");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "IP Address";
                worksheet.Cell(1, 6).Value = "Action";
                worksheet.Cell(1, 7).Value = "Field Name";
                worksheet.Cell(1, 8).Value = "Old Value";
                worksheet.Cell(1, 9).Value = "New Value";
                worksheet.Cell(1, 10).Value = "Reason";
                worksheet.Cell(1, 11).Value = "Comment";
                worksheet.Cell(1, 12).Value = "User";
                worksheet.Cell(1, 13).Value = "Role";
                worksheet.Cell(1, 14).Value = "DateTime";
                worksheet.Cell(1, 15).Value = "TimeZone";

                j = 2;
                VisitLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(6).SetValue(d.Action);
                    worksheet.Row(j).Cell(7).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(8).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(9).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(10).SetValue(d.Reason);
                    worksheet.Row(j).Cell(11).SetValue(d.Comment);
                    worksheet.Row(j).Cell(12).SetValue(d.User);
                    worksheet.Row(j).Cell(13).SetValue(d.Role);
                    worksheet.Row(j).Cell(14).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(15).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add Visit language sheet

                #region Add template language sheet
                worksheet = workbook.Worksheets.Add("Template Language");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "IP Address";
                worksheet.Cell(1, 7).Value = "Action";
                worksheet.Cell(1, 8).Value = "Field Name";
                worksheet.Cell(1, 9).Value = "Old Value";
                worksheet.Cell(1, 10).Value = "New Value";
                worksheet.Cell(1, 11).Value = "Reason";
                worksheet.Cell(1, 12).Value = "Comment";
                worksheet.Cell(1, 13).Value = "User";
                worksheet.Cell(1, 14).Value = "Role";
                worksheet.Cell(1, 15).Value = "DateTime";
                worksheet.Cell(1, 16).Value = "TimeZone";

                j = 2;
                TemplateLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(7).SetValue(d.Action);
                    worksheet.Row(j).Cell(8).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(11).SetValue(d.Reason);
                    worksheet.Row(j).Cell(12).SetValue(d.Comment);
                    worksheet.Row(j).Cell(13).SetValue(d.User);
                    worksheet.Row(j).Cell(14).SetValue(d.Role);
                    worksheet.Row(j).Cell(15).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(16).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add template language sheet

                #region Add template note sheet
                worksheet = workbook.Worksheets.Add("Template Note");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "IP Address";
                worksheet.Cell(1, 7).Value = "Action";
                worksheet.Cell(1, 8).Value = "Field Name";
                worksheet.Cell(1, 9).Value = "Old Value";
                worksheet.Cell(1, 10).Value = "New Value";
                worksheet.Cell(1, 11).Value = "Reason";
                worksheet.Cell(1, 12).Value = "Comment";
                worksheet.Cell(1, 13).Value = "User";
                worksheet.Cell(1, 14).Value = "Role";
                worksheet.Cell(1, 15).Value = "DateTime";
                worksheet.Cell(1, 16).Value = "TimeZone";

                j = 2;
                TemplateNoteData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(7).SetValue(d.Action);
                    worksheet.Row(j).Cell(8).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(11).SetValue(d.Reason);
                    worksheet.Row(j).Cell(12).SetValue(d.Comment);
                    worksheet.Row(j).Cell(13).SetValue(d.User);
                    worksheet.Row(j).Cell(14).SetValue(d.Role);
                    worksheet.Row(j).Cell(15).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(16).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add template note sheet

                #region Add template note language sheet
                worksheet = workbook.Worksheets.Add("Template Note Lang");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "IP Address";
                worksheet.Cell(1, 7).Value = "Action";
                worksheet.Cell(1, 8).Value = "Field Name";
                worksheet.Cell(1, 9).Value = "Old Value";
                worksheet.Cell(1, 10).Value = "New Value";
                worksheet.Cell(1, 11).Value = "Reason";
                worksheet.Cell(1, 12).Value = "Comment";
                worksheet.Cell(1, 13).Value = "User";
                worksheet.Cell(1, 14).Value = "Role";
                worksheet.Cell(1, 15).Value = "DateTime";
                worksheet.Cell(1, 16).Value = "TimeZone";

                j = 2;
                TemplateNoteLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(7).SetValue(d.Action);
                    worksheet.Row(j).Cell(8).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(9).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(10).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(11).SetValue(d.Reason);
                    worksheet.Row(j).Cell(12).SetValue(d.Comment);
                    worksheet.Row(j).Cell(13).SetValue(d.User);
                    worksheet.Row(j).Cell(14).SetValue(d.Role);
                    worksheet.Row(j).Cell(15).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(16).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add template note language sheet

                #region Add variable language sheet
                worksheet = workbook.Worksheets.Add("Variable Language");

                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                VariableLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add variable language sheet

                #region Add variable note language sheet
                worksheet = workbook.Worksheets.Add("Variable Note Lang");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                VariableNoteLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add variable note language sheet

                #region Add variable value language sheet
                worksheet = workbook.Worksheets.Add("Variable value Lang");

                worksheet.Rows(1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(1, 1).Value = "Key";
                worksheet.Cell(1, 2).Value = "STUDY CODE";
                worksheet.Cell(1, 3).Value = "Period";
                worksheet.Cell(1, 4).Value = "Visit";
                worksheet.Cell(1, 5).Value = "Template";
                worksheet.Cell(1, 6).Value = "Variable";
                worksheet.Cell(1, 7).Value = "IP Address";
                worksheet.Cell(1, 8).Value = "Action";
                worksheet.Cell(1, 9).Value = "Field Name";
                worksheet.Cell(1, 10).Value = "Old Value";
                worksheet.Cell(1, 11).Value = "New Value";
                worksheet.Cell(1, 12).Value = "Reason";
                worksheet.Cell(1, 13).Value = "Comment";
                worksheet.Cell(1, 14).Value = "User";
                worksheet.Cell(1, 15).Value = "Role";
                worksheet.Cell(1, 16).Value = "DateTime";
                worksheet.Cell(1, 17).Value = "TimeZone";

                j = 2;
                VariableValueLanguageData.ToList().ForEach(d =>
                {
                    worksheet.Row(j).Cell(1).SetValue(d.Key);
                    worksheet.Row(j).Cell(2).SetValue(d.StudyCode);
                    worksheet.Row(j).Cell(3).SetValue(d.Period);
                    worksheet.Row(j).Cell(4).SetValue(d.Visit);
                    worksheet.Row(j).Cell(5).SetValue(d.Template);
                    worksheet.Row(j).Cell(6).SetValue(d.Variable);
                    worksheet.Row(j).Cell(7).SetValue(d.IpAddress);
                    worksheet.Row(j).Cell(8).SetValue(d.Action);
                    worksheet.Row(j).Cell(9).SetValue(d.FieldName);
                    worksheet.Row(j).Cell(10).SetValue(d.OldValue);
                    worksheet.Row(j).Cell(11).SetValue(d.NewValue);
                    worksheet.Row(j).Cell(12).SetValue(d.Reason);
                    worksheet.Row(j).Cell(13).SetValue(d.Comment);
                    worksheet.Row(j).Cell(14).SetValue(d.User);
                    worksheet.Row(j).Cell(15).SetValue(d.Role);
                    worksheet.Row(j).Cell(16).SetValue(d.CreatedDate);
                    worksheet.Row(j).Cell(17).SetValue(d.TimeZone);
                    j++;
                });
                #endregion Add variable value language sheet

                string path = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.StudyDesignAudit.ToString());
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    stream.Position = 0;
                    var FileName = "DesignAudit_" + DateTime.Now.Ticks + ".xlsx";
                    var FilePath = Path.Combine(path, FileName);
                    workbook.SaveAs(FilePath);

                    #region Update Job Status
                    var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
                    string savepath = Path.Combine(documentUrl, FolderType.StudyDesignAudit.ToString());
                    jobMonitoring.CompletedTime = _jwtTokenAccesser.GetClientDate();
                    jobMonitoring.JobStatus = JobStatusType.Completed;
                    jobMonitoring.FolderPath = savepath;
                    jobMonitoring.FolderName = FileName;
                    _jobMonitoringRepository.Update(jobMonitoring);
                    _context.Save();
                    #endregion


                    #region EmailSend
                    var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                    var ProjectName = Project.ProjectCode + "-" + Project.ProjectName;
                    string pathofdoc = Path.Combine(savepath, FileName);
                    var linkOfDoc = "<a href='" + pathofdoc + "'>Click Here</a>";
                    _emailSenderRespository.SendDesignAuditGeneratedEMail(user.Email, _jwtTokenAccesser.UserName, ProjectName, linkOfDoc);
                    #endregion
                }

            }
            #endregion
        }

        private IList<AuditTrailDto> GetItems(IQueryable<AuditTrail> query)
        {
            return query.Select(x => new AuditTrailDto
            {
                Id = x.Id,
                TableName = x.TableName,
                RecordId = x.RecordId,
                Action = x.Action,
                ColumnName = x.ColumnName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ReasonOth = x.ReasonOth,
                UserId = x.UserId,
                CreatedDate = x.CreatedDate,
                ReasonName = x.Reason,
                UserName = x.User.UserName,
                UserRoleName = x.UserRole,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone
            }).OrderByDescending(x => x.Id).ToList();
        }

        private IList<ProjectDesignAuditReportDto> GetDesignItems(string TableName, List<int> ids, string projectName)
        {
            var query = All.AsQueryable();
            query = query.Where(x => x.TableName == TableName && ids.Contains(x.RecordId));

            return query.Select(x => new ProjectDesignAuditReportDto
            {
                Id = x.Id,
                Key = x.RecordId,
                StudyCode = projectName,
                Period = "",
                Visit = "",
                Template = "",
                Variable = "",
                Action = x.Action,
                FieldName = x.ColumnName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                Comment = x.ReasonOth,
                CreatedDate = x.CreatedDate,
                Reason = x.Reason,
                User = x.User.UserName,
                Role = x.UserRole,
                IpAddress = x.IpAddress,
                TimeZone = x.TimeZone
            }).ToList();

        }

    }
}