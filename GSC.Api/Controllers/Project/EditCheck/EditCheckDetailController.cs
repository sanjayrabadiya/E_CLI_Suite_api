using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Domain.Context;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Project.EditCheck;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.EditCheck
{
    [Route("api/[controller]")]
    public class EditCheckDetailController : BaseController
    {
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IEditCheckRepository _editCheckRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IImpactService _impactService;
        public EditCheckDetailController(IEditCheckDetailRepository editCheckDetailRepository,
            IUnitOfWork uow, IMapper mapper, IEditCheckRepository editCheckRepository, IImpactService impactService)
        {
            _editCheckDetailRepository = editCheckDetailRepository;
            _uow = uow;
            _mapper = mapper;
            _editCheckRepository = editCheckRepository;
            _impactService = impactService;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var drugs = _editCheckDetailRepository.All.Where(x =>
                isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(t => t.Id).ToList();
            var drugsDto = _mapper.Map<IEnumerable<EditCheckDetailDto>>(drugs);
            return Ok(drugsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var result = _editCheckDetailRepository.GetDetailById(id);
            return Ok(result);
        }


        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] EditCheckDetailDto editCheckDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            editCheckDetailDto.Id = 0;
            EditCheckDetail editCheckDetail = null;

            if (editCheckDetailDto.VariableIds.Length != 0)
            {
                for (var i = 0; i < editCheckDetailDto.VariableIds.Length; i++)
                {
                    editCheckDetailDto.ProjectDesignVariableId = editCheckDetailDto.VariableIds[i];
                    editCheckDetail = _mapper.Map<EditCheckDetail>(editCheckDetailDto);

                    var validateMsg = _editCheckDetailRepository.Validate(editCheckDetail);
                    if (!string.IsNullOrEmpty(validateMsg))
                    {
                        return BadRequest(validateMsg);
                    }

                    if (editCheckDetailDto.VariableIds.Length > 1 && !editCheckDetail.IsTarget && editCheckDetailDto.VariableIds.Length - 1 == i)
                    {
                        editCheckDetail.Operator = null;
                        editCheckDetail.LogicalOperator = null;
                    }


                    if (editCheckDetail.CheckBy == Helper.EditCheckRuleBy.ByVariable)
                        editCheckDetail.CollectionValue = _impactService.GetProjectDesignVariableId(editCheckDetailDto.ProjectDesignVariableId ?? 0, editCheckDetail.CollectionValue);

                    _editCheckDetailRepository.Add(editCheckDetail);

                }
                _uow.Save();
            }
            else
            {
                editCheckDetail = _mapper.Map<EditCheckDetail>(editCheckDetailDto);
                _editCheckDetailRepository.Add(editCheckDetail);
                _uow.Save();
                _editCheckDetailRepository.UpdateEditDetail(editCheckDetail);
            }
            _uow.Save();
            _editCheckRepository.UpdateFormula(editCheckDetailDto.EditCheckId);
            _editCheckDetailRepository.UpdateEditDetail(editCheckDetail);
            return Ok(_editCheckRepository.GetEditCheckDetail(editCheckDetailDto.EditCheckId, true));
        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] EditCheckDetailDto editCheckDetailDto)
        {
            if (editCheckDetailDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var editCheckDetail = _mapper.Map<EditCheckDetail>(editCheckDetailDto);
            if (editCheckDetail.IsTarget)
            {
                editCheckDetail.StartParens = "";
                editCheckDetail.EndParens = "";
            }
            var validateMsg = _editCheckDetailRepository.Validate(editCheckDetail);
            if (!string.IsNullOrEmpty(validateMsg))
            {
                return BadRequest(validateMsg);
            }

            _editCheckDetailRepository.Update(editCheckDetail);

            _uow.Save();
            _editCheckDetailRepository.UpdateEditDetail(editCheckDetail);
            _editCheckRepository.UpdateFormula(editCheckDetailDto.EditCheckId);
            return Ok(_editCheckRepository.GetEditCheckDetail(editCheckDetailDto.EditCheckId, true));
        }


        [HttpDelete("{id}")]
        [TransactionRequired]
        public ActionResult Delete(int id)
        {
            var record = _editCheckDetailRepository.Find(id);
            if (record == null)
                return NotFound();

            _editCheckDetailRepository.Delete(record);
            _uow.Save();

            _editCheckRepository.UpdateFormula(record.EditCheckId);
            return Ok();
        }


        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _editCheckDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            _editCheckDetailRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}