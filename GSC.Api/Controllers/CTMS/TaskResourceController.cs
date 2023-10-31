using System;
using System.Linq.Dynamic.Core;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class TaskResourceController : BaseController
    {

        private readonly ITaskResourceRepository _taskResourceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public TaskResourceController(ITaskResourceRepository taskResourceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IGSCContext context)
        {
            _taskResourceRepository = taskResourceRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}/{PlanTaskId}")]
        public IActionResult Get(bool isDeleted, int PlanTaskId)
        {
            var taskResource = _taskResourceRepository.GetTaskResourceList(isDeleted, PlanTaskId);
            return Ok(taskResource);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var taskResource = _taskResourceRepository.Find(id);
            var taskResourceDto = _mapper.Map<TaskResourceDto>(taskResource);
            return Ok(taskResourceDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TaskResourceDto taskResourceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            taskResourceDto.Id = 0;

            //taskResourceDto.ResourceTypeId = _context.ResourceType.Include(s => s.Designation).Where(x => x.DeletedBy == null && ((int)x.ResourceTypes) == taskResourceDto.resource && ((int)x.ResourceSubType) == taskResourceDto.subresource && x.DesignationId== taskResourceDto.designation)
            //                                 .Select(c =>c.Id ).FirstOrDefault();


            var taskResource = _mapper.Map<TaskResource>(taskResourceDto);
            var validate = _taskResourceRepository.Duplicate(taskResource);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _taskResourceRepository.Add(taskResource);

            if (_uow.Save() <= 0) throw new Exception("Creating Resource failed on save.");

            //Add by mitul task was Resource Add in StudyPlanTask && Apply validation User Access
            var studyPlanTaskData = _context.StudyPlanTask.Include(e => e.StudyPlan).Where(d => d.TaskId == taskResource.TaskMasterId && d.DeletedDate == null).ToList();
            if (studyPlanTaskData.Count > 0)
            {
                var userAccessData = _context.UserAccess.Include(x => x.UserRole).Where(s => s.DeletedBy == null && studyPlanTaskData.Select(y => y.StudyPlan.ProjectId).Contains(s.ProjectId)).ToList();
                if (userAccessData.Count > 0)
                {
                    var resourceTypedata = _context.ResourceType.Where(s => s.Id == taskResourceDto.ResourceTypeId && s.DeletedBy == null && userAccessData.Select(y => y.UserRole.UserId).Contains(s.UserId)).FirstOrDefault();
                    if (resourceTypedata != null)
                    {
                        var studyPlanTask = _context.StudyPlanTask.Include(e => e.StudyPlan).Where(d => d.TaskId == taskResource.TaskMasterId && d.DeletedDate == null && userAccessData.Select(y => y.ProjectId).Contains(d.StudyPlan.ProjectId))
                            .Select(t => new StudyPlanResource
                            {
                                StudyPlanTaskId = t.Id,
                                ResourceTypeId = taskResource.ResourceTypeId
                            }).ToList();
                        _context.StudyPlanResource.AddRange(studyPlanTask);
                        _context.Save();
                    }
                }
            }

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] TaskResourceDto taskResourceDto)
        {
            if (taskResourceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskResource = _mapper.Map<TaskResource>(taskResourceDto);
            var validate = _taskResourceRepository.Duplicate(taskResource);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _taskResourceRepository.Update(taskResource);

            if (_uow.Save() <= 0) throw new Exception("Updating Resource failed on save.");
            return Ok(taskResource.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _taskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            _taskResourceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _taskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _taskResourceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _taskResourceRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}