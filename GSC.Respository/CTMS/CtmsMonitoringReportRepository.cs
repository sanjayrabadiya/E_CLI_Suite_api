using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportRepository : GenericRespository<CtmsMonitoringReport>, ICtmsMonitoringReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;
        public CtmsMonitoringReportRepository(IGSCContext context,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int CtmsMonitoringReportId)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var ctmsMonitoringReport = All.Where(x => x.Id == CtmsMonitoringReportId && x.DeletedDate == null).FirstOrDefault();

            var ctmsMonitoringReportFormBasic = GetFormBasic(CtmsMonitoringReportId);

            designTemplateDto.CtmsMonitoringReportId = CtmsMonitoringReportId;
            designTemplateDto.ProjectId = ctmsMonitoringReportFormBasic.ProjectId;
            if (ctmsMonitoringReport != null)
            {
                designTemplateDto.IsSender = ctmsMonitoringReport.CreatedBy == _jwtTokenAccesser.UserId;
                designTemplateDto.ReportStatus = ctmsMonitoringReport.ReportStatus;

                designTemplateDto.VariableDisable =
                     ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.ReviewInProgress
                    || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.Approved;
            }
            var reviewPerson = _ctmsMonitoringReportReviewRepository.GetReview(CtmsMonitoringReportId);

            designTemplateDto.Variables.Where(x => x.CollectionSource == CollectionSources.Table).ToList().ForEach(t =>
            {
                t.Values.Where(c => c.LevelNo == null).ToList().ForEach(v =>
                {
                    v.LevelNo = 1;
                });
            });

            var values = _ctmsMonitoringReportVariableValueRepository.GetVariableValues(ctmsMonitoringReportFormBasic.Id);

            values.ForEach(t =>
            {
                var ScreeningTemplateValueChild = GetCTMSTemplateValueChild(t.Id);

                var MaxLevel = ScreeningTemplateValueChild.Max(x => x.LevelNo);
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.StudyLevelFormVariableId == t.StudyLevelFormVariableId);
                if (variable != null)
                {
                    variable.VariableValue = t.Value;
                    variable.VariableValueOld = t.Value;
                    variable.CtmsMonitoringReportVariableValueId = t.Id;
                    variable.HasComments = t.IsComment;
                    variable.IsReviewPerson = reviewPerson;
                    variable.QueryStatus = t.QueryStatus;
                    variable.IsNaValue = t.IsNa;
                    if (!string.IsNullOrWhiteSpace(variable.VariableValue) || variable.IsNaValue)
                        variable.IsValid = true;
                    variable.DocPath = t.DocPath != null ? t.DocPath : null;
                    variable.DocFullPath = t.DocPath != null ? documentUrl + t.DocPath : null;
                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.StudyLevelFormVariableValueId == val.Id);
                            if (childValue != null)
                            {
                                variable.IsValid = true;
                                val.VariableValue = childValue.Value;
                                val.VariableValueOld = childValue.Value;
                                val.CtmsMonitoringReportVariableValueChildId = childValue.Id;
                            }
                        });

                    if (variable.Values != null && variable.CollectionSource == CollectionSources.Table)
                    {
                        var ValuesList = new List<StudyLevelFormVariableValueDto>();

                        variable.Values.ToList().ForEach(val =>
                        {
                            MaxLevel = MaxLevel > 0 ? MaxLevel : 0;
                            var notExistLevel = Enumerable.Range(1, (int)MaxLevel).ToArray();

                            var childValue = t.Children.Where(v => v.StudyLevelFormVariableValueId == val.Id).GroupBy(x => x.LevelNo)
                            .Select(x => new CtmsMonitoringReportVariableValueChild
                            {

                                Id = x.Select(s => s.Id).FirstOrDefault(),
                                CtmsMonitoringReportVariableValueId = x.Select(s => s.CtmsMonitoringReportVariableValueId).FirstOrDefault(),
                                StudyLevelFormVariableValueId = x.Select(s => s.StudyLevelFormVariableValueId).FirstOrDefault(),
                                Value = x.Select(s => s.Value).FirstOrDefault(),
                                LevelNo = x.Select(s => s.LevelNo).FirstOrDefault(),
                                DeletedDate = x.Select(s => s.DeletedDate).FirstOrDefault()
                            }).ToList();


                            var Levels = notExistLevel.Where(x => !childValue.Select(y => (int)y.LevelNo).Contains(x)).ToList();

                            Levels.ForEach(x =>
                            {
                                CtmsMonitoringReportVariableValueChild obj = new CtmsMonitoringReportVariableValueChild();
                                obj.Id = 0;
                                obj.CtmsMonitoringReportVariableValueId = t.Id;
                                obj.StudyLevelFormVariableValueId = val.Id;
                                obj.Value = null;
                                obj.LevelNo = (short)x;
                                childValue.Add(obj);
                            });

                            if (childValue.Count == 0 && Levels.Count == 0)
                            {
                                CtmsMonitoringReportVariableValueChild obj = new CtmsMonitoringReportVariableValueChild();
                                obj.Id = 0;
                                obj.CtmsMonitoringReportVariableValueId = t.Id;
                                obj.StudyLevelFormVariableValueId = val.Id;
                                obj.Value = null;
                                obj.LevelNo = 1;
                                childValue.Add(obj);
                            }

                            childValue.ForEach(child =>
                            {
                                StudyLevelFormVariableValueDto obj = new StudyLevelFormVariableValueDto();
                                variable.IsValid = true;
                                obj.Id = child.StudyLevelFormVariableValueId;
                                obj.VariableValue = child.Value;
                                obj.VariableValueOld = child.Value;
                                obj.CtmsMonitoringReportVariableValueChildId = child.Id;
                                obj.LevelNo = child.LevelNo;
                                obj.ValueName = val.ValueName;
                                if (child.DeletedDate == null)
                                    obj.IsDeleted = false;
                                else  obj.IsDeleted = true;

                                obj.TableCollectionSource = val.TableCollectionSource;
                                ValuesList.Add(obj);
                            });
                        });
                        variable.Values = ValuesList.Where(x => x.IsDeleted).ToList();
                    }
                }
            });

            return designTemplateDto;
        }

        public CtmsMonitoringReportBasic GetFormBasic(int ManageMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoring).ThenInclude(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate)
                .Where(r => r.Id == ManageMonitoringReportId).Select(
               c => new CtmsMonitoringReportBasic
               {
                   Id = c.Id,
                   StudyLevelFormId = c.CtmsMonitoring.StudyLevelFormId,
                   ProjectId = c.CtmsMonitoring.ProjectId,
                   VariableTemplateId = c.CtmsMonitoring.StudyLevelForm.VariableTemplateId,
               }).FirstOrDefault();
        }

        public string GetMonitoringFormApprovedOrNOt(int projectId, int siteId, int tabNumber)
        {
            var appscreen = _context.AppScreen.Where(x => x.ScreenCode == "mnu_ctms").FirstOrDefault();

            string ActivityCode = "";

            if (tabNumber == 0)
                ActivityCode = "act_001";
            else if (tabNumber == 1)
                ActivityCode = "act_002";
            else if (tabNumber == 2)
                ActivityCode = "act_003";
            else if (tabNumber == 3)
                ActivityCode = "act_004";
            else if (tabNumber == 4)
                ActivityCode = "act_005";
            else
                ActivityCode = "act_006";

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == ActivityCode && x.DeletedDate == null).FirstOrDefault();

            var Activity = _context.Activity.Where(x => x.CtmsActivityId == CtmsActivity.Id && x.AppScreenId == appscreen.Id && x.DeletedDate == null).FirstOrDefault();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                                .Where(x => x.ProjectId == projectId && x.ActivityId == Activity.Id
                                && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            if (ActivityCode != "act_004" && ActivityCode != "act_001")
            {
                var CtmsMonitoringStatus = _context.CtmsMonitoringStatus.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) && x.CtmsMonitoring.DeletedDate == null).ToList();
            
                if (!(CtmsMonitoringStatus.Count != 0 && CtmsMonitoringStatus.OrderByDescending(c => c.Id).Select(s=>s.Status).FirstOrDefault() == MonitoringSiteStatus.Approved))
                    return "Please Approve " + CtmsActivity.ActivityName + " .";
             
                return "";
            }
            else if (ActivityCode == "act_001")
            {
                //Add by mitul on 04-10-2023 Is Not Applicable in feasibility
                var CtmsMonitoringStatus = _context.CtmsMonitoringStatus.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId) && x.CtmsMonitoring.DeletedDate == null).ToList();
                var applicable = _context.CtmsMonitoring.Where(x => x.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.StudyLevelFormId) && x.DeletedDate == null).ToList();
                if (applicable.Count > 0 && applicable.OrderByDescending(c => c.Id).Select(s => s.IfApplicable).FirstOrDefault() != true)
                {
                    if (!(CtmsMonitoringStatus.Count != 0 && CtmsMonitoringStatus.OrderByDescending(c => c.Id).Select(s => s.Status).FirstOrDefault() == MonitoringSiteStatus.Approved))
                        return "Please Approve " + CtmsActivity.ActivityName + " .";

                    return "";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                var CtmsMonitoringStatus = _context.CtmsMonitoringReport.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
                                      && x.CtmsMonitoring.DeletedDate == null).ToList();

                var openQuerydata = _context.CtmsActionPoint.Include(s => s.CtmsMonitoring).ThenInclude(d => d.StudyLevelForm).ThenInclude(x => x.Activity).ThenInclude(r => r.CtmsActivity).
                    Where(x => x.CtmsMonitoring.ProjectId == siteId && x.Status == CtmsActionPointStatus.Open && x.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityCode != "act_005").Count() > 0;

                if (!(CtmsMonitoringStatus.Count != 0 && CtmsMonitoringStatus.OrderByDescending(c => c.Id).Select(s => s.ReportStatus).FirstOrDefault() == MonitoringReportStatus.Approved))
                    return "Please Approve " + CtmsActivity.ActivityName + " .";
                else if (openQuerydata)
                    return "Please Close All Open Query.";

                return "";
            }
        }
        private List<CtmsMonitoringReportVariableValueChild> GetCTMSTemplateValueChild(int CtmsMonitoringReportVariableValueId)
        {
            return _ctmsMonitoringReportVariableValueChildRepository.All.AsNoTracking().Where(t => t.CtmsMonitoringReportVariableValueId == CtmsMonitoringReportVariableValueId && t.DeletedDate == null).ToList();
        }
    }
}