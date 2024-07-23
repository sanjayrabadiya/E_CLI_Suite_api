using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.XlsIO.Implementation.XmlReaders;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeekEndMasterController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IWeekEndMasterRepository _weekEndMasterRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public WeekEndMasterController(IUnitOfWork uow, IMapper mapper,
        IGSCContext context, IWeekEndMasterRepository weekEndMasterRepository, IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _weekEndMasterRepository = weekEndMasterRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var weekend = _weekEndMasterRepository.GetWeekendList(isDeleted);
            return Ok(weekend);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var weekend = _weekEndMasterRepository.Find(id);
            var weekendDto = _mapper.Map<WeekEndMasterDto>(weekend);
            weekendDto.SiteId = weekendDto.IsSite == true ? weekendDto.ProjectId : null;
            weekendDto.ProjectId = weekendDto.IsSite == true ? (int)_context.Project.Find(weekendDto.ProjectId).ParentProjectId : weekendDto.ProjectId;
            return Ok(weekendDto);
        }


        [HttpGet]
        [Route("GetweekendList/{id}")]
        public IActionResult GetweekendList(int id)
        {
            if (id <= 0) return BadRequest();
            var weekend = _weekEndMasterRepository.FindBy(x => x.ProjectId == id).FirstOrDefault();
            return Ok(weekend);
        }

        [HttpGet]
        [Route("GetWorkingDay/{studyPlanId}")]
        public IActionResult GetWorkingDay(int studyPlanId)
        {
            if (studyPlanId <= 0) return BadRequest();
            var project = _context.StudyPlan.Where(x => x.Id == studyPlanId).FirstOrDefault();
            if (project != null)
            {
                var workingDay = _weekEndMasterRepository.GetWorkingDayList(project.ProjectId);
                return Ok(workingDay);
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetWeekEndDay/{projectId}")]
        public IActionResult GetWeekEndDay(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var weekendDay = _weekEndMasterRepository.GetWeekEndDay(ProjectId);
            return Ok(weekendDay);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<WeekEndMasterDto> weekEndMasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //Only Site Right Add but not Add study right this time check validation #1327
            if (weekEndMasterDto[0].ProjectId != null && weekEndMasterDto[0].SiteId == null && _context.UserAccess.Where(x => x.ParentProjectId == weekEndMasterDto[0].ProjectId && x.ProjectId == weekEndMasterDto[0].ProjectId && x.DeletedBy == null).Count() == 0)
            {
                ModelState.AddModelError("Message", "You have only a site right. Not Study");
                return BadRequest(ModelState);
            }
            foreach (var item in weekEndMasterDto)
            {
                item.Id = 0;
                var weekend = _mapper.Map<WeekEndMaster>(item);
                weekend.IpAddress = _jwtTokenAccesser.IpAddress;
                weekend.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

                _weekEndMasterRepository.Add(weekend);
            }
            if (_uow.Save() <= 0) return Ok(new Exception("weekend is failed on save."));
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] List<WeekEndMasterDto> weekEndMasterDto)
        {
            //Only Site Right Add but not Add study right this time check validation #1327
            if (weekEndMasterDto[0].ProjectId != null && weekEndMasterDto[0].SiteId == null && _context.UserAccess.Where(x => x.ParentProjectId == weekEndMasterDto[0].ProjectId && x.ProjectId == weekEndMasterDto[0].ProjectId && x.DeletedBy == null).Count() == 0)
            {
                ModelState.AddModelError("Message", "You have only a site right. Not Study");
                return BadRequest(ModelState);
            }

            foreach (var item in weekEndMasterDto)
            {
                if (item.Id <= 0) return BadRequest();
                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

                var weekend = _mapper.Map<WeekEndMaster>(item);
                weekend.IpAddress = _jwtTokenAccesser.IpAddress;
                weekend.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                _weekEndMasterRepository.Update(weekend);
            }
            if (_uow.Save() <= 0) return Ok(new Exception("Weekend is failed on save."));
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _weekEndMasterRepository.Find(id);

            if (record == null)
                return NotFound();

            _weekEndMasterRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _weekEndMasterRepository.Find(id);

            if (record == null)
                return NotFound();

            _weekEndMasterRepository.Active(record);
            _uow.Save();

            return Ok();
        }

    }
}
