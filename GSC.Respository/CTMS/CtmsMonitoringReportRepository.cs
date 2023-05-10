using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportRepository : GenericRespository<CtmsMonitoringReport>, ICtmsMonitoringReportRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportReviewRepository _ctmsMonitoringReportReviewRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;
        public CtmsMonitoringReportRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            ICtmsMonitoringReportReviewRepository ctmsMonitoringReportReviewRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _ctmsMonitoringReportReviewRepository = ctmsMonitoringReportReviewRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
        }

        public CtmsMonitoringReportFormDto GetCtmsMonitoringReportVariableValue(CtmsMonitoringReportFormDto designTemplateDto, int CtmsMonitoringReportId)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var ctmsMonitoringReport = All.Where(x => x.Id == CtmsMonitoringReportId && x.DeletedDate == null).FirstOrDefault();

            var ctmsMonitoringReportFormBasic = GetFormBasic(CtmsMonitoringReportId);

            designTemplateDto.VariableTemplateId = designTemplateDto.VariableTemplateId;
            designTemplateDto.CtmsMonitoringReportId = CtmsMonitoringReportId;
            designTemplateDto.ProjectId = ctmsMonitoringReportFormBasic.ProjectId;
            designTemplateDto.IsSender = ctmsMonitoringReport.CreatedBy == _jwtTokenAccesser.UserId;
            designTemplateDto.ReportStatus = ctmsMonitoringReport.ReportStatus;
            //Changes made by Sachin
            designTemplateDto.VariableDisable = ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.OnGoing
                || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.ReviewInProgress
                || ctmsMonitoringReport.ReportStatus == MonitoringReportStatus.Approved;

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
                                Id = x.FirstOrDefault().Id,
                                CtmsMonitoringReportVariableValueId = x.FirstOrDefault().CtmsMonitoringReportVariableValueId,
                                StudyLevelFormVariableValueId = x.FirstOrDefault().StudyLevelFormVariableValueId,
                                Value = x.FirstOrDefault().Value,
                                LevelNo = x.FirstOrDefault().LevelNo,
                                DeletedDate = x.FirstOrDefault().DeletedDate
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

                            if (childValue.Count() == 0 && Levels.Count() == 0)
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
                                obj.IsDeleted = child.DeletedDate == null ? false : true;
                                obj.TableCollectionSource = val.TableCollectionSource;
                                ValuesList.Add(obj);
                            });
                        });
                        variable.Values = ValuesList.Where(x => x.IsDeleted == false).ToList();
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

            string ActivityCode = tabNumber == 0 ? "act_001" : tabNumber == 1 ? "act_002" : tabNumber == 2 ? "act_003" :
                tabNumber == 3 ? "act_004" : tabNumber == 4 ? "act_005" : "act_006";

            var CtmsActivity = _context.CtmsActivity.Where(x => x.ActivityCode == ActivityCode && x.DeletedDate == null).FirstOrDefault();

            var Activity = _context.Activity.Where(x => x.CtmsActivityId == CtmsActivity.Id && x.AppScreenId == appscreen.Id && x.DeletedDate == null).FirstOrDefault();

            var StudyLevelForm = _context.StudyLevelForm.Include(x => x.Activity)
                                .Where(x => x.ProjectId == projectId && x.ActivityId == Activity.Id
                                && x.AppScreenId == appscreen.Id && x.DeletedDate == null).ToList();

            if(ActivityCode != "act_004")
            {
                var CtmsMonitoringStatus = _context.CtmsMonitoringStatus.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
                                     && x.CtmsMonitoring.DeletedDate == null).ToList();

                if (!(CtmsMonitoringStatus.Count() != 0 && CtmsMonitoringStatus.OrderByDescending(c => c.Id).FirstOrDefault().Status == MonitoringSiteStatus.Approved))
                    return "Please Approve " + CtmsActivity.ActivityName + " .";

                return "";
            }
            else
            {
                var CtmsMonitoringStatus = _context.CtmsMonitoringReport.Where(x => x.CtmsMonitoring.ProjectId == siteId && StudyLevelForm.Select(y => y.Id).Contains(x.CtmsMonitoring.StudyLevelFormId)
                                      && x.CtmsMonitoring.DeletedDate == null).ToList();

                if (!(CtmsMonitoringStatus.Count() != 0 && CtmsMonitoringStatus.OrderByDescending(c => c.Id).FirstOrDefault().ReportStatus == MonitoringReportStatus.Approved))
                    return "Please Approve " + CtmsActivity.ActivityName + " .";

                return "";
            }
        }
        private List<CtmsMonitoringReportVariableValueChild> GetCTMSTemplateValueChild(int CtmsMonitoringReportVariableValueId)
        {
            return _ctmsMonitoringReportVariableValueChildRepository.All.AsNoTracking().Where(t => t.CtmsMonitoringReportVariableValueId == CtmsMonitoringReportVariableValueId && t.DeletedDate == null).ToList();
        }
    }
}