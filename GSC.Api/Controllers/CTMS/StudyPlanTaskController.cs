using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanTaskController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;


        public StudyPlanTaskController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IStudyPlanRepository studyPlanRepository, IGSCContext context, IStudyPlanTaskRepository studyPlanTaskRepository, ITaskMasterRepository taskMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _studyPlanTaskRepository = studyPlanTaskRepository;
        }

        [HttpGet("{isDeleted:bool?}/{StudyPlanId:int}/{ProjectId:int}")]
        public IActionResult Get(bool isDeleted, int StudyPlanId,int ProjectId)
        {
            var studyplan = _studyPlanTaskRepository.GetStudyPlanTaskList(isDeleted, StudyPlanId, ProjectId);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _studyPlanTaskRepository.FindByInclude(x => x.Id == id).FirstOrDefault();
            var taskDto = _mapper.Map<StudyPlanTaskDto>(task);            
            return Ok(taskDto);
        }

        [HttpPost]       
        public IActionResult Post([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            taskmasterDto.Id = 0;
            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);           
            tastMaster.TaskOrder = _studyPlanTaskRepository.UpdateTaskOrder(taskmasterDto);            
            var data = _studyPlanTaskRepository.UpdateDependentTaskDate(tastMaster);
            if (data != null)
            {
                tastMaster.StartDate = data.StartDate;
                tastMaster.EndDate = data.EndDate;
            }
            var validate = _studyPlanTaskRepository.ValidateTask(tastMaster);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _studyPlanTaskRepository.Add(tastMaster);
            _uow.Save();
           //  var tasklist= _studyPlanTaskRepository.Save(tastMaster);
            //string mvalidate = _studyPlanTaskRepository.UpdateDependentTask(taskmasterDto.StudyPlanId);
            //if (!string.IsNullOrEmpty(mvalidate))
            //{
            //    ModelState.AddModelError("Message", mvalidate);
            //    _studyPlanTaskRepository.Remove(tastMaster);
            //    _uow.Save();
            //    return BadRequest(ModelState);
            //}            
            _studyPlanTaskRepository.UpdateTaskOrderSequence(taskmasterDto.Id);
            return Ok(tastMaster.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (taskmasterDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);          
            var data = _studyPlanTaskRepository.UpdateDependentTaskDate(tastMaster);
            if (data != null)
            {
                tastMaster.StartDate = data.StartDate;
                tastMaster.EndDate = data.EndDate;
            }
            //var validate = _studyPlanTaskRepository.ValidateTask(tastMaster);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            var revertdata = _studyPlanTaskRepository.Find(taskmasterDto.Id);
            _studyPlanTaskRepository.Update(tastMaster);
            if (_uow.Save() <= 0) throw new Exception("Updating Task failed on save.");
            string mvalidate = _studyPlanTaskRepository.UpdateDependentTask(taskmasterDto.Id);
            if (!string.IsNullOrEmpty(mvalidate))
            {
                ModelState.AddModelError("Message", mvalidate);
                _studyPlanTaskRepository.Update(revertdata);
                _uow.Save();
                return BadRequest(ModelState);
            }         
            return Ok(tastMaster.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanTaskRepository.Find(id);

            var parenttask = _studyPlanTaskRepository.FindBy(x => x.ParentId == id);
            foreach (var task in parenttask)
            {
                if (record == null)
                    return NotFound();
                var subtask = _studyPlanTaskRepository.FindBy(x => x.ParentId == task.Id).ToList();
                foreach (var sub in subtask)
                    _studyPlanTaskRepository.Delete(sub.Id);
                _studyPlanTaskRepository.Delete(task.Id);
            }
            _studyPlanTaskRepository.Delete(record.Id);
            _uow.Save();
            _studyPlanTaskRepository.UpdateTaskOrderSequence(record.StudyPlanId);
            return Ok();
        }

        [HttpGet("ConverttoMileStone/{id}")]
        public IActionResult ConverttoMileStone(int id)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _studyPlanTaskRepository.Find(id);
            milestonetask.isMileStone = true;
            milestonetask.EndDate = milestonetask.StartDate;
            milestonetask.Duration = 0;
            _studyPlanTaskRepository.Update(milestonetask);
            _uow.Save();
            _studyPlanTaskRepository.UpdateDependentTask(id);
            return Ok(id);
        }

        [HttpGet("ConverttoTask/{id}")]
        public IActionResult ConverttoTask(int id)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _studyPlanTaskRepository.Find(id);
            milestonetask.isMileStone = false;
            milestonetask.EndDate = WorkingDayHelper.GetNextWorkingDay(milestonetask.StartDate);
            milestonetask.Duration = 1;
            _studyPlanTaskRepository.Update(milestonetask);
            _uow.Save();
            _studyPlanTaskRepository.UpdateDependentTask(id);
            return Ok(id);
        }


        [HttpPost("GetNetxtworkingDate")]
        public IActionResult GetNetxtworkingDate([FromBody] NextWorkingDateParameterDto parameterDto)
        {
            if (parameterDto.StudyPlanId <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var validate = _studyPlanTaskRepository.ValidateweekEnd(parameterDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var nextworkingdate = _studyPlanTaskRepository.GetNextWorkingDate(parameterDto);
            return Ok(nextworkingdate);     
        }

    }
}
