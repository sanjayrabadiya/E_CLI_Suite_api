using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class WeekEndMasterController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IWeekEndMasterRepository _weekEndMasterRepository;

        public WeekEndMasterController(IUnitOfWork uow, IMapper mapper,
        IJwtTokenAccesser jwtTokenAccesser, IGSCContext context, IWeekEndMasterRepository weekEndMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _weekEndMasterRepository = weekEndMasterRepository;
        }


        [HttpGet]
        [Route("getweekendList/{id}")]
        public IActionResult getweekendList(int id)
         {
            if (id <= 0) return BadRequest();
            var weekend = _weekEndMasterRepository.FindBy(x=>x.ProjectId==id).SingleOrDefault();
            //var holidayDto = _mapper.Map<HolidayMasterDto>(holiday);
            //if (weekend == null)
            //    return Ok();
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
        [Route("GetWeekEndDay/{studyPlanId}")]
        public IActionResult GetWeekEndDay(int studyPlanId)
        {
            if (studyPlanId <= 0) return BadRequest();
            int ProjectId = _context.StudyPlan.Where(x => x.Id == studyPlanId).FirstOrDefault().ProjectId;
            var weekendDay = _weekEndMasterRepository.GetweekEndDay(ProjectId);
            return Ok(weekendDay);
        }

        [HttpPost]
        public IActionResult Post([FromBody] WeekEndparameterDto parameterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            parameterDto.Id = 0;
            var weekend = _mapper.Map<WeekEndMaster>(parameterDto);
            //var validatecode = _weekEndMasterRepository.Duplicate(studyplan);
            //if (!string.IsNullOrEmpty(validatecode))
            //{
            //    ModelState.AddModelError("Message", validatecode);
            //    return BadRequest(ModelState);
            //}
            _weekEndMasterRepository.Add(weekend);
            if (_uow.Save() <= 0) throw new Exception("weekend is failed on save.");           
            return Ok(weekend.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] WeekEndparameterDto parameterDto)
        {
            if (parameterDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var weekend = _mapper.Map<WeekEndMaster>(parameterDto);
            //var validatecode = _studyPlanRepository.Duplicate(studyplan);
            //if (!string.IsNullOrEmpty(validatecode))
            //{
            //    ModelState.AddModelError("Message", validatecode);
            //    return BadRequest(ModelState);
            //}
            _weekEndMasterRepository.Update(weekend);
            if (_uow.Save() <= 0) throw new Exception("Weekend is failed on save.");
            return Ok(weekend.Id);
        }


    }
}
