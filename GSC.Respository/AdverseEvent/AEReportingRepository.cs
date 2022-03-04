using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.AdverseEvent
{
    public class AEReportingRepository : GenericRespository<AEReporting>, IAEReportingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IAEReportingValueRepository _aEReportingValueRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IAdverseEventSettingsRepository _adverseEventSettingsRepository;

        public AEReportingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IRandomizationRepository randomizationRepository,
            IProjectRepository projectRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IAEReportingValueRepository aEReportingValueRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IUnitOfWork uow,
            IAdverseEventSettingsRepository adverseEventSettingsRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _randomizationRepository = randomizationRepository;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _aEReportingValueRepository = aEReportingValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _uow = uow;
            _adverseEventSettingsRepository = adverseEventSettingsRepository;
        }

        public AEReportingDto GetAEReportingFilledForm(int id)
        {
            var aereportingdata = Find(id);
            var randomization = _randomizationRepository.Find(aereportingdata.RandomizationId);
            if (randomization == null) return new AEReportingDto();
            var adversesettingdata = _adverseEventSettingsRepository.All.Where(x => x.Id == aereportingdata.AdverseEventSettingsId).FirstOrDefault();
            if (adversesettingdata != null)
            {
                var aereportingvalue = _aEReportingValueRepository.FindBy(x => x.AEReportingId == id).ToList();
                var aEReportingdto = _mapper.Map<AEReportingDto>(aereportingdata);

                var data = _projectDesignTemplateRepository.GetTemplate(adversesettingdata.ProjectDesignTemplateIdPatient);
                aereportingvalue.ForEach(t =>
                {
                    var variable = data.Variables.FirstOrDefault(v => v.ProjectDesignVariableId == t.ProjectDesignVariableId);
                    if (variable != null)
                    {
                        variable.ScreeningValue = t.Value;
                        variable.ScreeningTemplateValueId = t.Id;
                    }
                });
                aEReportingdto.template = data;
                return aEReportingdto;
            }
            return new AEReportingDto();

        }


        public DesignScreeningTemplateDto GetAEReportingForm()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (randomization == null) return new DesignScreeningTemplateDto();
            var projectid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var adverseEventSettings = _context.AdverseEventSettings.Where(x => x.ProjectId == projectid).FirstOrDefault();
            if (adverseEventSettings == null)
            {
                return null;
            }
            var data = _projectDesignTemplateRepository.GetTemplate(adverseEventSettings.ProjectDesignTemplateIdPatient);
            return data;
        }

        public List<AEReportingGridDto> GetAEReportingGridData(int projectId)
        {
            var aEData = All.Include(x => x.CreatedByUser).Include(x => x.Randomization).Include(x => x.AEReportingValueValues).Where(x => x.Randomization.ProjectId == projectId && x.DeletedDate == null).ToList();
            var aEGridData = aEData.Select(c => new AEReportingGridDto
            {
                Id = c.Id,
                SubjectName = c.Randomization.FirstName + " " + c.Randomization.LastName,
                Initial = c.Randomization.Initial,
                ScreeningNumber = c.Randomization.ScreeningNumber,
                RandomizationNumber = c.Randomization.RandomizationNumber,
                CreatedDate = c.CreatedDate,
                EventEffectName = GetEventEffectName(c.Id, c.AdverseEventSettingsId),
                IsReviewedDone = c.IsReviewedDone,
                ReviewStatus = c.IsReviewedDone == false ? "" : (c.IsApproved == true ? "Approved" : "Rejected"),
                ApproveRejectDateTime = c.ApproveRejectDateTime,
                RejectReasonOth = c.RejectReasonOth,
                RejectReason = c.RejectReasonId == null ? "" : _context.AuditReason.Where(x => x.Id == c.RejectReasonId).ToList().FirstOrDefault().ReasonName,
                Status = c.IsReviewedDone ? "Review Done" : "Review Pending",
                ReviewBy = c.ReviewedByUser != null ? _context.Users.Where(x => x.Id == c.ReviewedByUser).FirstOrDefault().UserName : "",
                IsApproved = c.IsApproved
            }).OrderByDescending(x => x.CreatedDate).ToList();
            return aEGridData;
        }

        public List<DashboardDto> GetAEReportingMyTaskList(int ProjectId)
        {
            var projectIdlist = _context.Project.Where(x => x.ParentProjectId == ProjectId).Select(x => x.Id).ToList();
            var rolelist = _context.SiteTeam.Where(x => projectIdlist.Contains(x.ProjectId) && x.DeletedDate == null).Select(x => x.RoleId).ToList();
            if (_context.ProjectRight.Any(x => rolelist.Contains(x.RoleId) && x.DeletedDate == null && x.RoleId == _jwtTokenAccesser.RoleId && x.UserId == _jwtTokenAccesser.UserId))
            {
                var result = All.Include(x => x.Randomization).Include(x => x.AEReportingValueValues).Where(x => projectIdlist.Contains(x.Randomization.ProjectId) && x.DeletedDate == null && x.Randomization.DeletedDate == null && !x.IsReviewedDone).Select(x => new DashboardDto
                {
                    Id = x.Id,
                    TaskInformation = $"{x.Randomization.FirstName} {x.Randomization.LastName} - {x.Randomization.ScreeningNumber}",
                    ExtraData = new { ReviewDone = x.IsReviewedDone, createdByUser = x.CreatedByUser.UserName, CreatedDate = x.CreatedDate, Data = x.AEReportingValueValues },
                    Module = MyTaskModule.AdverseEvent.GetDescription(),
                }).ToList();
                return result;
            }
            return new List<DashboardDto>();
        }

        public string GetEventEffectName(int id, int? adversesettingId)
        {
            var data = _context.AEReportingValue.Include(x => x.ProjectDesignVariable).Where(x => x.AEReportingId == id
                        && (x.ProjectDesignVariable.Id == x.ProjectDesignVariableId
                        && x.ProjectDesignVariable.CollectionSource == Helper.CollectionSources.RadioButton)
                        ).FirstOrDefault();
            if (data != null)
            {
                var eventname = _context.AdverseEventSettingsDetails.Where(x => x.AdverseEventSettingsId == adversesettingId
                               && x.ProjectDesignVariableValueId == Convert.ToInt32(data.Value)).Select(x => x.Severity).FirstOrDefault();
                return eventname;
            }
            return "";
        }

        public List<AEReportingDto> GetAEReportingList()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null) return new List<AEReportingDto>();
            var data = FindBy(x => x.RandomizationId == randomization.Id).ToList();
            var datadtos = _mapper.Map<List<AEReportingDto>>(data);
            datadtos.ForEach(x =>
            {
                x.EventEffectName = GetEventEffectName(x.Id, x.AdverseEventSettingsId);
            });
            return datadtos;
        }

        public ScreeningDetailsforAE GetScreeningDetailsforAE(int id)
        {
            var aereportingdata = Find(id);
            var adversesettingdata = _adverseEventSettingsRepository.All.Where(x => x.Id == aereportingdata.AdverseEventSettingsId).FirstOrDefault();
            var randomization = _randomizationRepository.Find(aereportingdata.RandomizationId);
            if (randomization == null) return new ScreeningDetailsforAE();
            var projectid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var projectDesignTemplateId = adversesettingdata.ProjectDesignTemplateIdInvestigator;
            var projectDesignVisitId = _context.ProjectDesignTemplate.Where(x => x.Id == projectDesignTemplateId).ToList().FirstOrDefault().ProjectDesignVisitId;
            var data = new ScreeningDetailsforAE();
            data.ProjectId = randomization.ProjectId;
            data.ParentProjectId = (int)projectid;
            data.ProjectDesignTemplateId = (int)projectDesignTemplateId;
            var entry = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList();
            if (entry == null || entry.Count <= 0)
            {
                data.RandomizationId = randomization.Id;
                data.ProjectDesignVisitId = projectDesignVisitId;
                return data;
            }
            data.ScreeningEntryId = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList().FirstOrDefault().Id;
            data.ProjectDesignPeriodId = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList().FirstOrDefault().ProjectDesignPeriodId;
            data.ScreeningVisitId = _context.ScreeningVisit.Where(x => x.ScreeningEntryId == data.ScreeningEntryId && x.ProjectDesignVisitId == projectDesignVisitId).ToList().FirstOrDefault().Id;
            if (aereportingdata.ScreeningTemplateId == null)
            {
                var screeningtemplates = _context.ScreeningTemplate.Where(x => x.ScreeningVisitId == data.ScreeningVisitId && x.ProjectDesignTemplateId == data.ProjectDesignTemplateId && x.Status == Helper.ScreeningTemplateStatus.Pending).ToList();
                if (screeningtemplates.Count > 0)
                {
                    data.ScreeningTemplateId = screeningtemplates.FirstOrDefault().Id;
                    data.Status = screeningtemplates.FirstOrDefault().Status;
                    aereportingdata.ScreeningTemplateId = data.ScreeningTemplateId;
                    Update(aereportingdata);
                }
                else
                {
                    var parentscreeningTemplateId = _screeningTemplateRepository.FindBy(x => x.ScreeningVisitId == data.ScreeningVisitId && x.ProjectDesignTemplateId == data.ProjectDesignTemplateId && x.ParentId == null).ToList().FirstOrDefault().Id;
                    var repeatScreeningTemplate = _screeningTemplateRepository.TemplateRepeat(parentscreeningTemplateId);
                    _uow.Save();
                    data.ScreeningTemplateId = repeatScreeningTemplate.Id;
                    data.Status = repeatScreeningTemplate.Status;
                    aereportingdata.ScreeningTemplateId = data.ScreeningTemplateId;
                    Update(aereportingdata);
                }
            }
            else
            {
                data.ScreeningTemplateId = _context.ScreeningTemplate.Where(x => x.Id == (int)aereportingdata.ScreeningTemplateId).ToList().FirstOrDefault().Id;
                data.Status = _context.ScreeningTemplate.Where(x => x.Id == (int)aereportingdata.ScreeningTemplateId).ToList().FirstOrDefault().Status;
            }
            return data;
        }
    }
}
