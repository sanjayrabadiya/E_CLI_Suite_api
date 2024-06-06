using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Report.Pdf;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Reports;
using Microsoft.EntityFrameworkCore;

namespace GSC.Report.Common
{
    public class ReportBaseRepository : IReportBaseRepository
    {

        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IGSCContext _context;

        public ReportBaseRepository(IJobMonitoringRepository jobMonitoringRepository,
             IGSCContext context)
        {

            _jobMonitoringRepository = jobMonitoringRepository;
            _context = context;
        }
        public void WriteLog(string log1, string path, JobMonitoring jobMonitoring)
        {
            string logFilePath = path;
            FileInfo logFileInfo = new FileInfo(logFilePath);
            DirectoryInfo logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            using (FileStream fileStream = new FileStream(logFilePath, FileMode.Append))
            {
                using (StreamWriter log = new StreamWriter(fileStream))
                {
                    log.WriteLine(log1);
                }
            }
            CompleteJobMonitoring(jobMonitoring);
        }

        public string CompleteJobMonitoring(JobMonitoring jobMonitoring)
        {
            #region Update JobMonitoring            
            _jobMonitoringRepository.Update(jobMonitoring);
            _context.Save();
            #endregion
            return "";
        }


        //blank report query
        public List<DossierReportDto> GetBlankPdfData(ReportSettingNew reportSetting)
        {
            var finaldata = _context.ProjectDesign.Where(x => x.ProjectId == reportSetting.ProjectId).Select(x => new DossierReportDto
            {
                ProjectDetails = new ProjectDetails { ProjectCode = x.Project.ProjectCode, ProjectName = x.Project.ProjectName, ClientId = x.Project.ClientId, ProjectDesignId = x.Id },
                Period = x.ProjectDesignPeriods.Where(a => a.DeletedDate == null).Select(a => new ProjectDesignPeriodReportDto
                {
                    DisplayName = a.DisplayName,
                    Visit = a.VisitList.Where(b => b.DeletedDate == null
                    && ((reportSetting.CRFType == CRFTypes.CRF && !b.IsNonCRF)
                    || (reportSetting.CRFType == CRFTypes.NonCRF && b.IsNonCRF)
                    || (reportSetting.CRFType == CRFTypes.Both && (b.IsNonCRF || !b.IsNonCRF))
                    ) && (reportSetting.VisitIds == null || (reportSetting.VisitIds != null && reportSetting.VisitIds.Contains(b.Id)))
                    ).Select(b => new ProjectDesignVisitList
                    {
                        DisplayName = b.DisplayName,
                        DesignOrder = b.DesignOrder,
                        ProjectDesignTemplatelist = b.Templates.Where(n => n.DeletedDate == null
                        && (reportSetting.TemplateIds == null || (reportSetting.TemplateIds != null && reportSetting.TemplateIds.Contains(n.Id))
                        )).Select(n => new ProjectDesignTemplatelist
                        {
                            TemplateCode = n.TemplateCode,
                            TemplateName = n.TemplateName,
                            DesignOrder = n.DesignOrder,
                            Label = n.Label,
                            PreLabel = n.PreLabel,
                            Domain = new DomainReportDto { DomainCode = n.Domain.DomainCode, DomainName = n.Domain.DomainName },
                            TemplateNotes = n.ProjectDesignTemplateNote.Where(tn => tn.DeletedDate == null && (tn.IsBottom == false || tn.IsBottom == null)).Select(tn => new ProjectDesignTemplateNoteReportDto { Notes = tn.Note, IsPreview = tn.IsPreview, IsBottom = tn.IsBottom }).ToList(),
                            TemplateNotesBottom = n.ProjectDesignTemplateNote.Where(tn => tn.DeletedDate == null && tn.IsBottom == true).Select(tn => new ProjectDesignTemplateNoteReportDto { Notes = tn.Note, IsPreview = tn.IsPreview, IsBottom = tn.IsBottom }).ToList(),

                            ProjectDesignVariable = n.Variables.Where(v => v.DeletedDate == null).Select(v => new ProjectDesignVariableReportDto
                            {
                                Id = v.Id,
                                VariableName = v.VariableName,
                                VariableCode = v.VariableCode,
                                DesignOrder = v.DesignOrder,
                                IsNa = v.IsNa,
                                CollectionSource = v.CollectionSource,
                                Annotation = v.Annotation,
                                CollectionAnnotation = v.CollectionAnnotation,
                                Note = v.Note,
                                DefaultValue = v.DefaultValue,
                                LowRangeValue = v.LowRangeValue,
                                HighRangeValue = v.HighRangeValue,
                                LargeStep = v.LargeStep,
                                Unit = new UnitReportDto
                                {
                                    UnitName = v.Unit.UnitName,
                                    UnitAnnotation = v.UnitAnnotation
                                },
                                Values = v.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto { Id = vd.Id, ValueName = vd.ValueName, SeqNo = vd.SeqNo, ValueCode = vd.ValueCode, Label = vd.Label }).OrderBy(vd => vd.SeqNo).ToList(),
                                VariableCategoryName = v.VariableCategory.CategoryName,
                                Label = v.Label,
                                PreLabel = v.PreLabel,
                                IsLevelNo = v.IsLevelNo
                            }).ToList()
                        }).ToList()
                    }).OrderBy(d => d.DesignOrder).ToList()
                }).ToList()
            }).ToList();
            return finaldata;

        }
        //data query
        public List<DossierReportDto> GetDataPdfReport(ReportSettingNew reportSetting)
        {

            var finaldata = _context.ScreeningEntry.Where(a => reportSetting.SiteId.Contains(a.ProjectId) && a.DeletedDate == null &&
              (reportSetting.SubjectIds == null || reportSetting.SubjectIds.Select(x => x.Id).ToList().Contains((int)a.RandomizationId)))
              .Select(x => new DossierReportDto
              {
                  ScreeningNumber = x.Randomization.ScreeningNumber,
                  Initial = x.Randomization.Initial,
                  RandomizationNumber = x.Randomization.RandomizationNumber,
                  ProjectDetails = new ProjectDetails
                  {
                      ProjectCode = _context.Project.Where(p => p.Id == x.Project.ParentProjectId).FirstOrDefault().ProjectCode,
                      ProjectName = _context.Project.Where(p => p.Id == x.Project.ParentProjectId).FirstOrDefault().ProjectName,
                      ClientId = x.Project.ClientId,
                      ProjectDesignId = x.ProjectDesignId
                  },
                  Period = new List<ProjectDesignPeriodReportDto> {
          new ProjectDesignPeriodReportDto {
            DisplayName = x.ProjectDesignPeriod.DisplayName,
              Visit = x.ScreeningVisit.Where(x => x.Status != ScreeningVisitStatus.NotStarted && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate==null
                     && ((reportSetting.CRFType == CRFTypes.CRF && !x.ProjectDesignVisit.IsNonCRF)
                    || (reportSetting.CRFType == CRFTypes.NonCRF && x.ProjectDesignVisit.IsNonCRF)
                    || (reportSetting.CRFType == CRFTypes.Both && (x.ProjectDesignVisit.IsNonCRF || !x.ProjectDesignVisit.IsNonCRF))
                    ) && (reportSetting.VisitIds == null || (reportSetting.VisitIds != null && reportSetting.VisitIds.Contains(x.ProjectDesignVisitId)))
                    ).Select(x => new ProjectDesignVisitList {
                  DisplayName = x.RepeatedVisitNumber==null ?x.ProjectDesignVisit.DisplayName:x.ProjectDesignVisit.DisplayName+"_"+x.RepeatedVisitNumber,
                  DesignOrder = x.ProjectDesignVisit.DesignOrder,
                  ProjectDesignTemplatelist = x.ScreeningTemplates.Where(a => a.Status != ScreeningTemplateStatus.Pending &&
                    a.DeletedDate == null  && a.ProjectDesignTemplate.DeletedDate == null
                   && (reportSetting.TemplateIds == null || (reportSetting.TemplateIds != null && reportSetting.TemplateIds.Contains(a.ProjectDesignTemplateId))
                        )).Select(a => new ProjectDesignTemplatelist {
                      TemplateCode = a.ProjectDesignTemplate.TemplateCode,
                      TemplateName = a.ProjectDesignTemplate.TemplateName,
                      DesignOrder = a.ProjectDesignTemplate.DesignOrder,
                      Label = a.ProjectDesignTemplate.Label,
                      PreLabel  = a.ProjectDesignTemplate.PreLabel,
                      RepeatSeqNo=a.RepeatSeqNo,
                      Domain = new DomainReportDto {
                        DomainCode = a.ProjectDesignTemplate.Domain.DomainCode, DomainName = a.ProjectDesignTemplate.Domain.DomainName
                      },
                      TemplateNotes = a.ProjectDesignTemplate.ProjectDesignTemplateNote.Select(n => new ProjectDesignTemplateNoteReportDto {
                        Notes = n.Note, IsPreview = n.IsPreview,IsBottom=n.IsBottom
                      }).Where(tn => tn.IsBottom == false || tn.IsBottom == null).ToList(),
                      TemplateNotesBottom = a.ProjectDesignTemplate.ProjectDesignTemplateNote.Select(n => new ProjectDesignTemplateNoteReportDto {
                        Notes = n.Note, IsPreview = n.IsPreview,IsBottom = n.IsBottom
                      }).Where(tn => tn.IsBottom == true).ToList(),
                      ProjectDesignVariable = a.ScreeningTemplateValues.Where(s => s.DeletedDate == null && s.ProjectDesignVariable.DeletedDate == null).Select(s => new ProjectDesignVariableReportDto {
                          Id = s.ProjectDesignVariable.Id,
                          VariableName = s.ProjectDesignVariable.VariableName,
                          VariableCode = s.ProjectDesignVariable.VariableCode,
                          DesignOrder = s.ProjectDesignVariable.DesignOrder,
                          IsNa = s.ProjectDesignVariable.IsNa,
                          CollectionSource = s.ProjectDesignVariable.CollectionSource,
                          Annotation=s.ProjectDesignVariable.Annotation,
                          CollectionAnnotation=s.ProjectDesignVariable.CollectionAnnotation,
                          Note=s.ProjectDesignVariable.Note,
                          Unit = new UnitReportDto {
                            UnitName = s.ProjectDesignVariable.Unit.UnitName,
                            UnitAnnotation = s.ProjectDesignVariable.UnitAnnotation
                          },
                          Values = s.ProjectDesignVariable.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto {
                            Id = vd.Id,
                            ValueName = vd.ValueName,
                            SeqNo = vd.SeqNo,
                            ValueCode = vd.ValueCode,
                            Label = vd.Label,
                            TableCollectionSource = vd.TableCollectionSource

                          }).ToList(),
                          ScreeningValue= s.Value,
                          ScreeningIsNa=s.IsNa,
                          ScreeningTemplateValueId=s.Id,
                          ValueChild=s.Children.Where(c=>c.DeletedDate==null).Select(c=>new ScreeningTemplateValueChildReportDto{
                              Value=c.Value,
                              ProjectDesignVariableValueId=c.ProjectDesignVariableValueId,
                              ScreeningTemplateValueId=c.ScreeningTemplateValueId,
                              ValueName=c.ProjectDesignVariableValue.ValueName,
                              LevelNo = c.LevelNo,
                              DeletedDate = c.DeletedDate
                          }).ToList(),
                          VariableCategoryName = s.ProjectDesignVariable.VariableCategory.CategoryName,
                          Label = s.ProjectDesignVariable.Label,
                          PreLabel = s.ProjectDesignVariable.PreLabel,
                          IsDocument =  s.ProjectDesignVariable.IsDocument,
                          DocPath =  s.DocPath,
                          MimeType = s.MimeType,
                          IsLevelNo = s.ProjectDesignVariable.IsLevelNo
                      }).ToList(),
                      ScreeningTemplateReview=a.ScreeningTemplateReview.Where(r=>r.DeletedDate==null).Select(r=>new ScreeningTemplateReviewReportDto{
                      ScreeningTemplateId=r.ScreeningTemplateId,
                      ReviewLevel=r.ReviewLevel,
                      RoleId=r.RoleId,
                      RoleName=r.Role.RoleName,
                      CreatedByUser=r.CreatedByUser.UserName,
                      CreatedDate=r.CreatedDate
                      }).ToList()
                  }).ToList()
              }).OrderBy(x=>x.DesignOrder).ToList()

          }
                  }
              }).ToList();
            return finaldata;
        }

        public List<ScreeningPdfReportDto> GetScreeningBlankPdfData(ScreeningReportSetting reportSetting)
        {

            var finaldata = _context.ProjectDesign.Where(x => x.ProjectId == reportSetting.ProjectId)
                .Select(x => new ScreeningPdfReportDto
                {
                    ProjectDetails = new ProjectDetails { ProjectCode = x.Project.ProjectCode, ProjectName = x.Project.ProjectName, ClientId = x.Project.ClientId },
                    Period = x.ProjectDesignPeriods.Where(a => a.DeletedDate == null)
                    .Select(a => new ProjectDesignPeriodReportDto
                    {
                        DisplayName = a.DisplayName,
                        Visit = a.VisitList.Where(b => b.DeletedDate == null).Select(b => new ProjectDesignVisitList
                        {
                            DisplayName = b.DisplayName,
                            DesignOrder = b.DesignOrder,
                            ProjectDesignTemplatelist = b.Templates.Where(n => n.DeletedDate == null).Select(n => new ProjectDesignTemplatelist
                            {
                                TemplateCode = n.TemplateCode,
                                TemplateName = n.TemplateName,
                                DesignOrder = n.DesignOrder,
                                Label = n.Label,
                                Domain = new DomainReportDto { DomainCode = n.Domain.DomainCode, DomainName = n.Domain.DomainName },
                                TemplateNotes = n.ProjectDesignTemplateNote.Where(tn => tn.DeletedDate == null && (tn.IsBottom == false || tn.IsBottom == null)).Select(tn => new ProjectDesignTemplateNoteReportDto { Notes = tn.Note, IsPreview = tn.IsPreview, IsBottom = tn.IsBottom }).ToList(),
                                TemplateNotesBottom = n.ProjectDesignTemplateNote.Where(tn => tn.DeletedDate == null && tn.IsBottom == true).Select(tn => new ProjectDesignTemplateNoteReportDto { Notes = tn.Note, IsPreview = tn.IsPreview, IsBottom = tn.IsBottom }).ToList(),
                                ProjectDesignVariable = n.Variables.Where(v => v.DeletedDate == null).Select(v => new ProjectDesignVariableReportDto
                                {
                                    Id = v.Id,
                                    VariableName = v.VariableName,
                                    VariableCode = v.VariableCode,
                                    DesignOrder = v.DesignOrder,
                                    IsNa = v.IsNa,
                                    CollectionSource = v.CollectionSource,
                                    Annotation = v.Annotation,
                                    CollectionAnnotation = v.CollectionAnnotation,
                                    Note = v.Note,
                                    DefaultValue = v.DefaultValue,
                                    LowRangeValue = v.LowRangeValue,
                                    HighRangeValue = v.HighRangeValue,
                                    LargeStep = v.LargeStep,
                                    Unit = new UnitReportDto
                                    {
                                        UnitName = v.Unit.UnitName,
                                        UnitAnnotation = v.UnitAnnotation
                                    },
                                    Values = v.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto { Id = vd.Id, ValueName = vd.ValueName, SeqNo = vd.SeqNo, ValueCode = vd.ValueCode, Label = vd.Label }).OrderBy(vd => vd.SeqNo).ToList(),
                                    VariableCategoryName = v.VariableCategory.CategoryName,
                                    Label = v.Label
                                }).ToList()
                            }).ToList()
                        }).OrderBy(d => d.DesignOrder).ToList()
                    }).ToList()
                }).ToList();
            return finaldata;
        }

        public List<ScreeningPdfReportDto> GetScreeningDataPdfReport(ScreeningReportSetting reportSetting)
        {

            var finaldata = _context.ScreeningEntry.Where(a => a.ProjectId == reportSetting.ProjectId && a.DeletedDate == null
            && (reportSetting.StudyId == null || a.StudyId == reportSetting.StudyId)
            && (reportSetting.VolunteerId == null || a.Attendance.VolunteerId == reportSetting.VolunteerId)
            && (reportSetting.ScreeningDate == null || reportSetting.ScreeningDate == "" || a.ScreeningDate.Date == Convert.ToDateTime(reportSetting.ScreeningDate).Date))
              .Select(x => new ScreeningPdfReportDto
              {
                  ScreeningNumber = x.ScreeningNo,
                  Initial = x.Attendance.Volunteer.AliasName,
                  VolunteerNumber = x.Attendance.Volunteer.VolunteerNo,
                  VolunteerId = x.Attendance.Volunteer.Id,
                  ScreeningDate = x.ScreeningDate,
                  ProjectDetails = new ProjectDetails
                  {
                      ProjectCode = x.Project.ProjectCode,
                      ProjectName = x.Project.ProjectName,
                      ClientId = x.Project.ClientId,
                      ProjectDesignId = x.ProjectDesignId
                  },
                  Period = new List<ProjectDesignPeriodReportDto> {
                           new ProjectDesignPeriodReportDto {
                            DisplayName = x.ProjectDesignPeriod.DisplayName,
                            Visit = x.ScreeningVisit.Where(x => x.Status != ScreeningVisitStatus.NotStarted && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate==null

                                    ).Select(x => new ProjectDesignVisitList {
                                          DisplayName = x.RepeatedVisitNumber==null ?x.ProjectDesignVisit.DisplayName:x.ProjectDesignVisit.DisplayName+"_"+x.RepeatedVisitNumber,
                                          DesignOrder = x.ProjectDesignVisit.DesignOrder,
                                          ProjectDesignTemplatelist = x.ScreeningTemplates.Where(a => a.Status != ScreeningTemplateStatus.Pending && a.DeletedDate == null  && a.ProjectDesignTemplate.DeletedDate == null)
                                          .Select(a => new ProjectDesignTemplatelist {
                                              TemplateCode = a.ProjectDesignTemplate.TemplateCode,
                                              TemplateName = a.ProjectDesignTemplate.TemplateName,
                                              DesignOrder = a.ProjectDesignTemplate.DesignOrder,
                                              Label = a.ProjectDesignTemplate.Label,
                                              RepeatSeqNo=a.RepeatSeqNo,
                                              Domain = new DomainReportDto {
                                                DomainCode = a.ProjectDesignTemplate.Domain.DomainCode, DomainName = a.ProjectDesignTemplate.Domain.DomainName
                                              },
                                              TemplateNotes = a.ProjectDesignTemplate.ProjectDesignTemplateNote.Where(n=>n.DeletedDate == null).Select(n => new ProjectDesignTemplateNoteReportDto {
                                                Notes = n.Note, IsPreview = n.IsPreview,IsBottom=n.IsBottom
                                              }).Where(tn => tn.IsBottom == false || tn.IsBottom == null).ToList(),
                                              TemplateNotesBottom = a.ProjectDesignTemplate.ProjectDesignTemplateNote.Where(n=>n.DeletedDate == null).Select(n => new ProjectDesignTemplateNoteReportDto {
                                                Notes = n.Note, IsPreview = n.IsPreview,IsBottom = n.IsBottom
                                              }).Where(tn => tn.IsBottom == true).ToList(),
                                              ProjectDesignVariable = a.ScreeningTemplateValues.Where(s => s.DeletedDate == null && s.ProjectDesignVariable.DeletedDate == null).Select(s => new ProjectDesignVariableReportDto {
                                                  Id = s.ProjectDesignVariable.Id,
                                                  VariableName = s.ProjectDesignVariable.VariableName,
                                                  VariableCode = s.ProjectDesignVariable.VariableCode,
                                                  DesignOrder = s.ProjectDesignVariable.DesignOrder,
                                                  IsNa = s.ProjectDesignVariable.IsNa,
                                                  CollectionSource = s.ProjectDesignVariable.CollectionSource,
                                                  Annotation=s.ProjectDesignVariable.Annotation,
                                                  CollectionAnnotation=s.ProjectDesignVariable.CollectionAnnotation,
                                                  Note=s.ProjectDesignVariable.Note,
                                                  Unit = new UnitReportDto {
                                                    UnitName = s.ProjectDesignVariable.Unit.UnitName,
                                                    UnitAnnotation = s.ProjectDesignVariable.UnitAnnotation
                                                  },
                                                  Values = s.ProjectDesignVariable.Values.Where(vd => vd.DeletedDate == null).Select(vd => new ProjectDesignVariableValueReportDto {
                                                    Id = vd.Id, ValueName = vd.ValueName, SeqNo = vd.SeqNo, ValueCode = vd.ValueCode, Label = vd.Label,TableCollectionSource = vd.TableCollectionSource
                                                  }).ToList(),
                                                  ScreeningValue= s.Value,
                                                  ScreeningIsNa=s.IsNa,
                                                  ScreeningTemplateValueId=s.Id,
                                                  ValueChild=s.Children.Where(c=>c.DeletedDate==null).Select(c=>new ScreeningTemplateValueChildReportDto{
                                                      Value=c.Value,
                                                      ProjectDesignVariableValueId=c.ProjectDesignVariableValueId,
                                                      ScreeningTemplateValueId=c.ScreeningTemplateValueId,
                                                      ValueName=c.ProjectDesignVariableValue.ValueName,
                                                      LevelNo = c.LevelNo,
                                                      DeletedDate = c.DeletedDate
                                                  }).ToList(),
                                                  VariableCategoryName = s.ProjectDesignVariable.VariableCategory.CategoryName,
                                                  Label = s.ProjectDesignVariable.Label
                                              }).ToList(),
                                              ScreeningTemplateReview=a.ScreeningTemplateReview.Where(r=>r.DeletedDate==null).Select(r=>new ScreeningTemplateReviewReportDto{
                                              ScreeningTemplateId=r.ScreeningTemplateId,
                                              ReviewLevel=r.ReviewLevel,
                                              RoleId=r.RoleId,
                                              RoleName=r.Role.RoleName,
                                              CreatedByUser=r.CreatedByUser.UserName,
                                              CreatedDate=r.CreatedDate
                                          }).ToList()
                                      }).ToList()
                                    }).Where(c=>c.ProjectDesignTemplatelist.Count!= 0).OrderBy(x=>x.DesignOrder).ToList()

                           }
                  }
              }).ToList();

            return finaldata.Where(x => x.Period.TrueForAll(y => y.Visit.Count != 0)).ToList();
        }

        public List<ScreeningPdfReportDto> GetScreeningPdfData(ScreeningReportSetting reportSetting)
        {
            if (reportSetting.PdfStatus == DossierPdfStatus.Blank)
            {
                return GetScreeningBlankPdfData(reportSetting);
            }
            else
            {
                return GetScreeningDataPdfReport(reportSetting);

            }
        }


        public async Task<List<DossierReportDto>> GetBlankPdfDataAsync(ReportSettingNew reportSetting)
        {
            var query = _context.ProjectDesign
                .Where(x => x.ProjectId == reportSetting.ProjectId)
                .Select(x => new
                {
                    ProjectDetails = new ProjectDetails
                    {
                        ProjectCode = x.Project.ProjectCode,
                        ProjectName = x.Project.ProjectName,
                        ClientId = x.Project.ClientId,
                        ProjectDesignId = x.Id
                    },
                    Periods = x.ProjectDesignPeriods.Where(a => a.DeletedDate == null).Select(a => new
                    {
                        a.DisplayName,
                        Visits = a.VisitList.Where(b => b.DeletedDate == null
                            && ((reportSetting.CRFType == CRFTypes.CRF && !b.IsNonCRF)
                            || (reportSetting.CRFType == CRFTypes.NonCRF && b.IsNonCRF)
                            || (reportSetting.CRFType == CRFTypes.Both && (b.IsNonCRF || !b.IsNonCRF)))
                            && (reportSetting.VisitIds == null || reportSetting.VisitIds.Contains(b.Id)))
                            .Select(b => new
                            {
                                b.DisplayName,
                                b.DesignOrder,
                                Templates = b.Templates.Where(n => n.DeletedDate == null
                                    && (reportSetting.TemplateIds == null || reportSetting.TemplateIds.Contains(n.Id)))
                                    .Select(n => new
                                    {
                                        n.TemplateCode,
                                        n.TemplateName,
                                        n.DesignOrder,
                                        n.Label,
                                        n.PreLabel,
                                        Domain = new DomainReportDto
                                        {
                                            DomainCode = n.Domain.DomainCode,
                                            DomainName = n.Domain.DomainName
                                        },
                                        TemplateNotes = n.ProjectDesignTemplateNote
                                            .Where(tn => tn.DeletedDate == null && (tn.IsBottom == false || tn.IsBottom == null))
                                            .Select(tn => new ProjectDesignTemplateNoteReportDto
                                            {
                                                Notes = tn.Note,
                                                IsPreview = tn.IsPreview,
                                                IsBottom = tn.IsBottom
                                            }).ToList(),
                                        TemplateNotesBottom = n.ProjectDesignTemplateNote
                                            .Where(tn => tn.DeletedDate == null && tn.IsBottom == true)
                                            .Select(tn => new ProjectDesignTemplateNoteReportDto
                                            {
                                                Notes = tn.Note,
                                                IsPreview = tn.IsPreview,
                                                IsBottom = tn.IsBottom
                                            }).ToList(),
                                        Variables = n.Variables.Where(v => v.DeletedDate == null)
                                            .Select(v => new ProjectDesignVariableReportDto
                                            {
                                                Id = v.Id,
                                                VariableName = v.VariableName,
                                                VariableCode = v.VariableCode,
                                                DesignOrder = v.DesignOrder,
                                                IsNa = v.IsNa,
                                                CollectionSource = v.CollectionSource,
                                                Annotation = v.Annotation,
                                                CollectionAnnotation = v.CollectionAnnotation,
                                                Note = v.Note,
                                                DefaultValue = v.DefaultValue,
                                                LowRangeValue = v.LowRangeValue,
                                                HighRangeValue = v.HighRangeValue,
                                                LargeStep = v.LargeStep,
                                                Unit = new UnitReportDto
                                                {
                                                    UnitName = v.Unit.UnitName,
                                                    UnitAnnotation = v.UnitAnnotation
                                                },
                                                Values = v.Values.Where(vd => vd.DeletedDate == null)
                                                    .Select(vd => new ProjectDesignVariableValueReportDto
                                                    {
                                                        Id = vd.Id,
                                                        ValueName = vd.ValueName,
                                                        SeqNo = vd.SeqNo,
                                                        ValueCode = vd.ValueCode,
                                                        Label = vd.Label
                                                    }).OrderBy(vd => vd.SeqNo).ToList(),
                                                VariableCategoryName = v.VariableCategory.CategoryName,
                                                Label = v.Label,
                                                PreLabel = v.PreLabel,
                                                IsLevelNo = v.IsLevelNo
                                            }).ToList()
                                    }).ToList()
                            }).OrderBy(b => b.DesignOrder).ToList()
                    }).ToList()
                }).ToListAsync();

            var finalData = (await query).Select(x => new DossierReportDto
            {
                ProjectDetails = x.ProjectDetails,
                Period = x.Periods.Select(a => new ProjectDesignPeriodReportDto
                {
                    DisplayName = a.DisplayName,
                    Visit = a.Visits.Select(b => new ProjectDesignVisitList
                    {
                        DisplayName = b.DisplayName,
                        DesignOrder = b.DesignOrder,
                        ProjectDesignTemplatelist = b.Templates.Select(n => new ProjectDesignTemplatelist
                        {
                            TemplateCode = n.TemplateCode,
                            TemplateName = n.TemplateName,
                            DesignOrder = n.DesignOrder,
                            Label = n.Label,
                            PreLabel = n.PreLabel,
                            Domain = n.Domain,
                            TemplateNotes = n.TemplateNotes,
                            TemplateNotesBottom = n.TemplateNotesBottom,
                            ProjectDesignVariable = n.Variables
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            return finalData;
        }

        public async Task<List<DossierReportDto>> GetDataPdfReportAsync(ReportSettingNew reportSetting)
        {
            var projectIds = reportSetting.SiteId;
            var subjectIds = reportSetting.SubjectIds?.Select(x => x.Id).ToList();

            var query = _context.ScreeningEntry
                .Where(a => projectIds.Contains(a.ProjectId) && a.DeletedDate == null &&
                            (subjectIds == null || subjectIds.Contains((int)a.RandomizationId)))
                .Select(x => new
                {
                    x.Randomization.ScreeningNumber,
                    x.Randomization.Initial,
                    x.Randomization.RandomizationNumber,
                    ProjectDetails = new
                    {
                        ProjectCode = _context.Project.Where(p => p.Id == x.Project.ParentProjectId).FirstOrDefault().ProjectCode,
                        ProjectName = _context.Project.Where(p => p.Id == x.Project.ParentProjectId).FirstOrDefault().ProjectName,
                        ClientId = x.Project.ClientId,
                        ProjectDesignId = x.ProjectDesignId
                    },
                    Periods = x.ScreeningVisit.GroupBy(s => s.ProjectDesignVisit.ProjectDesignPeriod.DisplayName).Select(p => new
                    {
                        DisplayName = p.Key,
                        Visit = x.ScreeningVisit.Where(v => v.Status != ScreeningVisitStatus.NotStarted && v.DeletedDate == null && v.ProjectDesignVisit.DeletedDate == null
                            && ((reportSetting.CRFType == CRFTypes.CRF && !v.ProjectDesignVisit.IsNonCRF)
                            || (reportSetting.CRFType == CRFTypes.NonCRF && v.ProjectDesignVisit.IsNonCRF)
                            || (reportSetting.CRFType == CRFTypes.Both && (v.ProjectDesignVisit.IsNonCRF || !v.ProjectDesignVisit.IsNonCRF)))
                            && (reportSetting.VisitIds == null || reportSetting.VisitIds.Contains(v.ProjectDesignVisitId)))
                          .Select(v => new
                          {
                              DisplayName = v.RepeatedVisitNumber == null ? v.ProjectDesignVisit.DisplayName : v.ProjectDesignVisit.DisplayName + "_" + v.RepeatedVisitNumber,
                              v.ProjectDesignVisit.DesignOrder,
                              ProjectDesignTemplatelist = v.ScreeningTemplates.Where(t => t.Status != ScreeningTemplateStatus.Pending &&
                                  t.DeletedDate == null && t.ProjectDesignTemplate.DeletedDate == null
                                  && (reportSetting.TemplateIds == null || reportSetting.TemplateIds.Contains(t.ProjectDesignTemplateId)))
                                .Select(t => new
                                {
                                    t.ProjectDesignTemplate.TemplateCode,
                                    t.ProjectDesignTemplate.TemplateName,
                                    t.ProjectDesignTemplate.DesignOrder,
                                    t.ProjectDesignTemplate.Label,
                                    t.ProjectDesignTemplate.PreLabel,
                                    t.RepeatSeqNo,
                                    Domain = new DomainReportDto
                                    {
                                        DomainCode = t.ProjectDesignTemplate.Domain.DomainCode,
                                        DomainName = t.ProjectDesignTemplate.Domain.DomainName
                                    },
                                    TemplateNotes = t.ProjectDesignTemplate.ProjectDesignTemplateNote
                                        .Where(n => n.DeletedDate == null && (n.IsBottom == false || n.IsBottom == null))
                                        .Select(n => new ProjectDesignTemplateNoteReportDto
                                        {
                                            Notes = n.Note,
                                            IsPreview = n.IsPreview,
                                            IsBottom = n.IsBottom
                                        }).ToList(),
                                    TemplateNotesBottom = t.ProjectDesignTemplate.ProjectDesignTemplateNote
                                        .Where(n => n.DeletedDate == null && n.IsBottom == true)
                                        .Select(n => new ProjectDesignTemplateNoteReportDto
                                        {
                                            Notes = n.Note,
                                            IsPreview = n.IsPreview,
                                            IsBottom = n.IsBottom
                                        }).ToList(),
                                    ProjectDesignVariable = t.ScreeningTemplateValues.Where(s => s.DeletedDate == null && s.ProjectDesignVariable.DeletedDate == null)
                                        .Select(s => new ProjectDesignVariableReportDto
                                        {
                                            Id = s.ProjectDesignVariable.Id,
                                            VariableName = s.ProjectDesignVariable.VariableName,
                                            VariableCode = s.ProjectDesignVariable.VariableCode,
                                            DesignOrder = s.ProjectDesignVariable.DesignOrder,
                                            IsNa = s.ProjectDesignVariable.IsNa,
                                            CollectionSource = s.ProjectDesignVariable.CollectionSource,
                                            Annotation = s.ProjectDesignVariable.Annotation,
                                            CollectionAnnotation = s.ProjectDesignVariable.CollectionAnnotation,
                                            Note = s.ProjectDesignVariable.Note,
                                            Unit = new UnitReportDto
                                            {
                                                UnitName = s.ProjectDesignVariable.Unit.UnitName,
                                                UnitAnnotation = s.ProjectDesignVariable.UnitAnnotation
                                            },
                                            Values = s.ProjectDesignVariable.Values.Where(vd => vd.DeletedDate == null)
                                                .Select(vd => new ProjectDesignVariableValueReportDto
                                                {
                                                    Id = vd.Id,
                                                    ValueName = vd.ValueName,
                                                    SeqNo = vd.SeqNo,
                                                    ValueCode = vd.ValueCode,
                                                    Label = vd.Label,
                                                    TableCollectionSource = vd.TableCollectionSource
                                                }).ToList(),
                                            ScreeningValue = s.Value,
                                            ScreeningIsNa = s.IsNa,
                                            ScreeningTemplateValueId = s.Id,
                                            ValueChild = s.Children.Where(c => c.DeletedDate == null)
                                                .Select(c => new ScreeningTemplateValueChildReportDto
                                                {
                                                    Value = c.Value,
                                                    ProjectDesignVariableValueId = c.ProjectDesignVariableValueId,
                                                    ScreeningTemplateValueId = c.ScreeningTemplateValueId,
                                                    ValueName = c.ProjectDesignVariableValue.ValueName,
                                                    LevelNo = c.LevelNo,
                                                    DeletedDate = c.DeletedDate
                                                }).ToList(),
                                            VariableCategoryName = s.ProjectDesignVariable.VariableCategory.CategoryName,
                                            Label = s.ProjectDesignVariable.Label,
                                            PreLabel = s.ProjectDesignVariable.PreLabel,
                                            IsDocument = s.ProjectDesignVariable.IsDocument,
                                            DocPath = s.DocPath,
                                            MimeType = s.MimeType,
                                            IsLevelNo = s.ProjectDesignVariable.IsLevelNo
                                        }).ToList(),
                                    ScreeningTemplateReview = t.ScreeningTemplateReview.Where(r => r.DeletedDate == null)
                                        .Select(r => new ScreeningTemplateReviewReportDto
                                        {
                                            ScreeningTemplateId = r.ScreeningTemplateId,
                                            ReviewLevel = r.ReviewLevel,
                                            RoleId = r.RoleId,
                                            RoleName = r.Role.RoleName,
                                            CreatedByUser = r.CreatedByUser.UserName,
                                            CreatedDate = r.CreatedDate
                                        }).ToList()
                                }).ToList()
                          }).OrderBy(v => v.DesignOrder).ToList(),
                    }).ToList()
                }).ToListAsync();

            var finalData = (await query).Select(x => new DossierReportDto
            {
                ScreeningNumber = x.ScreeningNumber,
                Initial = x.Initial,
                RandomizationNumber = x.RandomizationNumber,
                ProjectDetails = new ProjectDetails
                {
                    ProjectCode = x.ProjectDetails.ProjectCode,
                    ProjectName = x.ProjectDetails.ProjectName,
                    ClientId = x.ProjectDetails.ClientId,
                    ProjectDesignId = x.ProjectDetails.ProjectDesignId
                },
                Period = x.Periods.Select(p => new ProjectDesignPeriodReportDto
                {
                    DisplayName = p.DisplayName,
                    Visit = p.Visit.Select(v => new ProjectDesignVisitList
                    {
                        DisplayName = v.DisplayName,
                        DesignOrder = v.DesignOrder,
                        ProjectDesignTemplatelist = v.ProjectDesignTemplatelist.Select(t => new ProjectDesignTemplatelist
                        {
                            TemplateCode = t.TemplateCode,
                            TemplateName = t.TemplateName,
                            DesignOrder = t.DesignOrder,
                            Label = t.Label,
                            PreLabel = t.PreLabel,
                            RepeatSeqNo = t.RepeatSeqNo,
                            Domain = t.Domain,
                            TemplateNotes = t.TemplateNotes,
                            TemplateNotesBottom = t.TemplateNotesBottom,
                            ProjectDesignVariable = t.ProjectDesignVariable,
                            ScreeningTemplateReview = t.ScreeningTemplateReview
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            return finalData;
        }


    }
}