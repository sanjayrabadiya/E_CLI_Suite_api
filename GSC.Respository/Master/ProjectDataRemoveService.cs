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
        private readonly IGSCContext _context;
        public ProjectDataRemoveService(IGSCContext context)
        {
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
                            if (aeReportingValue.Any())
                                _context.AEReportingValue.RemoveRange(aeReportingValue);
                        });
                        if (aereporting.Any())
                            _context.AEReporting.RemoveRange(aereporting);

                        var adversedetails = _context.AdverseEventSettingsDetails.Where(s => s.AdverseEventSettingsId == x.Id).ToList();
                        if (adversedetails.Any())
                            _context.AdverseEventSettingsDetails.RemoveRange(adversedetails);

                    });
                    if (data.Any())
                        _context.AdverseEventSettings.RemoveRange(data);
                    _context.Save();
                }
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
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
                finaldata.Message = ex.Message.ToString();
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
                var childproject = _context.Project.Where(x => x.ParentProjectId == obj.ProjectId).ToList();
                if (childproject.Any())
                {
                    childproject.ForEach(z =>
                    {
                        var AppScreenPatientRights = _context.AppScreenPatientRights.Where(x => x.ProjectId == z.Id).ToList();
                        if (AppScreenPatientRights.Count > 0)
                            _context.AppScreenPatientRights.RemoveRange(AppScreenPatientRights);
                    });
                }
                var AppScreenPatientRights = _context.AppScreenPatientRights.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (AppScreenPatientRights.Count > 0)
                    _context.AppScreenPatientRights.RemoveRange(AppScreenPatientRights);

                var UserSetting = _context.UserSetting.Where(x => x.ProjectId == obj.ProjectId || x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (UserSetting.Count > 0)
                    _context.UserSetting.RemoveRange(UserSetting);

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
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
                            if (CtmsMonitoringReportVariableValue.Any())
                                _context.CtmsMonitoringReportVariableValue.RemoveRange(CtmsMonitoringReportVariableValue);
                        });
                        if (CtmsMonitoringReport.Any())
                            _context.CtmsMonitoringReport.RemoveRange(CtmsMonitoringReport);
                    });
                    _context.CtmsMonitoring.RemoveRange(CtmsMonitoring);
                }

                var StudyPlan = _context.StudyPlan.Where(x => x.ProjectId == obj.ProjectId || x.Project.ParentProjectId == obj.ProjectId).ToList();
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
                        if (StudyLevelFormVariable.Any())
                        {
                            StudyLevelFormVariable.ForEach(slfv =>
                            {
                                var StudyLevelFormVariableRemarks = _context.StudyLevelFormVariableRemarks.Where(x => x.StudyLevelFormVariableId == slfv.Id).ToList();
                                if (StudyLevelFormVariableRemarks.Any())
                                    _context.StudyLevelFormVariableRemarks.RemoveRange(StudyLevelFormVariableRemarks);

                                var StudyLevelFormVariableValue = _context.StudyLevelFormVariableValue.Where(x => x.StudyLevelFormVariableId == slfv.Id).ToList();
                                if (StudyLevelFormVariableValue.Any())
                                    _context.StudyLevelFormVariableValue.RemoveRange(StudyLevelFormVariableValue);
                            });
                            if (StudyLevelFormVariable.Any())
                                _context.StudyLevelFormVariable.RemoveRange(StudyLevelFormVariable);
                        }
                    });
                    if (studylevelform.Any())
                        _context.StudyLevelForm.RemoveRange(studylevelform);
                }

                var ProjectSettings = _context.ProjectSettings.Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (ProjectSettings.Count > 0)
                    _context.ProjectSettings.RemoveRange(ProjectSettings);

                var HolidayMaster = _context.HolidayMaster.Where(x => x.ProjectId == obj.ProjectId || x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (HolidayMaster.Count > 0)
                    _context.HolidayMaster.RemoveRange(HolidayMaster);

                var WeekEndMaster = _context.WeekEndMaster.Where(x => x.ProjectId == obj.ProjectId || x.Project.ParentProjectId == obj.ProjectId).ToList();
                if (WeekEndMaster.Count > 0)
                    _context.WeekEndMaster.RemoveRange(WeekEndMaster);

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
                return finaldata;
            }
        }
        public async Task<ProjectRemoveDataSuccess> LabManagementDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var LabManagementConfiguration = _context.LabManagementConfiguration
                       .Where(a => a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == obj.ProjectId).ToList();
                if (LabManagementConfiguration.Count > 0)
                {
                    LabManagementConfiguration.ForEach(e =>
                    {
                        var LabManagementUploadData = _context.LabManagementUploadData.Include(x => x.LabManagementUploadExcelDatas).Where(a => a.LabManagementConfigurationId == e.Id).ToList();
                        if (LabManagementUploadData.Count > 0)
                        {
                            LabManagementUploadData.ForEach(z =>
                            {
                                _context.LabManagementUploadExcelData.RemoveRange(z.LabManagementUploadExcelDatas);
                            });
                            _context.LabManagementUploadData.RemoveRange(LabManagementUploadData);
                        }
                        var LabManagementSendEmailUser = _context.LabManagementSendEmailUser.Where(a => a.LabManagementConfigurationId == e.Id).ToList();
                        if (LabManagementSendEmailUser.Count > 0)
                            _context.LabManagementSendEmailUser.RemoveRange(LabManagementSendEmailUser);

                        var LabManagementVariableMapping = _context.LabManagementVariableMapping.Where(a => a.LabManagementConfigurationId == e.Id).ToList();
                        if (LabManagementVariableMapping.Count > 0)
                            _context.LabManagementVariableMapping.RemoveRange(LabManagementVariableMapping);

                    });
                    _context.LabManagementConfiguration.RemoveRange(LabManagementConfiguration);
                }

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
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
                finaldata.Message = ex.Message.ToString();
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> ETMFDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var EtmfProjectWorkPlace = _context.EtmfProjectWorkPlace
                    .Include(x => x.ProjectWorkPlace)
                    .ThenInclude(x => x.ProjectWorkplaceArtificatedocument)
                    .ThenInclude(x => x.ProjectArtificateDocumentApprover)

                    .Where(x => x.ProjectId == obj.ProjectId).ToList();
                if (EtmfProjectWorkPlace.Count > 0)
                {
                    EtmfProjectWorkPlace.ForEach(pw =>
                    {
                        pw.ProjectWorkplaceDetails.ToList().ForEach(pwd =>
                         {
                             pwd.ProjectWorkplaceDetails.ToList().ForEach(pwpz =>
                             {

                                 pwpz.ProjectWorkplaceDetails.ToList().ForEach(pwsection =>
                                 {
                                     pwsection.ProjectWorkplaceDetails.ToList().ForEach(pwa =>
                                     {

                                         pwa.ProjectWorkplaceArtificatedocument.ForEach(pwaart =>
                                         {
                                             if (pwaart.ProjectArtificateDocumentApprover != null)
                                                 _context.ProjectArtificateDocumentApprover.RemoveRange(pwaart.ProjectArtificateDocumentApprover);

                                             var ProjectArtificateDocumentComment = _context.ProjectArtificateDocumentComment.Where(x => x.ProjectWorkplaceArtificatedDocumentId == pwaart.Id).ToList();
                                             if (ProjectArtificateDocumentComment.Any())
                                                 _context.ProjectArtificateDocumentComment.RemoveRange(ProjectArtificateDocumentComment);
                                             var ProjectArtificateDocumentHistory = _context.ProjectArtificateDocumentHistory.Where(x => x.ProjectWorkplaceArtificateDocumentId == pwaart.Id).ToList();
                                             if (ProjectArtificateDocumentHistory.Any())
                                                 _context.ProjectArtificateDocumentHistory.RemoveRange(ProjectArtificateDocumentHistory);
                                             var ProjectArtificateDocumentReview = _context.ProjectArtificateDocumentReview.Where(x => x.ProjectWorkplaceArtificatedDocumentId == pwaart.Id).ToList();
                                             if (ProjectArtificateDocumentReview.Any())
                                                 _context.ProjectArtificateDocumentReview.RemoveRange(ProjectArtificateDocumentReview);

                                         });
                                         if (pwa.ProjectWorkplaceArtificatedocument.Any())
                                             _context.ProjectWorkplaceArtificatedocument.RemoveRange(pwa.ProjectWorkplaceArtificatedocument);
                                     });
                                     if (pwsection.ProjectWorkplaceDetails != null)
                                         _context.EtmfProjectWorkPlace.RemoveRange(pwsection.ProjectWorkplaceDetails);

                                     var ProjectWorkplaceSubSection = _context.EtmfProjectWorkPlace
                                     .Include(x => x.ProjectWorkPlace)
                                     .ThenInclude(x => x.ProjectWorkplaceSubSecArtificatedocument)
                                     .ThenInclude(x => x.ProjectSubSecArtificateDocumentReview)
                                     .Where(a => a.EtmfProjectWorkPlaceId == pwsection.Id).ToList();

                                     if (ProjectWorkplaceSubSection.Any())
                                     {
                                         ProjectWorkplaceSubSection.ForEach(PWsubsection =>
                                         {
                                             PWsubsection.ProjectWorkplaceDetails.ToList().ForEach(PWsubsectionart =>
                                             {
                                                 PWsubsectionart.ProjectWorkplaceSubSecArtificatedocument.ForEach(PWsubsectionartDoc =>
                                                 {
                                                     if (PWsubsectionartDoc.ProjectSubSecArtificateDocumentReview != null)
                                                         _context.ProjectSubSecArtificateDocumentReview.RemoveRange(PWsubsectionartDoc.ProjectSubSecArtificateDocumentReview);

                                                     var ProjectSubSecArtificateDocumentComment = _context.ProjectSubSecArtificateDocumentComment.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == PWsubsectionartDoc.Id).ToList();
                                                     if (ProjectSubSecArtificateDocumentComment.Any())
                                                         _context.ProjectSubSecArtificateDocumentComment.RemoveRange(ProjectSubSecArtificateDocumentComment);
                                                     var ProjectSubSecArtificateDocumentApprover = _context.ProjectSubSecArtificateDocumentApprover.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == PWsubsectionartDoc.Id).ToList();
                                                     if (ProjectSubSecArtificateDocumentApprover.Any())
                                                         _context.ProjectSubSecArtificateDocumentApprover.RemoveRange(ProjectSubSecArtificateDocumentApprover);
                                                     var ProjectSubSecArtificateDocumentHistory = _context.ProjectSubSecArtificateDocumentHistory.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == PWsubsectionartDoc.Id).ToList();
                                                     if (ProjectSubSecArtificateDocumentHistory.Any())
                                                         _context.ProjectSubSecArtificateDocumentHistory.RemoveRange(ProjectSubSecArtificateDocumentHistory);
                                                 });
                                                 if (PWsubsectionart.ProjectWorkplaceSubSecArtificatedocument != null)
                                                     _context.ProjectWorkplaceSubSecArtificatedocument.RemoveRange(PWsubsectionart.ProjectWorkplaceSubSecArtificatedocument);

                                             });
                                             if (PWsubsection.ProjectWorkplaceDetails != null)
                                                 _context.EtmfProjectWorkPlace.RemoveRange(PWsubsection.ProjectWorkplaceDetails);

                                         });

                                         _context.EtmfProjectWorkPlace.RemoveRange(ProjectWorkplaceSubSection);
                                     }
                                 });
                                 if (pwpz.ProjectWorkplaceDetails != null)
                                     _context.EtmfProjectWorkPlace.RemoveRange(pwpz.ProjectWorkplaceDetails);

                             });
                             if (pwd.ProjectWorkplaceDetails != null)
                                 _context.EtmfProjectWorkPlace.RemoveRange(pwd.ProjectWorkplaceDetails);

                             var EtmfUserPermission = _context.EtmfUserPermission.Where(x => x.ProjectWorkplaceDetailId == pwd.Id).ToList();
                             if (EtmfUserPermission.Any())
                                 _context.EtmfUserPermission.RemoveRange(EtmfUserPermission);
                         });
                        if (pw.ProjectWorkplaceDetails != null)
                            _context.EtmfProjectWorkPlace.RemoveRange(pw.ProjectWorkplaceDetails);
                    });
                    if (EtmfProjectWorkPlace.Any())
                        _context.EtmfProjectWorkPlace.RemoveRange(EtmfProjectWorkPlace);
                }

                _context.Save();
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
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
                        var ScreeningEntry = _context.ScreeningEntry
                        .Include(x => x.ScreeningHistory)
                        .Include(x => x.ScreeningVisit)
                        .ThenInclude(x => x.ScreeningTemplates)
                        .ThenInclude(x => x.ScreeningTemplateValues)
                        .ThenInclude(x => x.ScreeningTemplateValueAudits)
                        .Where(x => x.RandomizationId == rand.Id).ToList();
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
                                            var ScreeningTemplateReview = _context.ScreeningTemplateReview.Where(x => x.ScreeningTemplateId == stemplate.Id).ToList();
                                            if (ScreeningTemplateReview.Any() && ScreeningTemplateReview.Count > 0)
                                                _context.ScreeningTemplateReview.RemoveRange(ScreeningTemplateReview);

                                            var ScreeningTemplateEditCheckValue = _context.ScreeningTemplateEditCheckValue.Where(x => x.ScreeningTemplateId == stemplate.Id).ToList();
                                            if (ScreeningTemplateEditCheckValue.Any() && ScreeningTemplateEditCheckValue.Count > 0)
                                                _context.ScreeningTemplateEditCheckValue.RemoveRange(ScreeningTemplateEditCheckValue);

                                            stemplate.ScreeningTemplateValues.ToList().ForEach(stemplatevalue =>
                                            {
                                                if (stemplatevalue.ScreeningTemplateValueAudits != null && stemplatevalue.ScreeningTemplateValueAudits.Count > 0)
                                                    _context.ScreeningTemplateValueAudit.RemoveRange(stemplatevalue.ScreeningTemplateValueAudits);

                                                var ScreeningTemplateValueChild = _context.ScreeningTemplateValueChild.Where(z => z.ScreeningTemplateValueId == stemplatevalue.Id).ToList();
                                                if (ScreeningTemplateValueChild.Any() && ScreeningTemplateValueChild.Count > 0)
                                                    _context.ScreeningTemplateValueChild.RemoveRange(ScreeningTemplateValueChild);

                                                var ScreeningTemplateValueQuerys = _context.ScreeningTemplateValueQuery.Where(z => z.ScreeningTemplateValueId == stemplatevalue.Id).ToList();
                                                if (ScreeningTemplateValueQuerys.Any() && ScreeningTemplateValueQuerys.Count > 0)
                                                    _context.ScreeningTemplateValueQuery.RemoveRange(ScreeningTemplateValueQuerys);

                                                var ScreeningTemplateValueComment = _context.ScreeningTemplateValueComment.Where(z => z.ScreeningTemplateValueId == stemplatevalue.Id).ToList();
                                                if (ScreeningTemplateValueComment.Any() && ScreeningTemplateValueComment.Count > 0)
                                                    _context.ScreeningTemplateValueComment.RemoveRange(ScreeningTemplateValueComment);

                                                var ScreeningTemplateRemarksChild = _context.ScreeningTemplateRemarksChild.Where(z => z.ScreeningTemplateValueId == stemplatevalue.Id).ToList();
                                                if (ScreeningTemplateRemarksChild.Any() && ScreeningTemplateRemarksChild.Count > 0)
                                                    _context.ScreeningTemplateRemarksChild.RemoveRange(ScreeningTemplateRemarksChild);
                                            });
                                            if (stemplate.ScreeningTemplateValues != null && stemplate.ScreeningTemplateValues.Count > 0)
                                                _context.ScreeningTemplateValue.RemoveRange(stemplate.ScreeningTemplateValues);
                                        });
                                        if (svisit.ScreeningTemplates != null && svisit.ScreeningTemplates.Count > 0)
                                            _context.ScreeningTemplate.RemoveRange(svisit.ScreeningTemplates);

                                        var ScreeningVisitHistory = _context.ScreeningVisitHistory.Where(x => x.ScreeningVisitId == svisit.Id).ToList();
                                        if (ScreeningVisitHistory.Any() && ScreeningVisitHistory.Count > 0)
                                            _context.ScreeningVisitHistory.RemoveRange(ScreeningVisitHistory);

                                    });
                                    if (sentry.ScreeningVisit != null && sentry.ScreeningVisit.ToList().Count > 0)
                                        _context.ScreeningVisit.RemoveRange(sentry.ScreeningVisit.ToList());
                                }
                                if (sentry.ScreeningHistory != null)
                                    _context.ScreeningHistory.RemoveRange(sentry.ScreeningHistory);
                                var ScreeningTemplateLockUnlockAudit = _context.ScreeningTemplateLockUnlockAudit.Where(x => x.ScreeningEntryId == sentry.Id).ToList();
                                if (ScreeningTemplateLockUnlockAudit.Any() && ScreeningTemplateLockUnlockAudit.Count > 0)
                                {
                                    _context.ScreeningTemplateLockUnlockAudit.RemoveRange(ScreeningTemplateLockUnlockAudit);
                                }
                            });
                            if (ScreeningEntry.Any())
                                _context.ScreeningEntry.RemoveRange(ScreeningEntry);

                        }
                    });
                    _context.Randomization.RemoveRange(Randomization);

                    _context.Save();
                }
                finaldata.IsSuccess = true;
                return finaldata;
            }
            catch (Exception ex)
            {
                finaldata.IsSuccess = false;
                finaldata.Message = ex.Message.ToString();
                return finaldata;
            }
        }

        public async Task<ProjectRemoveDataSuccess> DesignDataRemove(ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess finaldata = new ProjectRemoveDataSuccess();
            try
            {
                var project = _context.Project
                    .Include(x => x.ProjectRight)
                    .Include(x => x.ChildProject)
                    .Include(x => x.ProjectDesigns)
                    .ThenInclude(x => x.ProjectDesignPeriods)
                    .ThenInclude(x => x.VisitList)
                    .ThenInclude(x => x.Templates)
                    .ThenInclude(x => x.Variables)
                    .ThenInclude(x => x.Values)
                    .ThenInclude(x => x.VariableValueLanguage)
                    .Include(x => x.ChildProject)
                    .ThenInclude(x => x.ProjectRight)
                    .Where(x => x.Id == obj.ProjectId).FirstOrDefault();
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
                                                if (values.VariableValueLanguage != null && values.VariableValueLanguage.Count > 0)
                                                    _context.VariableValueLanguage.RemoveRange(values.VariableValueLanguage);

                                            });
                                            if (variables.Values != null && variables.Values.Count > 0)
                                                _context.ProjectDesignVariableValue.RemoveRange(variables.Values);

                                            var ProjectDesignVariableEncryptRole = _context.ProjectDesignVariableEncryptRole.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            if (ProjectDesignVariableEncryptRole.Any() && ProjectDesignVariableEncryptRole.Count > 0)
                                                _context.ProjectDesignVariableEncryptRole.RemoveRange(ProjectDesignVariableEncryptRole);

                                            var VariableNoteLanguage = _context.VariableNoteLanguage.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            if (VariableNoteLanguage.Any() && VariableNoteLanguage.Count > 0)
                                                _context.VariableNoteLanguage.RemoveRange(VariableNoteLanguage);

                                            var VariableLanguage = _context.VariableLanguage.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            if (VariableLanguage.Any() && VariableLanguage.Count > 0)
                                                _context.VariableLanguage.RemoveRange(VariableLanguage);

                                            var remarks = _context.ProjectDesignVariableRemarks.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            if (remarks.Any() && remarks.Count > 0)
                                                _context.ProjectDesignVariableRemarks.RemoveRange(remarks);

                                            var ProjectDesignVisitStatus = _context.ProjectDesignVisitStatus.Where(z => z.ProjectDesignVariableId == variables.Id).ToList();
                                            if (ProjectDesignVisitStatus.Any() && ProjectDesignVisitStatus.Count > 0)
                                                _context.ProjectDesignVisitStatus.RemoveRange(ProjectDesignVisitStatus);
                                        });
                                        if (templates.Variables != null && templates.Variables.Count > 0)
                                            _context.ProjectDesignVariable.RemoveRange(templates.Variables);

                                        var TemplateLanguage = _context.TemplateLanguage.Where(x => x.ProjectDesignTemplateId == templates.Id).ToList();
                                        if (TemplateLanguage.Any() && TemplateLanguage.Count > 0)
                                            _context.TemplateLanguage.RemoveRange(TemplateLanguage);

                                        var TemplateNoteLanguage = _context.TemplateNoteLanguage.Where(x => x.ProjectDesignTemplateNote.ProjectDesignTemplateId == templates.Id).ToList();
                                        if (TemplateNoteLanguage.Any() && TemplateNoteLanguage.Count > 0)
                                            _context.TemplateNoteLanguage.RemoveRange(TemplateNoteLanguage);

                                        var ProjectDesignTemplateNote = _context.ProjectDesignTemplateNote.Where(x => x.ProjectDesignTemplateId == templates.Id).ToList();
                                        if (ProjectDesignTemplateNote.Any() && ProjectDesignTemplateNote.Count > 0)
                                            _context.ProjectDesignTemplateNote.RemoveRange(ProjectDesignTemplateNote);

                                        var ProjectDesingTemplateRestriction = _context.ProjectDesingTemplateRestriction.Where(x => x.ProjectDesignTemplateId == templates.Id).ToList();
                                        if (ProjectDesingTemplateRestriction.Any() && ProjectDesingTemplateRestriction.Count > 0)
                                            _context.ProjectDesingTemplateRestriction.RemoveRange(ProjectDesingTemplateRestriction);


                                    });
                                    if (visitList.Templates != null && visitList.Templates.Count > 0)
                                        _context.ProjectDesignTemplate.RemoveRange(visitList.Templates);

                                    var VisitLanguage = _context.VisitLanguage.Where(x => x.ProjectDesignVisitId == visitList.Id).ToList();
                                    if (VisitLanguage.Any() && VisitLanguage.Count > 0)
                                        _context.VisitLanguage.RemoveRange(VisitLanguage);
                                });
                                if (designperiod.VisitList != null && designperiod.VisitList.Count > 0)
                                    _context.ProjectDesignVisit.RemoveRange(designperiod.VisitList);

                            });
                            if (design.ProjectDesignPeriods != null && design.ProjectDesignPeriods.Count > 0)
                                _context.ProjectDesignPeriod.RemoveRange(design.ProjectDesignPeriods);

                            var StudyVersions = _context.StudyVersion.Include(x => x.StudyVersionStatus).Where(x => x.ProjectDesignId == design.Id).ToList();
                            if (StudyVersions.Any() && StudyVersions.Count > 0)
                            {
                                StudyVersions.ForEach(version =>
                                {
                                    if (version.StudyVersionStatus != null && version.StudyVersionStatus.Count > 0)
                                        _context.StudyVersionStatus.RemoveRange(version.StudyVersionStatus);
                                });
                                _context.StudyVersion.RemoveRange(StudyVersions);
                            }
                            var ProjectSchedule = _context.ProjectSchedule.Include(x => x.Templates).Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (ProjectSchedule.Any() && ProjectSchedule.Count > 0)
                            {
                                ProjectSchedule.ForEach(pschedule =>
                                {
                                    if (pschedule.Templates != null && pschedule.Templates.Count > 0)
                                        _context.ProjectScheduleTemplate.RemoveRange(pschedule.Templates);
                                });
                                _context.ProjectSchedule.RemoveRange(ProjectSchedule);
                            }
                            var editCheck = _context.EditCheck.Include(x => x.EditCheckDetails).Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (editCheck.Any() && editCheck.Count > 0)
                            {
                                editCheck.ForEach(editcheck =>
                                {
                                    if (editcheck != null && editcheck.EditCheckDetails.ToList().Count > 0)
                                        _context.EditCheckDetail.RemoveRange(editcheck.EditCheckDetails.ToList());
                                });
                                _context.EditCheck.RemoveRange(editCheck);
                            }
                            var pworkflow = _context.ProjectWorkflow.Include(x => x.Levels).Include(x => x.Independents).Where(x => x.ProjectDesignId == design.Id).ToList();
                            if (pworkflow.Any() && pworkflow.Count > 0)
                            {
                                pworkflow.ForEach(pflow =>
                                {
                                    if (pflow.Levels != null && pflow.Levels.Count > 0)
                                        _context.ProjectWorkflowLevel.RemoveRange(pflow.Levels);
                                    if (pflow.Independents != null && pflow.Independents.Count > 0)
                                        _context.ProjectWorkflowIndependent.RemoveRange(pflow.Independents);
                                });
                                _context.ProjectWorkflow.RemoveRange(pworkflow);
                            }
                            var pdreportsetting = _context.ProjectDesignReportSetting.Where(x => x.ProjectDesignId == design.Id).ToList();

                            if (pdreportsetting.Any() && pdreportsetting.Count > 0)
                            {
                                _context.ProjectDesignReportSetting.RemoveRange(pdreportsetting);
                            }
                        });
                        if (project.ProjectDesigns != null && project.ProjectDesigns.Count > 0)
                            _context.ProjectDesign.RemoveRange(project.ProjectDesigns);
                    }


                    var randomizationNumberSettings = _context.RandomizationNumberSettings.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                    || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (randomizationNumberSettings.Any() && randomizationNumberSettings.Count > 0)
                        _context.RandomizationNumberSettings.RemoveRange(randomizationNumberSettings);

                    var screeningNumberSettings = _context.ScreeningNumberSettings.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                   || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (screeningNumberSettings.Any() && screeningNumberSettings.Count > 0)
                        _context.ScreeningNumberSettings.RemoveRange(screeningNumberSettings);

                    var uploadLimit = _context.UploadLimit.Where(x => x.ProjectId == obj.ProjectId).ToList();
                    if (uploadLimit.Any() && uploadLimit.Count > 0)
                        _context.UploadLimit.RemoveRange(uploadLimit);

                    if (project.ProjectRight != null && project.ProjectRight.Count > 0)
                        _context.ProjectRight.RemoveRange(project.ProjectRight);

                    var SiteTeam = _context.SiteTeam.Where(x => project.ChildProject.Select(z => z.Id).Contains(x.ProjectId)).ToList();
                    if (SiteTeam.Any() && SiteTeam.Count > 0)
                        _context.SiteTeam.RemoveRange(SiteTeam);

                    var ProjectDocument = _context.ProjectDocument.Where(x => (x.Project.ParentProjectId == null && x.ProjectId == obj.ProjectId)
                   || (x.Project.ParentProjectId != null && x.Project.ParentProjectId == obj.ProjectId)).ToList();
                    if (ProjectDocument.Any() && ProjectDocument.Count > 0)
                    {
                        ProjectDocument.ForEach(projdata =>
                        {
                            var ProjectDocumentReview = _context.ProjectDocumentReview.Where(x => x.ProjectDocumentId == projdata.Id).ToList();
                            if (ProjectDocumentReview.Any() && ProjectDocumentReview.Count > 0)
                                _context.ProjectDocumentReview.RemoveRange(ProjectDocumentReview);
                        });
                        _context.ProjectDocument.RemoveRange(ProjectDocument);
                    }
                    if (project.ChildProject != null && project.ChildProject.Count > 0)
                    {
                        project.ChildProject.ForEach(x =>
                        {
                            _context.ProjectRight.RemoveRange(x.ProjectRight);
                        });
                    }

                    if (project.ChildProject != null && project.ChildProject.Count > 0)
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
                finaldata.Message = ex.Message.ToString();
                return finaldata;
            }
        }
    }
}
