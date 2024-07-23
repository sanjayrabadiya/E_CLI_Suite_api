using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
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
            var taskmaster = _workingDayRepository.FindByInclude(x => x.Id == id, x => x.siteTypes).FirstOrDefault();
            if (taskmaster != null && taskmaster.siteTypes != null)
                taskmaster.siteTypes = taskmaster.siteTypes.Where(x => x.DeletedDate == null).ToList();
            var taskDto = _mapper.Map<WorkingDayDto>(taskmaster);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] WorkingDayDto WorkingDay)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            WorkingDay.Id = 0;
            var WorkingDaydata = _mapper.Map<WorkingDay>(WorkingDay);
            WorkingDaydata.IpAddress = _jwtTokenAccesser.IpAddress;
            WorkingDaydata.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _workingDayRepository.Add(WorkingDaydata);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Holiday failed on save."));
            WorkingDay.Id = WorkingDaydata.Id;
            _workingDayRepository.AddSiteType(WorkingDay);
            return Ok(WorkingDaydata.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] WorkingDayDto WorkingDay)
        {
            if (WorkingDay.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var WorkingDaydata = _mapper.Map<WorkingDay>(WorkingDay);
            WorkingDaydata.IpAddress = _jwtTokenAccesser.IpAddress;
            WorkingDaydata.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            UpdateSiteType(WorkingDaydata);
            _workingDayRepository.Update(WorkingDaydata);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating holiday failed on save."));
            return Ok(WorkingDaydata.Id);
        }
        private void UpdateSiteType(WorkingDay workingDay)
        {
            var SiteTyp = _context.SiteTypes.Where(x => x.WorkingDayId == workingDay.Id && x.DeletedDate == null)
               .ToList();

            SiteTyp.ForEach(t =>
            {
                if (SiteTyp != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.SiteTypes.Update(t);
                }
            });
            workingDay.siteTypes.ForEach(z =>
            {
                _context.SiteTypes.Add(z);
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _workingDayRepository.Find(id);
            if (record == null)
                return NotFound();
            _workingDayRepository.Delete(record);

            var siteTyp = _context.SiteTypes.Where(x => x.WorkingDayId == id && x.DeletedDate == null).ToList();
            siteTyp.ForEach(t =>
            {
                if (siteTyp != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.SiteTypes.Update(t);
                }
            });
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
