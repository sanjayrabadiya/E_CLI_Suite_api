using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        public WeekEndMasterController(IUnitOfWork uow, IMapper mapper,
        IGSCContext context, IWeekEndMasterRepository weekEndMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _weekEndMasterRepository = weekEndMasterRepository;
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
            weekendDto.SiteId = weekendDto.IsSite == true ? weekendDto.ProjectId : (int?)null;
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
            int ProjectId = _context.StudyPlan.Where(x => x.Id == studyPlanId).FirstOrDefault().ProjectId;
            var workingDay = _weekEndMasterRepository.GetworkingDayList(ProjectId);
            return Ok(workingDay);
        }

        [HttpGet]
        [Route("GetWeekEndDay/{projectId}")]
        public IActionResult GetWeekEndDay(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var weekendDay = _weekEndMasterRepository.GetweekEndDay(ProjectId);
            return Ok(weekendDay);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<WeekEndMasterDto> weekEndMasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var item in weekEndMasterDto)
            {
                item.Id = 0;
                var weekend = _mapper.Map<WeekEndMaster>(item);

                _weekEndMasterRepository.Add(weekend);
            }
            if (_uow.Save() <= 0) throw new Exception("weekend is failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] List<WeekEndMasterDto> weekEndMasterDto)
        {
            foreach (var item in weekEndMasterDto)
            {
                if (item.Id <= 0) return BadRequest();
                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

                var weekend = _mapper.Map<WeekEndMaster>(item);

                _weekEndMasterRepository.Update(weekend);
            }
            if (_uow.Save() <= 0) throw new Exception("Weekend is failed on save.");
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
