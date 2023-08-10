using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class WorkingDayController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IWorkingDayRepository _workingDayRepository;

        public WorkingDayController(IUnitOfWork uow, IMapper mapper,
         IJwtTokenAccesser jwtTokenAccesser, IGSCContext context, IWorkingDayRepository workingDayRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _workingDayRepository = workingDayRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var holidaylist = _workingDayRepository.GetWorkingDayList(isDeleted);
            return Ok(holidaylist);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var WorkingDayData = _workingDayRepository.Find(id);
            var WorkingDayDto = _mapper.Map<WorkingDayDto>(WorkingDayData);
            WorkingDayDto.SiteId = WorkingDayDto.IsSite == true ? WorkingDayDto.ProjectId : (int?)null;
            WorkingDayDto.ProjectId = WorkingDayDto.IsSite == true ? (int)_context.Project.Find(WorkingDayDto.ProjectId).ParentProjectId : WorkingDayDto.ProjectId;
            return Ok(WorkingDayDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] WorkingDayDto WorkingDay)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            WorkingDay.Id = 0;
            var WorkingDaydata = _mapper.Map<WorkingDay>(WorkingDay);
            _workingDayRepository.Add(WorkingDaydata);
            if (_uow.Save() <= 0) throw new Exception("Creating Holiday failed on save.");
            return Ok(WorkingDaydata.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] WorkingDayDto WorkingDay)
        {
            if (WorkingDay.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var WorkingDaydata = _mapper.Map<WorkingDay>(WorkingDay);
            _workingDayRepository.Update(WorkingDaydata);
            if (_uow.Save() <= 0) throw new Exception("Updating holiday failed on save.");
            return Ok(WorkingDaydata.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _workingDayRepository.Find(id);

            if (record == null)
                return NotFound();

            _workingDayRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _workingDayRepository.Find(id);

            if (record == null)
                return NotFound();

            _workingDayRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
