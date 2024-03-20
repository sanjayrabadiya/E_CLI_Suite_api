using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Attendance;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.AdverseEvent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AEReportingController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IAEReportingRepository _iAEReportingRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly ISiteTeamRepository _siteTeamRepository;
        private readonly IUserRepository _usersRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _uow;
        private readonly IAEReportingValueRepository _aEReportingValueRepository;
        private readonly IGSCContext _context;
        private readonly IAdverseEventSettingsRepository _adverseEventSettingsRepository;
        public AEReportingController(IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IAEReportingRepository iAEReportingRepository,
            IRandomizationRepository randomizationRepository,
            IUnitOfWork uow,
            ISiteTeamRepository siteTeamRepository,
            IUserRepository usersRepository,
            IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository,
            IAEReportingValueRepository aEReportingValueRepository,
            IGSCContext context,
            IAdverseEventSettingsRepository adverseEventSettingsRepository
            )
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _iAEReportingRepository = iAEReportingRepository;
            _randomizationRepository = randomizationRepository;
            _uow = uow;
            _siteTeamRepository = siteTeamRepository;
            _usersRepository = usersRepository;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
            _aEReportingValueRepository = aEReportingValueRepository;
            _context = context;
            _adverseEventSettingsRepository = adverseEventSettingsRepository;
        }

        [HttpGet("GetAEReportingList")]
        public IActionResult GetAEReportingList()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (randomization == null || string.IsNullOrEmpty(randomization.RandomizationNumber))
            {
                ModelState.AddModelError("Message", "Randomization is pending!");
                return BadRequest(ModelState);
            }
            var data = _iAEReportingRepository.GetAEReportingList();
            return Ok(data);
        }

        [HttpGet("GetAEReportingForm")]
        public IActionResult GetAEReportingForm()
        {
            var data = _iAEReportingRepository.GetAEReportingForm();
            return Ok(data);
        }

        [HttpGet("GetAEReportingFilledForm/{id}")]
        public IActionResult GetAEReportingFilledForm(int id)
        {
            var data = _iAEReportingRepository.GetAEReportingFilledForm(id);
            return Ok(data);
        }

        [HttpPost("GetScreeningDetailsforAE/{id}")]
        public IActionResult GetScreeningDetailsforAE(int id)
        {
            var data = _iAEReportingRepository.GetScreeningDetailsforAE(id);
            _uow.Save();
            return Ok(data);
        }

        [HttpGet("GetAEReportingGridData/{projectId}")]
        public IActionResult GetAEReportingGridData(int projectId)
        {
            var data = _iAEReportingRepository.GetAEReportingGridData(projectId);
            return Ok(data);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AEReportingDto aEReportingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var aEReporting = _mapper.Map<AEReporting>(aEReportingDto);
            var randomization = await _randomizationRepository.AllIncluding(x => x.ScreeningEntry).FirstOrDefaultAsync(x => x.UserId == _jwtTokenAccesser.UserId);

            var template = _context.ScreeningTemplate.Include(x => x.ScreeningVisit.ScreeningEntry).FirstOrDefault(q => q.ProjectDesignTemplateId == aEReporting.ScreeningTemplateId && q.ScreeningVisit.ScreeningEntry.RandomizationId == randomization.Id);
            if (template != null && (template.IsHardLocked || template.IsLocked))
            {
                ModelState.AddModelError("Message", "The template is locked");
                return BadRequest(ModelState);
            }

            if (randomization == null)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event Reporting.");
                return BadRequest(ModelState);
            }
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studyname = _projectRepository.Find((int)studyId).ProjectCode;

            aEReporting.RandomizationId = randomization.Id;
            aEReporting.IsReviewedDone = false;
            aEReporting.AdverseEventSettingsId = studyId > 0 ? _adverseEventSettingsRepository.All.Where(x => x.ProjectId == (int)studyId && x.DeletedDate == null).Select(x => x.Id).FirstOrDefault() : 0;
            _iAEReportingRepository.Add(aEReporting);
            _uow.Save();
            for (int i = 0; i <= aEReportingDto.template.Variables.Count - 1; i++)
            {
                AEReportingValue aEReportingValue = new AEReportingValue();
                aEReportingValue.Id = 0;
                aEReportingValue.AEReportingId = aEReporting.Id;
                aEReportingValue.ProjectDesignVariableId = (int)aEReportingDto.template.Variables[i].ProjectDesignVariableId;
                aEReportingValue.Value = aEReportingDto.template.Variables[i].ScreeningValue;
                _aEReportingValueRepository.Add(aEReportingValue);
            }

            var siteteams = _siteTeamRepository.FindBy(x => x.ProjectId == randomization.ProjectId).ToList();
            var userdata = siteteams.Select(c => new UserDto
            {
                Id = c.UserId,
                UserName = _usersRepository.Find(c.UserId).UserName,
                Email = _usersRepository.Find(c.UserId).Email,
                Phone = _usersRepository.Find(c.UserId).Phone
            }).Distinct().ToList();

            userdata.ForEach(async x =>
            {
                await _emailSenderRespository.SendAdverseEventAlertEMailtoInvestigator(x.Email, x.Phone, x.UserName, studyname, randomization.Initial + " " + randomization.ScreeningNumber, DateTime.Now.ToString("dd-MMM-yyyy"));
            });
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event Reporting.");
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [HttpPut("ApproveAdverseEvent/{id}")]
        public IActionResult ApproveAdverseEvent(int id)
        {
            if (id <= 0) return BadRequest();

            var data = _iAEReportingRepository.Find(id);
            data.IsReviewedDone = true;
            data.IsApproved = true;
            data.ApproveRejectDateTime = _jwtTokenAccesser.GetClientDate();
            data.ReviewedByUser = _jwtTokenAccesser.UserId;
            data.ReviewedByRole = _jwtTokenAccesser.RoleId;
            _iAEReportingRepository.Update(data);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Approve Failed.");
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpPut("RejectAdverseEvent")]
        public IActionResult RejectAdverseEvent([FromBody] AEReportingDto aEReportingDto)
        {
            if (aEReportingDto.Id <= 0) return BadRequest();

            var data = _iAEReportingRepository.Find(aEReportingDto.Id);
            data.IsReviewedDone = true;
            data.IsApproved = false;
            data.RejectReasonId = aEReportingDto.RejectReasonId;
            data.RejectReasonOth = aEReportingDto.RejectReasonOth;
            data.ApproveRejectDateTime = _jwtTokenAccesser.GetClientDate();
            data.ReviewedByUser = _jwtTokenAccesser.UserId;
            data.ReviewedByRole = _jwtTokenAccesser.RoleId;
            _iAEReportingRepository.Update(data);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Approve Failed.");
                return BadRequest(ModelState);
            }
            return Ok();
        }

    }
}
