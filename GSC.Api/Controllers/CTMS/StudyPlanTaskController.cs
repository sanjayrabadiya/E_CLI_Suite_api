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

        [HttpGet("{isDeleted:bool?}/{StudyPlanId:int}")]
        public IActionResult Get(bool isDeleted, int StudyPlanId)
        {
            var studyplan = _studyPlanTaskRepository.GetStudyPlanTaskList(isDeleted, StudyPlanId);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _studyPlanTaskRepository.FindByInclude(x => x.Id == id).FirstOrDefault();
            var taskDto = _mapper.Map<StudyPlanTaskDto>(task);
            //var datetime = WorkingDayHelper.GetNextWorkingDay(DateTime.Now, 4);
            return Ok(taskDto);
        }



        [HttpPost]
        public IActionResult Post([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            taskmasterDto.Id = 0;
            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);

           // var validate = _studyPlanTaskRepository.ValidateTask(tastMaster);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            //if (taskmasterDto.IsMileStone || taskmasterDto.StartDate.Date == taskmasterDto.EndDate.Date)
            //{
            //    tastMaster.isMileStone = true;
            //    tastMaster.EndDate = taskmasterDto.StartDate;
            //}
            tastMaster.TaskOrder = _studyPlanTaskRepository.UpdateTaskOrder(taskmasterDto);
            //tastMaster.Duration = Duration(taskmasterDto.StartDate, taskmasterDto.EndDate);          
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
            string mvalidate= _studyPlanTaskRepository.UpdateDependentTask(taskmasterDto.StudyPlanId);
            if (!string.IsNullOrEmpty(mvalidate))
            {
                ModelState.AddModelError("Message", mvalidate);
                _studyPlanTaskRepository.Remove(tastMaster);
                _uow.Save();
                return BadRequest(ModelState);
            }

          //  if (_uow.Save() <= 0) throw new Exception("Creating Task failed on save.");
          //  if (taskmasterDto.ParentId > 0)
                // _studyPlanTaskRepository.UpdateParentDate(taskmasterDto.ParentId);

              //  _studyPlanTaskRepository.InsertDependentTask(taskmasterDto.DependentTask, tastMaster.Id);
            _studyPlanTaskRepository.UpdateTaskOrderSequence(taskmasterDto.Id);
            return Ok(tastMaster.Id);
        }

        //private int Duration(DateTime d1, DateTime d2)
        //{
        //    TimeSpan span = d2.Subtract(d1);
        //    return (int)span.TotalDays;
        //}

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (taskmasterDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);
            //var validate = _studyPlanTaskRepository.ValidateTask(tastMaster);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            //if (taskmasterDto.IsMileStone || taskmasterDto.StartDate.Date == taskmasterDto.EndDate.Date)
            //{
            //    tastMaster.isMileStone = true;
            //    tastMaster.EndDate = taskmasterDto.StartDate;
            //}
            //tastMaster.TaskOrder = _studyPlanTaskRepository.UpdateTaskOrder(taskmasterDto);
            // tastMaster.Duration = Duration(taskmasterDto.StartDate, taskmasterDto.EndDate);
            var data= _studyPlanTaskRepository.UpdateDependentTaskDate(tastMaster);
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
            // if (taskmasterDto.ParentId > 0)
            // _studyPlanTaskRepository.UpdateParentDate(taskmasterDto.ParentId);
            //     _studyPlanTaskRepository.InsertDependentTask(taskmasterDto.DependentTask, tastMaster.Id);

            //_studyPlanTaskRepository.UpdateTaskOrderSequence(taskmasterDto.StudyPlanId);
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

    }
}
