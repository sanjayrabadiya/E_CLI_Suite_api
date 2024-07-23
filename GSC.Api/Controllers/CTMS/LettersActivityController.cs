using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class LettersActivityController : BaseController
    {
        private readonly ILettersActivityRepository _lettersActivityRepository;
        private readonly ILettersFormateRepository _lettersFormateRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public LettersActivityController(ILettersActivityRepository lettersActivityRepository, IJwtTokenAccesser jwtTokenAccesser,
            ILettersFormateRepository lettersFormateRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _lettersActivityRepository = lettersActivityRepository;
            _lettersFormateRepository = lettersFormateRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}/{projectId:int?}")]
        public IActionResult Get(bool isDeleted, int? projectId)
        {
            var lettersActivity = _lettersActivityRepository.GetLettersActivityList(isDeleted, projectId);
            return Ok(lettersActivity);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var lettersActivity = _lettersActivityRepository.Find(id);
            var lettersActivityDto = _mapper.Map<LettersActivityDto>(lettersActivity);
            return Ok(lettersActivityDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] LettersActivityDto lettersActivityDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lettersFormate = _lettersFormateRepository.Find(lettersActivityDto.LettersFormateId);

             _lettersActivityRepository.CreateLettersEmail(lettersFormate,lettersActivityDto);

            lettersActivityDto.Id = 0;
            var lettersActivity = _mapper.Map<LettersActivity>(lettersActivityDto);
            lettersActivity.IpAddress = _jwtTokenAccesser.IpAddress;
            lettersActivity.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _lettersActivityRepository.Add(lettersActivity);
            if (_uow.Save() <= 0) return Ok(new Exception("letters Formate failed on save."));
            return Ok(lettersActivity.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LettersActivityDto lettersActivityDto)
        {
            if (lettersActivityDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _lettersActivityRepository.updateLettersEmail(lettersActivityDto);

            var lettersActivity = _mapper.Map<LettersActivity>(lettersActivityDto);
            lettersActivity.IpAddress = _jwtTokenAccesser.IpAddress;
            lettersActivity.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _lettersActivityRepository.Update(lettersActivity);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating letters Formate failed on save."));
            return Ok(lettersActivity.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _lettersActivityRepository.Find(id);
            if (record == null)
                return NotFound();

            _lettersActivityRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _lettersActivityRepository.Find(id);

            if (record == null)
                return NotFound();
            _lettersActivityRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetActivityTypeDropDown")]
        public IActionResult GetActivityTypeDropDown()
        {
            return Ok(_lettersActivityRepository.GetActivityTypeDropDown());
        }

        [HttpGet]
        [Route("GetLettersFormatTypeDropDown")]
        public IActionResult GetLettersFormatTypeDropDown()
        {
            return Ok(_lettersActivityRepository.GetLettersFormatTypeDropDown());
        }
       
        [HttpGet]
        [Route("GetMedicalUserTypeDown/{siteId}")]
        public IActionResult GetMedicalUserTypeDown(int siteId)
        {
            return Ok(_lettersActivityRepository.GetMedicalUserTypeDown(siteId));
        }

        [HttpGet]
        [Route("getSelectDateDrop/{projectId}/{siteId}")]
        public IActionResult getSelectDateDrop(int projectId, int siteId)
        {
            return Ok(_lettersActivityRepository.getSelectDateDrop(projectId, siteId));
        }

        [HttpPost]
        [Route("GetSendMail")]
        public IActionResult GetSendMail([FromBody] SendMailModel sendMailModel)
        {
            _lettersActivityRepository.GetSendMail(sendMailModel);
            return Ok(true);
        }

        [HttpGet]
        [Route("UserRoles/{Id}")]
        public IActionResult UserRoles(int Id)
        {
            if (Id <= 0) return BadRequest();

            var record = _lettersActivityRepository.Find(Id);
            return Ok(_lettersActivityRepository.UserRoles(record.ProjectId));
            
        }
    }
}