using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsActionPointController : BaseController
    {
        private readonly ICtmsActionPointRepository _ctmsActionPointRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsActionPointController(ICtmsActionPointRepository ctmsActionPointRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _ctmsActionPointRepository = ctmsActionPointRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ctmsActionPoint = _ctmsActionPointRepository.Find(id);
            var ctmsActionPointDto = _mapper.Map<CtmsActionPointDto>(ctmsActionPoint);
            return Ok(ctmsActionPointDto);
        }

        [HttpGet]
        [Route("GetActionPoint/{CtmsMonitoringId}")]
        public IActionResult GetActionPoint(int CtmsMonitoringId)
        {
            if (CtmsMonitoringId <= 0) return BadRequest();
            var ctmsActionPoint = _ctmsActionPointRepository.GetActionPointList(CtmsMonitoringId);
            return Ok(ctmsActionPoint);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsActionPointDto ctmsActionPointDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsActionPointDto.Id = 0;
            ctmsActionPointDto.Status = Helper.CtmsActionPointStatus.Open;
            var ctmsActionPoint = _mapper.Map<CtmsActionPoint>(ctmsActionPointDto);
            _ctmsActionPointRepository.Add(ctmsActionPoint);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating action point failed on save."));

            return Ok(ctmsActionPoint.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsActionPointDto ctmsActionPointDto)
        {
            if (ctmsActionPointDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ctmsActionPointDto.ResponseBy = _jwtTokenAccesser.UserId;
            ctmsActionPointDto.ResponseDate = _jwtTokenAccesser.GetClientDate();
            ctmsActionPointDto.Status = Helper.CtmsActionPointStatus.Resolved;
            var ctmsActionPoint = _mapper.Map<CtmsActionPoint>(ctmsActionPointDto);

            _ctmsActionPointRepository.Update(ctmsActionPoint);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating action point failed on save."));
            return Ok(ctmsActionPoint.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsActionPointRepository.Find(id);

            if (record == null)
                return NotFound();

            _ctmsActionPointRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPut("CloseQueryActionPoint/{id}")]
        public IActionResult CloseQueryActionPoint(int id)
        {
            var ctmsActionPoint = _ctmsActionPointRepository.Find(id);

            ctmsActionPoint.CloseBy = _jwtTokenAccesser.UserId;
            ctmsActionPoint.CloseDate = _jwtTokenAccesser.GetClientDate();
            ctmsActionPoint.Status = Helper.CtmsActionPointStatus.Closed;

            _ctmsActionPointRepository.Update(ctmsActionPoint);
            if (_uow.Save() <= 0) return Ok(new Exception("Close action point failed on save."));
            return Ok(ctmsActionPoint.Id);
        }

        [HttpGet]
        [Route("GetActionPointForFollowUpList/{ProjectId}")]
        public IActionResult GetActionPointForFollowUpList(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var ctmsActionPoint = _ctmsActionPointRepository.GetActionPointForFollowUpList(ProjectId);
            return Ok(ctmsActionPoint);
        }
    }
}