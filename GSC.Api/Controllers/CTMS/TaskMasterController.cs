using System;
using System.Linq;
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
    [ApiController]
    public class TaskMasterController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ITaskMasterRepository _taskMasterRepository;
        private readonly IGSCContext _context;

        public TaskMasterController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, ITaskMasterRepository taskMasterRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _taskMasterRepository = taskMasterRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{templateId:int}")]
        public IActionResult Get(bool isDeleted, int templateId)
        {
            var tasklist = _taskMasterRepository.GetTasklist(isDeleted, templateId);
            return Ok(tasklist);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var taskmaster = _taskMasterRepository.FindByInclude(x => x.Id == id, x => x.RefrenceTypes).FirstOrDefault();

            if (taskmaster != null && taskmaster.RefrenceTypes != null)
                taskmaster.RefrenceTypes = taskmaster.RefrenceTypes.Where(x => x.DeletedDate == null).ToList();

            var taskDto = _mapper.Map<TaskmasterDto>(taskmaster);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TaskmasterDto taskmasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            taskmasterDto.Id = 0;
            var tastMaster = _mapper.Map<TaskMaster>(taskmasterDto);

            if (tastMaster.IsMileStone || tastMaster.Duration == 0)
            {
                tastMaster.Duration = 0;
                tastMaster.IsMileStone = true;
            }
            tastMaster.TaskOrder = _taskMasterRepository.UpdateTaskOrder(taskmasterDto);
            _taskMasterRepository.Add(tastMaster);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating Task failed on save."));
            taskmasterDto.Id = tastMaster.Id;
            _taskMasterRepository.AddRefrenceTypes(taskmasterDto);
            //this comment not remove add by mitul on 0805-2024
            //_taskMasterRepository.AddTaskToSTudyPlan(taskmasterDto);
            return Ok(tastMaster.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TaskmasterDto taskmasterDto)
        {
            if (taskmasterDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskmaster = _mapper.Map<TaskMaster>(taskmasterDto);
            if (taskmaster.IsMileStone || taskmaster.Duration == 0)
            {
                taskmaster.Duration = 0;
                taskmaster.IsMileStone = true;
            }
            UpdateRefrenceTypes(taskmaster);
            _taskMasterRepository.Update(taskmaster);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating Task Master failed on save."));
            //this comment not remove add by mitul on 0805-2024
            // _taskMasterRepository.AddTaskToSTudyPlan(taskmasterDto);
            return Ok(taskmaster.Id);
        }

        private void UpdateRefrenceTypes(TaskMaster taskmaster)
        {
            var refrenceType = _context.RefrenceTypes.Where(x => x.TaskMasterId == taskmaster.Id && x.DeletedDate == null)
               .ToList();

            refrenceType.ForEach(t =>
            {
                if (refrenceType != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.RefrenceTypes.Update(t);
                }
            });

            taskmaster.RefrenceTypes.ForEach(z =>
            {
                _context.RefrenceTypes.Add(z);
            });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _taskMasterRepository.Find(id);

            var parenttask = _taskMasterRepository.FindBy(x => x.ParentId == id);
            foreach (var task in parenttask)
            {
                if (record == null)
                    return NotFound();

                _taskMasterRepository.Delete(task);
            }
            var refrenceType = _context.RefrenceTypes.Where(x => x.TaskMasterId == id && x.DeletedDate == null).ToList();
            refrenceType.ForEach(t =>
            {
                if (refrenceType != null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.RefrenceTypes.Update(t);
                }
            });
            _taskMasterRepository.Delete(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet("ConverttoMileStone/{id}")]
        public IActionResult ConverttoMileStone(int id)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _taskMasterRepository.Find(id);
            milestonetask.IsMileStone = true;
            milestonetask.Duration = 0;
            _taskMasterRepository.Update(milestonetask);
            _uow.Save();
            return Ok(id);
        }

        [HttpGet("ConverttoTask/{id}")]
        public IActionResult ConverttoTask(int id)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _taskMasterRepository.Find(id);
            milestonetask.IsMileStone = false;
            milestonetask.Duration = 1;
            _taskMasterRepository.Update(milestonetask);
            _uow.Save();
            return Ok(id);
        }

        [HttpGet("GetTaskHistory/{id}")]
        public IActionResult GetTaskHistory(int id)
        {
            if (id <= 0) return BadRequest();

            var result = _taskMasterRepository.GetTaskHistory(id);
            return Ok(result);
        }
        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _taskMasterRepository.Find(id);
            if (record == null)
                return NotFound();
            _taskMasterRepository.Active(record);
            _uow.Save();

            var refrenceType = _context.RefrenceTypes.Where(x => x.TaskMasterId == id && x.DeletedDate != null).ToList();
            refrenceType.ForEach(t =>
            {
                if (refrenceType != null)
                {
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    _context.RefrenceTypes.Update(t);
                    _uow.Save();
                }
            });
         
            return Ok();
        }
    }
}
