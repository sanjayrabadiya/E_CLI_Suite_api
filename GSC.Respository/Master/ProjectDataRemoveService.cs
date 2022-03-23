using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Shared.Caching;
using GSC.Shared.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GSC.Respository.Master
{
    public class ProjectDataRemoveService : IProjectDataRemoveService
    {
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly HttpClient _httpClient;
        private readonly IGSCCaching _gSCCaching;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IGSCContext _context;
        public ProjectDataRemoveService(ILoginPreferenceRepository loginPreferenceRepository,
            HttpClient httpClient,
            IGSCCaching gSCCaching, IUserLoginReportRespository userLoginReportRepository,
            IOptions<EnvironmentSetting> environmentSetting, IGSCContext context)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _httpClient = httpClient;
            _gSCCaching = gSCCaching;
            _userLoginReportRepository = userLoginReportRepository;
            _environmentSetting = environmentSetting;
            _context = context;
        }
        public async Task<ProjectRemoveDataSuccess> AdverseEventRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var data = _context.AdverseEventSettings.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (data.Count > 0)
                {
                    data.ForEach(x =>
                    {
                        var aereporting = _context.AEReporting.Where(s => s.AdverseEventSettingsId == x.Id).ToList();
                        aereporting.ForEach(z =>
                        {

                            var aeReportingValue = _context.AEReportingValue.Where(ae => ae.AEReportingId == z.Id).ToList();
                            if (aeReportingValue != null)
                                _context.AEReportingValue.RemoveRange(aeReportingValue);
                        });
                        if (aereporting != null)
                            _context.AEReporting.RemoveRange(aereporting);

                        var adversedetails = _context.AdverseEventSettingsDetails.Where(s => s.AdverseEventSettingsId == x.Id).ToList();
                        if (adversedetails != null)
                            _context.AdverseEventSettingsDetails.RemoveRange(adversedetails);

                    });
                    if (data != null)
                        _context.AdverseEventSettings.RemoveRange(data);
                    _context.Save();
                }
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> InformConsentRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var data = _context.EconsentReviewDetailsSections.Where(x => x.EconsentReviewDetails.EconsentSetup.ProjectId == obj.ProjectId).ToList();
                if (data.Count > 0)
                {
                    data.ForEach(x =>
                    {
                        var econsentReviewDetailsAudit = _context.EconsentReviewDetailsAudit.Where(z => z.EconsentReviewDetailsId == x.EconsentReviewDetailId).ToList();
                        if (econsentReviewDetailsAudit.Count > 0)
                            _context.EconsentReviewDetailsAudit.RemoveRange(econsentReviewDetailsAudit);
                    });
                    _context.EconsentReviewDetailsSections.RemoveRange(data);
                }
                var econsentSetup = _context.EconsentReviewDetails.Where(x => x.EconsentSetup.ProjectId == obj.ProjectId).ToList();
                if (econsentSetup.Count > 0)
                {
                    econsentSetup.ForEach(e =>
                    {
                        var EconsentSectionReference = _context.EconsentSectionReference.Where(a => a.EconsentSetupId == e.EconsentSetupId).ToList();
                        if (EconsentSectionReference.Count > 0)
                            _context.EconsentSectionReference.RemoveRange(EconsentSectionReference);
                    });
                    _context.EconsentReviewDetails.RemoveRange(econsentSetup);
                }

                var econsentlist = _context.EconsentSetup.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (econsentlist.Count > 0)
                    _context.EconsentSetup.RemoveRange(econsentlist);
                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }
        public async Task<ProjectRemoveDataSuccess> AttendenceDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var Attendance = _context.Attendance.Include(x => x.AttendanceHistory).Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (Attendance.Count > 0)
                {
                    Attendance.ForEach(e =>
                    {
                        if (e.AttendanceHistory != null)
                            _context.AttendanceHistory.RemoveRange(e.AttendanceHistory);

                        var AttendanceBarcodeGenerate = _context.AttendanceBarcodeGenerate.Where(a => a.AttendanceId == e.Id).ToList();
                        if (AttendanceBarcodeGenerate.Count > 0)
                        {
                            _context.AttendanceBarcodeGenerate.RemoveRange(AttendanceBarcodeGenerate);

                        }
                        _context.Entry(e).State = EntityState.Detached;

                    });
                    _context.Attendance.RemoveRange(Attendance);
                }
                var AppScreenPatientRights = _context.AppScreenPatientRights.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (AppScreenPatientRights.Count > 0)
                    _context.AppScreenPatientRights.RemoveRange(AppScreenPatientRights);

                var UserSetting = _context.UserSetting.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (UserSetting.Count > 0)
                    _context.UserSetting.RemoveRange(UserSetting);

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }
        public async Task<ProjectRemoveDataSuccess> CTMSDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var CtmsMonitoring = _context.CtmsMonitoring.Where(x => x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (CtmsMonitoring.Count > 0)
                {
                    CtmsMonitoring.ForEach(e =>
                    {
                        var CtmsMonitoringReport = _context.CtmsMonitoringReport.Where(a => a.CtmsMonitoringId == e.Id).ToList();
                        CtmsMonitoringReport.ForEach(t =>
                        {
                            var CtmsMonitoringReportReview = _context.CtmsMonitoringReportReview.Where(x => x.CtmsMonitoringReportId == t.Id).ToList();
                            if (CtmsMonitoringReportReview.Count > 0)
                                _context.CtmsMonitoringReportReview.RemoveRange(CtmsMonitoringReportReview);


                            var CtmsMonitoringReportVariableValue = _context.CtmsMonitoringReportVariableValue.Where(x => x.CtmsMonitoringReportId == t.Id).ToList();
                            CtmsMonitoringReportVariableValue.ForEach(value =>
                            {
                                var CtmsMonitoringReportVariableValueAudit = _context.CtmsMonitoringReportVariableValueAudit.Where(z => z.CtmsMonitoringReportVariableValueId == value.Id).ToList();
                                if (CtmsMonitoringReportVariableValueAudit.Count > 0)
                                    _context.CtmsMonitoringReportVariableValueAudit.RemoveRange(CtmsMonitoringReportVariableValueAudit);

                                var CtmsMonitoringReportVariableValueChild = _context.CtmsMonitoringReportVariableValueChild.Where(z => z.CtmsMonitoringReportVariableValueId == value.Id).ToList();
                                if (CtmsMonitoringReportVariableValueChild.Count > 0)
                                    _context.CtmsMonitoringReportVariableValueChild.RemoveRange(CtmsMonitoringReportVariableValueChild);

                                var CtmsMonitoringReportVariableValueQuery = _context.CtmsMonitoringReportVariableValueQuery.Where(z => z.CtmsMonitoringReportVariableValueId == value.Id).ToList();
                                if (CtmsMonitoringReportVariableValueQuery.Count > 0)
                                    _context.CtmsMonitoringReportVariableValueQuery.RemoveRange(CtmsMonitoringReportVariableValueQuery);

                            });
                            if (CtmsMonitoringReportVariableValue != null)
                                _context.CtmsMonitoringReportVariableValue.RemoveRange(CtmsMonitoringReportVariableValue);
                        });
                        if (CtmsMonitoringReport != null)
                            _context.CtmsMonitoringReport.RemoveRange(CtmsMonitoringReport);
                    });
                    _context.CtmsMonitoring.RemoveRange(CtmsMonitoring);
                }

                var StudyPlan = _context.StudyPlan.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (StudyPlan.Count > 0)
                {
                    StudyPlan.ForEach(value =>
                    {
                        var StudyPlanTask = _context.StudyPlanTask.Where(z => z.StudyPlanId == value.Id).ToList();
                        if (StudyPlanTask.Count > 0)
                        {
                            StudyPlanTask.ForEach(valuetask =>
                            {
                                var StudyPlanTaskResource = _context.StudyPlanTaskResource.Where(z => z.StudyPlanTaskId == valuetask.Id).ToList();
                                if (StudyPlanTaskResource.Count > 0)
                                    _context.StudyPlanTaskResource.RemoveRange(StudyPlanTaskResource);

                            });
                            _context.StudyPlanTask.RemoveRange(StudyPlanTask);
                        }
                    });
                    _context.StudyPlan.RemoveRange(StudyPlan);
                }

                var studylevelform = _context.StudyLevelForm.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (studylevelform.Count > 0)
                {
                    studylevelform.ForEach(slf =>
                    {
                        var StudyLevelFormVariable = _context.StudyLevelFormVariable.Where(x => x.StudyLevelFormId == slf.Id).ToList();

                        StudyLevelFormVariable.ForEach(slfv =>
                        {
                            var StudyLevelFormVariableRemarks = _context.StudyLevelFormVariableRemarks.Where(x => x.StudyLevelFormVariableId == slfv.Id).ToList();
                            _context.StudyLevelFormVariableRemarks.RemoveRange(StudyLevelFormVariableRemarks);

                            var StudyLevelFormVariableValue = _context.StudyLevelFormVariableValue.Where(x => x.StudyLevelFormVariableId == slfv.Id).ToList();
                            _context.StudyLevelFormVariableValue.RemoveRange(StudyLevelFormVariableValue);
                        });
                        _context.StudyLevelFormVariable.RemoveRange(StudyLevelFormVariable);
                    });
                    _context.StudyLevelForm.RemoveRange(studylevelform);
                }

                var CtmsSettings = _context.CtmsSettings.Where(x => x.ProjectId == obj.ProjectId).ToList();
                _context.CtmsSettings.RemoveRange(CtmsSettings);

                var HolidayMaster = _context.HolidayMaster.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (HolidayMaster.Count > 0)
                    _context.HolidayMaster.RemoveRange(HolidayMaster);

                var WeekEndMaster = _context.WeekEndMaster.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (WeekEndMaster.Count > 0)
                    _context.WeekEndMaster.RemoveRange(WeekEndMaster);

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }
        public async Task<ProjectRemoveDataSuccess> LabManagementDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var LabManagementUploadData = _context.LabManagementUploadData
                    .Where(x => x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (LabManagementUploadData.Count > 0)
                {
                    LabManagementUploadData.ForEach(e =>
                    {
                        var LabManagementSendEmailUser = _context.LabManagementSendEmailUser.Where(a => a.LabManagementConfigurationId == e.LabManagementConfigurationId).ToList();
                        if (LabManagementSendEmailUser.Count > 0)
                            _context.LabManagementSendEmailUser.RemoveRange(LabManagementSendEmailUser);

                        var LabManagementVariableMapping = _context.LabManagementVariableMapping.Where(a => a.LabManagementConfigurationId == e.LabManagementConfigurationId).ToList();
                        if (LabManagementVariableMapping.Count > 0)
                            _context.LabManagementVariableMapping.RemoveRange(LabManagementVariableMapping);

                        var LabManagementUploadExcelData = _context.LabManagementUploadExcelData.Where(a => a.LabManagementUploadDataId == e.Id).ToList();
                        if (LabManagementUploadExcelData.Count > 0)
                            _context.LabManagementUploadExcelData.RemoveRange(LabManagementUploadExcelData);

                    });
                    _context.LabManagementUploadData.RemoveRange(LabManagementUploadData);
                    var LabManagementConfiguration = _context.LabManagementConfiguration
                        .Where(a => a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == obj.ProjectId).ToList();
                    if (LabManagementConfiguration.Count > 0)
                        _context.LabManagementConfiguration.RemoveRange(LabManagementConfiguration);


                    _context.Save();
                }
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> MedraDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var MeddraCoding = _context.MeddraCoding.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ParentProjectId == obj.ProjectId).ToList();
                if (MeddraCoding.Count > 0)
                {
                    MeddraCoding.ForEach(e =>
                    {
                        var MeddraCodingAudit = _context.MeddraCodingAudit.Where(a => a.MeddraCodingId == e.Id).ToList();
                        if (MeddraCodingAudit.Count > 0)
                            _context.MeddraCodingAudit.RemoveRange(MeddraCodingAudit);

                        var MeddraCodingComment = _context.MeddraCodingComment.Where(a => a.MeddraCodingId == e.Id).ToList();
                        if (MeddraCodingComment.Count > 0)
                            _context.MeddraCodingComment.RemoveRange(MeddraCodingComment);

                    });
                    _context.MeddraCoding.RemoveRange(MeddraCoding);
                }
                var StudyScoping = _context.StudyScoping.Where(a => a.ProjectId == obj.ProjectId).ToList();
                if (StudyScoping.Count > 0)
                    _context.StudyScoping.RemoveRange(StudyScoping);

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> ETMFDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var ProjectWorkplace = _context.ProjectWorkplace.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (ProjectWorkplace.Count > 0)
                {
                    ProjectWorkplace.ForEach(pw =>
                    {
                        pw.ProjectWorkplaceDetail.ForEach(pwd =>
                         {
                             pwd.ProjectWorkPlaceZone.ForEach(pwpz =>
                             {

                                 pwpz.ProjectWorkplaceSection.ForEach(pwsection =>
                                 {
                                     pwsection.ProjectWorkplaceArtificate.ForEach(pwa =>
                                     {

                                         pwa.ProjectWorkplaceArtificatedocument.ForEach(pwaart =>
                                         {
                                             _context.ProjectArtificateDocumentApprover.RemoveRange(pwaart.ProjectArtificateDocumentApprover);
                                             _context.ProjectArtificateDocumentComment.RemoveRange(pwaart.ProjectArtificateDocumentComment);
                                             _context.ProjectArtificateDocumentHistory.RemoveRange(pwaart.ProjectArtificateDocumentHistory);
                                             _context.ProjectArtificateDocumentReview.RemoveRange(pwaart.ProjectArtificateDocumentReview);

                                         });
                                         _context.ProjectWorkplaceArtificatedocument.RemoveRange(pwa.ProjectWorkplaceArtificatedocument);
                                     });
                                     _context.ProjectWorkplaceArtificate.RemoveRange(pwsection.ProjectWorkplaceArtificate);
                                     var ProjectWorkplaceSubSection = _context.ProjectWorkplaceSubSection.Where(a => a.ProjectWorkplaceSectionId == pwsection.Id).ToList();
                                     if (ProjectWorkplaceSubSection.Count > 0)
                                     {
                                         ProjectWorkplaceSubSection.ForEach(PWsubsection =>
                                         {
                                             PWsubsection.ProjectWorkplaceSubSectionArtifact.ForEach(PWsubsectionart =>
                                             {
                                                 PWsubsectionart.ProjectWorkplaceSubSecArtificatedocument.ForEach(PWsubsectionartDoc =>
                                                 {
                                                     _context.ProjectSubSecArtificateDocumentReview.RemoveRange(PWsubsectionartDoc.ProjectSubSecArtificateDocumentReview);
                                                     _context.ProjectSubSecArtificateDocumentComment.RemoveRange(PWsubsectionartDoc.ProjectSubSecArtificateDocumentComment);
                                                     _context.ProjectSubSecArtificateDocumentApprover.RemoveRange(PWsubsectionartDoc.ProjectSubSecArtificateDocumentApprover);
                                                     _context.ProjectSubSecArtificateDocumentHistory.RemoveRange(PWsubsectionartDoc.ProjectSubSecArtificateDocumentHistory);
                                                 });
                                                 _context.ProjectWorkplaceSubSecArtificatedocument.RemoveRange(PWsubsectionart.ProjectWorkplaceSubSecArtificatedocument);

                                             });
                                             _context.ProjectWorkplaceSubSectionArtifact.RemoveRange(PWsubsection.ProjectWorkplaceSubSectionArtifact);

                                         });
                                         _context.ProjectWorkplaceSubSection.RemoveRange(ProjectWorkplaceSubSection);
                                     }
                                 });
                                 _context.ProjectWorkplaceSection.RemoveRange(pwpz.ProjectWorkplaceSection);

                             });
                             _context.ProjectWorkPlaceZone.RemoveRange(pwd.ProjectWorkPlaceZone);

                         });
                        _context.ProjectWorkplaceDetail.RemoveRange(pw.ProjectWorkplaceDetail);
                    });
                    _context.ProjectWorkplace.RemoveRange(ProjectWorkplace);
                }

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> ScreeningDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var Randomization = _context.Randomization.Where(x => x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (Randomization.Count > 0)
                {
                    Randomization.ForEach(rand =>
                    {
                        var ScreeningEntry = _context.ScreeningEntry.Where(x => x.RandomizationId == rand.Id).ToList();
                        if (ScreeningEntry.Count > 0)
                        {
                            ScreeningEntry.ForEach(sentry =>
                            {
                                if (sentry.ScreeningVisit != null && sentry.ScreeningVisit.ToList().Count > 0)
                                {
                                    sentry.ScreeningVisit.ToList().ForEach(svisit =>
                                    {
                                        svisit.ScreeningTemplates.ForEach(stemplate =>
                                        {
                                            _context.ScreeningTemplateReview.RemoveRange(stemplate.ScreeningTemplateReview);

                                            var ScreeningTemplateEditCheckValue = _context.ScreeningTemplateEditCheckValue.Where(x => x.ScreeningTemplateId == stemplate.Id).ToList();
                                            if (ScreeningTemplateEditCheckValue.Count > 0)
                                                _context.ScreeningTemplateEditCheckValue.RemoveRange(ScreeningTemplateEditCheckValue);

                                            stemplate.ScreeningTemplateValues.ToList().ForEach(stemplatevalue =>
                                            {
                                                _context.ScreeningTemplateValueAudit.RemoveRange(stemplatevalue.ScreeningTemplateValueAudits);
                                                _context.ScreeningTemplateValueChild.RemoveRange(stemplatevalue.Children);
                                                _context.ScreeningTemplateValueQuery.RemoveRange(stemplatevalue.ScreeningTemplateValueQuerys);
                                                _context.ScreeningTemplateValueComment.RemoveRange(stemplatevalue.Comments.ToList());
                                                var ScreeningTemplateRemarksChild = _context.ScreeningTemplateRemarksChild.Where(z => z.ScreeningTemplateValueId == stemplatevalue.Id).ToList();
                                                if (ScreeningTemplateRemarksChild.Count > 0)
                                                    _context.ScreeningTemplateRemarksChild.RemoveRange(ScreeningTemplateRemarksChild);
                                            });
                                            _context.ScreeningTemplateValue.RemoveRange(stemplate.ScreeningTemplateValues);
                                        });
                                        _context.ScreeningTemplate.RemoveRange(svisit.ScreeningTemplates);
                                    });
                                    _context.ScreeningVisit.RemoveRange(sentry.ScreeningVisit.ToList());
                                }
                                _context.ScreeningHistory.RemoveRange(sentry.ScreeningHistory);
                                var ScreeningTemplateLockUnlockAudit = _context.ScreeningTemplateLockUnlockAudit.Where(x => x.ScreeningEntryId == sentry.Id).ToList();
                                if (ScreeningTemplateLockUnlockAudit.Count > 0)
                                {
                                    _context.ScreeningTemplateLockUnlockAudit.RemoveRange(ScreeningTemplateLockUnlockAudit);
                                }
                            });
                            _context.ScreeningEntry.RemoveRange(ScreeningEntry);

                        }
                    });

                    _context.Save();
                }
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> DesignDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var project = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                if (project != null)
                {
                    if (project.ProjectDesigns.Count > 0)
                    {
                        project.ProjectDesigns.ToList().ForEach(design =>
                        {
                            design.ProjectDesignPeriods.ToList().ForEach(designperiod =>
                            {
                                designperiod.VisitList.ToList().ForEach(visitList =>
                                {
                                    visitList.Templates.ToList().ForEach(templates =>
                                    {
                                        templates.Variables.ToList().ForEach(variables =>
                                        {
                                            variables.Values.ToList().ForEach(values =>
                                            {
                                                _context.VariableValueLanguage.RemoveRange(values.VariableValueLanguage);

                                            });
                                            _context.ProjectDesignVariableValue.RemoveRange(variables.Values);
                                            _context.VariableLanguage.RemoveRange(variables.VariableLanguage);
                                            _context.VariableNoteLanguage.RemoveRange(variables.VariableNoteLanguage);
                                            _context.ProjectDesignVariableEncryptRole.RemoveRange(variables.Roles);

                                            var remarks = _context.ProjectDesignVariableRemarks.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            _context.ProjectDesignVariableRemarks.RemoveRange(remarks);
                                        });
                                        _context.ProjectDesignVariable.RemoveRange(templates.Variables);
                                        _context.TemplateLanguage.RemoveRange(templates.TemplateLanguage);
                                        _context.ProjectDesignTemplateNote.RemoveRange(templates.ProjectDesignTemplateNote);
                                        _context.ProjectDesingTemplateRestriction.RemoveRange(templates.ProjectDesingTemplateRestriction);
                                    });
                                    _context.ProjectDesignTemplate.RemoveRange(visitList.Templates);
                                    _context.ProjectDesignVisitStatus.RemoveRange(visitList.ProjectDesignVisitStatus);
                                });
                                _context.ProjectDesignVisit.RemoveRange(designperiod.VisitList);

                            });

                            _context.ProjectDesignPeriod.RemoveRange(design.ProjectDesignPeriods);

                            if (design.StudyVersions.Count > 0)
                            {
                                design.StudyVersions.ForEach(version =>
                                {
                                    _context.StudyVersionStatus.RemoveRange(version.StudyVersionStatus);
                                });
                                _context.StudyVersion.RemoveRange(design.StudyVersions);
                            }
                            var ProjectSchedule = _context.ProjectSchedule.Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (ProjectSchedule.Count > 0)
                            {
                                ProjectSchedule.ForEach(pschedule =>
                                {
                                    _context.ProjectScheduleTemplate.RemoveRange(pschedule.Templates);
                                });
                                _context.ProjectSchedule.RemoveRange(ProjectSchedule);
                            }
                            var editCheck = _context.EditCheck.Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (editCheck.Count > 0)
                            {
                                editCheck.ForEach(editcheck =>
                                {
                                    _context.EditCheckDetail.RemoveRange(editcheck.EditCheckDetails.ToList());
                                });
                                _context.EditCheck.RemoveRange(editCheck);
                            }
                            var pworkflow = _context.ProjectWorkflow.Where(x => x.ProjectDesignId == design.Id).ToList();
                            if (pworkflow.Count > 0)
                            {
                                pworkflow.ForEach(pflow =>
                                {
                                    _context.ProjectWorkflowLevel.RemoveRange(pflow.Levels);
                                    _context.ProjectWorkflowIndependent.RemoveRange(pflow.Independents);
                                });
                                _context.ProjectWorkflow.RemoveRange(pworkflow);
                            }
                            var pdreportsetting = _context.ProjectDesignReportSetting.Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (pdreportsetting.Count > 0)
                            {
                                _context.ProjectDesignReportSetting.RemoveRange(pdreportsetting);
                            }
                        });
                        _context.ProjectDesign.RemoveRange(project.ProjectDesigns);
                    }


                    var randomizationNumberSettings = _context.RandomizationNumberSettings.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                    || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (randomizationNumberSettings.Count > 0)
                        _context.RandomizationNumberSettings.RemoveRange(randomizationNumberSettings);

                    var screeningNumberSettings = _context.ScreeningNumberSettings.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                   || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (screeningNumberSettings.Count > 0)
                        _context.ScreeningNumberSettings.RemoveRange(screeningNumberSettings);

                    var uploadLimit = _context.UploadLimit.Where(x => x.ProjectId == obj.ProjectId).ToList();
                    if (uploadLimit.Count > 0)
                        _context.UploadLimit.RemoveRange(uploadLimit);

                    if (project.ProjectRight.Count > 0)
                        _context.ProjectRight.RemoveRange(project.ProjectRight);

                    var SiteTeam = _context.SiteTeam.Where(x => project.ChildProject.Select(z => z.Id).Contains(x.ProjectId)).ToList();
                    if (SiteTeam.Count > 0)
                        _context.SiteTeam.RemoveRange(SiteTeam);

                    var ProjectDocument = _context.ProjectDocument.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                   || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (ProjectDocument.Count > 0)
                    {
                        ProjectDocument.ForEach(projdata =>
                        {
                            var ProjectDocumentReview = _context.ProjectDocumentReview.Where(x => x.ProjectDocumentId == projdata.Id).ToList();
                            _context.ProjectDocumentReview.RemoveRange(ProjectDocumentReview);
                        });
                        _context.ProjectDocument.RemoveRange(ProjectDocument);
                    }
                    _context.Project.RemoveRange(project.ChildProject);
                    _context.Project.RemoveRange(project);
                }


                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                return finaldata;
            }
        }
    }
}
