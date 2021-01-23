using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.AdverseEvent;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Attendance;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public AEReportingController(IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IAEReportingRepository iAEReportingRepository,
            IRandomizationRepository randomizationRepository,
            IUnitOfWork uow,
            ISiteTeamRepository siteTeamRepository,
            IUserRepository usersRepository,
            IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository
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
        }

        [HttpGet("GetAEReportingList")]
        public IActionResult GetAEReportingList()
        {
            var data = _iAEReportingRepository.GetAEReportingList();
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
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event Reporting.");
                return BadRequest(ModelState);
            }
            aEReporting.RandomizationId = randomization.Id;
            aEReporting.IsReviewedDone = false;
            _iAEReportingRepository.Add(aEReporting);
            var siteteams = _siteTeamRepository.FindBy(x => x.ProjectId == randomization.ProjectId).ToList();
            var userdata = siteteams.Select(c => new UserDto
            {
                Id = c.UserId,
                UserName = _usersRepository.Find(c.UserId).UserName,
                Email = _usersRepository.Find(c.UserId).Email,
                Phone = _usersRepository.Find(c.UserId).Phone
            }).Distinct().ToList();
            userdata = userdata.Distinct().ToList();
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studyname = _projectRepository.Find((int)studyId).ProjectCode;
            userdata.ForEach(async x =>
            {
                await _emailSenderRespository.SendAdverseEventAlertEMailtoInvestigator(x.Email, x.Phone, x.UserName, studyname, randomization.Initial + " " + randomization.ScreeningNumber, DateTime.Now.ToString("dd-MMM-yyyy"));
            });
            if (_uow.Save() <= 0) throw new Exception("Error to save Adverse Event Reporting.");

            return Ok();
        }

    }
}
