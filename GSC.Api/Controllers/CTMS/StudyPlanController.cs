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
    public class StudyPlanController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IGSCContext _context;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;
        private readonly ITaskMasterRepository _taskMasterRepository;

        public StudyPlanController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IStudyPlanRepository studyPlanRepository, IGSCContext context, IStudyPlanTaskRepository studyPlanTaskRepository, ITaskMasterRepository taskMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _studyPlanRepository = studyPlanRepository;
            _context = context;
            _studyPlanTaskRepository = studyPlanTaskRepository;
            _taskMasterRepository = taskMasterRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var studyplan = _studyPlanRepository.GetStudyplanList(isDeleted);
            return Ok(studyplan);                      
        }


        [HttpPost]
        public IActionResult Post([FromBody] StudyPlanDto studyplanDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyplanDto.Id = 0;
            var studyplan = _mapper.Map<StudyPlan>(studyplanDto);  
            _studyPlanRepository.Add(studyplan);
            if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");

            var taskdata= _taskMasterRepository.FindBy(x => x.TaskTemplateId == studyplanDto.TaskTemplateId).ToList();
            
            foreach (var item in taskdata)
            {
                var task = _mapper.Map<StudyPlanTask>(item);
                task.StudyPlanId = studyplan.Id;
                task.StartDate = DateTime.Now;
                task.EndDate = DateTime.Now;
                task.isMileStone = true;
                task.Progress = 80;
                //_studyPlanTaskRepository.Add(task);
                _uow.Save();
            }
            return Ok(studyplan.Id);
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanRepository.Find(id);
            if (record == null)
                return NotFound();
            //var validate = _studyTrackerTemplateRepository.Duplicate(record);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            _studyPlanRepository.Active(record);
            _uow.Save();

            return Ok();
        }


    }
}
