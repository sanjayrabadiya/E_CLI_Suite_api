﻿using System;
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
    public class TaskMasterController : ControllerBase
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
        [HttpPost]
        public IActionResult Post([FromBody] TaskmasterDto taskmasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            taskmasterDto.Id = 0;
            var tastMaster = _mapper.Map<TaskMaster>(taskmasterDto);
            //var validate = _update.Duplicate(ResourceType);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}       
            tastMaster.TaskOrder = _taskMasterRepository.UpdateTaskOrder(taskmasterDto);
            _taskMasterRepository.Add(tastMaster);         

            if (_uow.Save() <= 0) throw new Exception("Creating Task failed on save.");
            return Ok(tastMaster.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TaskmasterDto taskmasterDto)
        {
            if (taskmasterDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskmaster = _mapper.Map<TaskMaster>(taskmasterDto);
            _taskMasterRepository.Update(taskmaster);
            if (_uow.Save() <= 0) throw new Exception("Updating Task Master failed on save.");

            return Ok(taskmaster.Id);
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
            _taskMasterRepository.Delete(record);
            _uow.Save();

            return Ok();
        }



    }
}