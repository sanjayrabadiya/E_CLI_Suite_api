using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
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
        private readonly IAdverseEventSettingsLanguageRepository _adverseEventSettingsLanguageRepository;

        public AEReportingRepository(IGSCContext context, 
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IRandomizationRepository randomizationRepository,
            IProjectRepository projectRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IAEReportingValueRepository aEReportingValueRepository,
            IAdverseEventSettingsLanguageRepository adverseEventSettingsLanguageRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _randomizationRepository = randomizationRepository;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _aEReportingValueRepository = aEReportingValueRepository;
            _adverseEventSettingsLanguageRepository = adverseEventSettingsLanguageRepository;
        }

        public AEReportingDto GetAEReportingFilledForm(int id)
        {
            var aereportingdata = Find(id);
            var aereportingvalue = _aEReportingValueRepository.FindBy(x => x.AEReportingId == id).ToList();
            var aEReportingdto = _mapper.Map<AEReportingDto>(aereportingdata);
            var randomization = _randomizationRepository.Find(aereportingdata.RandomizationId);
            if (randomization == null) return new AEReportingDto();
            var projectid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var projectDesignTemplateId = _context.AdverseEventSettings.Where(x => x.ProjectId == projectid).ToList().FirstOrDefault().ProjectDesignTemplateIdPatient;
            var data = _projectDesignTemplateRepository.GetTemplate((int)projectDesignTemplateId);
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
            var severitydetails = GetSeverityDetails((int)projectid);
            aEReportingdto.SeverityLabel1 = severitydetails.SeverityLabel1;
            aEReportingdto.SeverityLabel2 = severitydetails.SeverityLabel2;
            aEReportingdto.SeverityLabel3 = severitydetails.SeverityLabel3;
            aEReportingdto.SeverityValue1 = severitydetails.SeverityValue1;
            aEReportingdto.SeverityValue2 = severitydetails.SeverityValue2;
            aEReportingdto.SeverityValue3 = severitydetails.SeverityValue3;
            return aEReportingdto;
        }

       public AEReportingForm GetSeverityDetails(int projectid)
        {
            var aEReportingdto = new AEReportingForm();
            var adverseeventsettings = _context.AdverseEventSettings.Where(x => x.ProjectId == projectid).ToList().FirstOrDefault();
            aEReportingdto.SeverityValue1 = adverseeventsettings.SeveritySeqNo1;
            aEReportingdto.SeverityValue2 = adverseeventsettings.SeveritySeqNo2;
            aEReportingdto.SeverityValue3 = adverseeventsettings.SeveritySeqNo3;
            //low  Uncomfortable but did not affect daily activities
            //medium Bad enough to affect daily activities
            //high Bad enough to be admitted to hospital
            string SeverityLabel1 = "Uncomfortable but did not affect daily activities";
            string SeverityLabel2 = "Bad enough to affect daily activities";
            string SeverityLabel3 = "Bad enough to be admitted to hospital";
            var languagedata = _adverseEventSettingsLanguageRepository.All.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.AdverseEventSettingsId == adverseeventsettings.Id).ToList();
            if (languagedata != null && languagedata.Count > 0)
            {
                SeverityLabel1 = (languagedata.FirstOrDefault().LowSeverityDisplay == null || languagedata.FirstOrDefault().LowSeverityDisplay == "") ? SeverityLabel1 : languagedata.FirstOrDefault().LowSeverityDisplay;
                SeverityLabel2 = (languagedata.FirstOrDefault().MediumSeverityDisplay == null || languagedata.FirstOrDefault().MediumSeverityDisplay == "") ? SeverityLabel2 : languagedata.FirstOrDefault().MediumSeverityDisplay;
                SeverityLabel3 = (languagedata.FirstOrDefault().HighSeverityDisplay == null || languagedata.FirstOrDefault().HighSeverityDisplay == "") ? SeverityLabel3 : languagedata.FirstOrDefault().HighSeverityDisplay;
            }
            if (aEReportingdto.SeverityValue1 == 1)
                aEReportingdto.SeverityLabel1 = SeverityLabel1;
            if (aEReportingdto.SeverityValue2 == 1)
                aEReportingdto.SeverityLabel2 = SeverityLabel1;
            if (aEReportingdto.SeverityValue3 == 1)
                aEReportingdto.SeverityLabel3 = SeverityLabel1;
            if (aEReportingdto.SeverityValue1 == 2)
                aEReportingdto.SeverityLabel1 = SeverityLabel2;
            if (aEReportingdto.SeverityValue2 == 2)
                aEReportingdto.SeverityLabel2 = SeverityLabel2;
            if (aEReportingdto.SeverityValue3 == 2)
                aEReportingdto.SeverityLabel3 = SeverityLabel2;
            if (aEReportingdto.SeverityValue1 == 3)
                aEReportingdto.SeverityLabel1 = SeverityLabel3;
            if (aEReportingdto.SeverityValue2 == 3)
                aEReportingdto.SeverityLabel2 = SeverityLabel3;
            if (aEReportingdto.SeverityValue3 == 3)
                aEReportingdto.SeverityLabel3 = SeverityLabel3;

            return aEReportingdto;
        }

        public AEReportingForm GetAEReportingForm()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null) return new AEReportingForm();
            var projectid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var projectDesignTemplateId = _context.AdverseEventSettings.Where(x => x.ProjectId == projectid).ToList().FirstOrDefault().ProjectDesignTemplateIdPatient;
            var data = _projectDesignTemplateRepository.GetTemplate((int)projectDesignTemplateId);
            var returndata = new AEReportingForm();
            returndata = GetSeverityDetails((int)projectid);
            returndata.template = data;
            return returndata;
        }

        public List<AEReportingGridDto> GetAEReportingGridData(int projectId)
        {
            var aEData = All.Include(x => x.Randomization).Where(x => x.Randomization.ProjectId == projectId && x.DeletedDate == null).ToList();//FindByInclude(x => x.Randomization.ProjectId == projectId && x.DeletedDate == null).ToList();
            var aEGridData = aEData.Select(c => new AEReportingGridDto
            {
                Id = c.Id,
                SubjectName = c.Randomization.FirstName + " " + c.Randomization.LastName,
                Initial = c.Randomization.Initial,
                ScreeningNumber = c.Randomization.ScreeningNumber,
                CreatedDate = c.CreatedDate,
                EventEffectName = c.EventEffect.GetDescription(),
                IsReviewedDone = c.IsReviewedDone,
                ReviewStatus = c.IsReviewedDone == false ? "" : (c.IsApproved == true ? "Approved" : "Rejected"),
                ApproveRejectDateTime = c.ApproveRejectDateTime,
                RejectReasonOth = c.RejectReasonOth,
                RejectReason = c.RejectReasonId == null ? "" : _context.AuditReason.Where(x => x.Id == c.RejectReasonId).ToList().FirstOrDefault().ReasonName,
            }).ToList();
            return aEGridData;
        }

        public List<AEReportingDto> GetAEReportingList()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null) return new List<AEReportingDto>();
            var data = FindBy(x => x.RandomizationId == randomization.Id).ToList();
            var datadtos = _mapper.Map<List<AEReportingDto>>(data);
            datadtos.ForEach(x =>
            {
                x.EventEffectName = x.EventEffect.GetDescription();
            });
            return datadtos;
        }

        public ScreeningDetailsforAE GetScreeningDetailsforAE(int id)
        {
            var aereportingdata = Find(id);
            var randomization = _randomizationRepository.Find(aereportingdata.RandomizationId);
            if (randomization == null) return new ScreeningDetailsforAE();
            var projectid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var projectDesignTemplateId = _context.AdverseEventSettings.Where(x => x.ProjectId == projectid).ToList().FirstOrDefault().ProjectDesignTemplateIdInvestigator;
            var projectDesignVisitId = _context.ProjectDesignTemplate.Where(x => x.Id == projectDesignTemplateId).ToList().FirstOrDefault().ProjectDesignVisitId;
            var data = new ScreeningDetailsforAE();
            data.ProjectId = randomization.ProjectId;
            data.ParentProjectId = (int)projectid;
            data.ProjectDesignTemplateId = (int)projectDesignTemplateId;
            var entry = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList();
            if (entry == null || entry.Count <= 0)
            {
                return new ScreeningDetailsforAE();
            } 
            data.ScreeningEntryId = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList().FirstOrDefault().Id;
            data.ProjectDesignPeriodId = _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id).ToList().FirstOrDefault().ProjectDesignPeriodId;
            data.ScreeningVisitId = _context.ScreeningVisit.Where(x => x.ScreeningEntryId == data.ScreeningEntryId && x.ProjectDesignVisitId == projectDesignVisitId).ToList().FirstOrDefault().Id;
            data.ScreeningTemplateId = _context.ScreeningTemplate.Where(x => x.ScreeningVisitId == data.ScreeningVisitId && x.ProjectDesignTemplateId == projectDesignTemplateId).ToList().FirstOrDefault().Id;
            data.Status = _context.ScreeningTemplate.Where(x => x.ScreeningVisitId == data.ScreeningVisitId && x.ProjectDesignTemplateId == projectDesignTemplateId).ToList().FirstOrDefault().Status;
            return data;
        }
    }
}
