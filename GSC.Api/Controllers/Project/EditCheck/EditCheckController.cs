using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.EditCheck
{
    [Route("api/[controller]")]
    public class EditCheckController : BaseController
    {
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IEditCheckRepository _editCheckRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IEditCheckRuleRepository _editCheckRuleRepository;
        public EditCheckController(
            IUnitOfWork uow, IMapper mapper, IEditCheckRepository editCheckRepository,
            IEditCheckDetailRepository editCheckDetailRepository,
            IEditCheckRuleRepository editCheckRuleRepository,
            IProjectDesignRepository projectDesignRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _editCheckRepository = editCheckRepository;
            _editCheckDetailRepository = editCheckDetailRepository;
            _projectDesignRepository = projectDesignRepository;
            _editCheckRuleRepository = editCheckRuleRepository;
        }

        [HttpGet("{Id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            return Ok(_editCheckRepository.GetAll(id, isDeleted));
        }

        [HttpGet("GetEditCheckDetail/{id}/{isDeleted:bool?}")]
        public IActionResult GetEditCheckDetail(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();
            var result = _editCheckRepository.GetEditCheckDetail(id, isDeleted);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EditCheckDto editCheck)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectEditCheck = _mapper.Map<Data.Entities.Project.EditCheck.EditCheck>(editCheck);
            projectEditCheck.Id = 0;
            _editCheckRepository.SaveEditCheck(projectEditCheck);
             _uow.Save();
            return Ok(_editCheckRepository.GetAll(editCheck.ProjectDesignId, false));
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _editCheckRepository.Find(id);
            if (record == null)
                return NotFound();

            _editCheckRepository.Delete(record);


            var details = _editCheckDetailRepository.FindBy(x => x.EditCheckId == id).ToList();
            details.ForEach(x => { _editCheckDetailRepository.Delete(x); });

            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _editCheckRepository.Find(id);

            if (record == null)
                return NotFound();

            _editCheckRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("checkEditCheckLocked/{projectDesignId}")]
        public IActionResult checkEditCheckLocked(int projectDesignId)
        {
            if (projectDesignId <= 0) return BadRequest();
            var projectDesign = _projectDesignRepository
                .FindBy(t => t.Id == projectDesignId && t.DeletedDate == null).FirstOrDefault();

            var projectDesignDto = _mapper.Map<ProjectDesignDto>(projectDesign);
            if (projectDesign != null) projectDesignDto.Locked = !projectDesign.IsUnderTesting;

            return Ok(projectDesignDto);
        }

        [HttpPost("CopyTo/{id}")]
        public IActionResult CopyTo(int id)
        {
            var editCheck = _editCheckRepository.CopyTo(id);
             _uow.Save() ;
            return Ok(_editCheckRepository.GetAll(editCheck.ProjectDesignId, false));
        }


        [HttpPost("ValidateEditCheck")]
        public IActionResult ValidateEditCheck([FromBody] List<EditCheckValidate> editCheck)
        {
            return Ok(_editCheckRuleRepository.ValidateEditCheck(editCheck));
        }
    }
}