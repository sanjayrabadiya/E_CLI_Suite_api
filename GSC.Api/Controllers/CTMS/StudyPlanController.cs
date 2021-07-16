﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IGSCContext _context;

        public StudyPlanController(IUnitOfWork uow, IMapper mapper,
            IStudyPlanRepository studyPlanRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _studyPlanRepository = studyPlanRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var studyplan = _studyPlanRepository.GetStudyplanList(isDeleted);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var studyplan = _studyPlanRepository.Find(id);
            var studyplandetail = _mapper.Map<StudyPlanDto>(studyplan);
            return Ok(studyplandetail);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] StudyPlanDto studyplanDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var lstStudyPlan = new List<StudyPlanDto>();
            lstStudyPlan.Add(studyplanDto);

            var TaskMaster = _context.TaskMaster.Where(x => x.TaskTemplateId == studyplanDto.TaskTemplateId).Any(x => x.RefrenceType == Helper.RefrenceType.Sites || x.RefrenceType == Helper.RefrenceType.Sites);
            if (TaskMaster)
            {
                var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == studyplanDto.ProjectId).ToList();
                sites.ForEach(s =>
                {
                    var data = new StudyPlanDto();
                    data.StartDate = studyplanDto.StartDate;
                    data.EndDate = studyplanDto.EndDate;
                    data.ProjectId = s.Id;
                    data.TaskTemplateId = studyplanDto.TaskTemplateId;
                    lstStudyPlan.Add(data);
                });
            }

            foreach (var item in lstStudyPlan)
            {
                item.Id = 0;
                var studyplan = _mapper.Map<StudyPlan>(item);
                var validatecode = _studyPlanRepository.Duplicate(studyplan);
                if (!string.IsNullOrEmpty(validatecode))
                {
                    ModelState.AddModelError("Message", validatecode);
                    return BadRequest(ModelState);
                }
                _studyPlanRepository.Add(studyplan);
                if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");


                var validate = _studyPlanRepository.ImportTaskMasterData(studyplan);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    //_studyPlanRepository.Remove(studyplan);
                    //_uow.Save();
                    return BadRequest(ModelState);
                }
                //_uow.Save();
            }
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanDto studyplanDto)
        {
            if (studyplanDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var studyplan = _mapper.Map<StudyPlan>(studyplanDto);
            var validatecode = _studyPlanRepository.Duplicate(studyplan);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            _studyPlanRepository.Update(studyplan);
            if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");
            return Ok(studyplan.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanRepository.Find(id);

            var AllProject = _context.Project.Where(x => x.DeletedDate == null && (x.ParentProjectId == record.ProjectId || x.Id == record.ProjectId)).ToList();
            foreach (var item in AllProject)
            {
                var data = _studyPlanRepository.FindByInclude(x => x.DeletedDate == null && x.ProjectId == item.Id).FirstOrDefault();
                if (data == null)
                    return NotFound();

                _studyPlanRepository.Delete(data.Id);
            }
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanRepository.Find(id);

            var AllProject = _context.Project.Where(x => x.DeletedDate == null && (x.ParentProjectId == record.ProjectId || x.Id == record.ProjectId)).ToList();
            foreach (var item in AllProject)
            {
                var data = _studyPlanRepository.FindByInclude(x => x.DeletedDate != null && x.ProjectId == item.Id).FirstOrDefault();
                if (data == null)
                    return NotFound();
                var validatecode = _studyPlanRepository.Duplicate(data);
                if (!string.IsNullOrEmpty(validatecode))
                {
                    ModelState.AddModelError("Message", validatecode);
                    return BadRequest(ModelState);
                }
                _studyPlanRepository.Active(data);
            }
            _uow.Save();

            return Ok();
        }


    }
}
