using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VariableCategoryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableCategoryRepository _variableCategoryRepository;

        public VariableCategoryController(IVariableCategoryRepository variableCategoryRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _variableCategoryRepository = variableCategoryRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var variablecategories = _variableCategoryRepository.GetVariableCategoryList(isDeleted);
            return Ok(variablecategories);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableCategory = _variableCategoryRepository.Find(id);
            var variableCategoryDto = _mapper.Map<VariableCategoryDto>(variableCategory);
            return Ok(variableCategoryDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] VariableCategoryDto variableCategoryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            variableCategoryDto.Id = 0;
            var variableCategory = _mapper.Map<VariableCategory>(variableCategoryDto);
            var validate = _variableCategoryRepository.Duplicate(variableCategory);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableCategoryRepository.Add(variableCategory);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Variable Category failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(variableCategory.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableCategoryDto variableCategoryDto)
        {
            if (variableCategoryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var variableCategory = _mapper.Map<VariableCategory>(variableCategoryDto);
            var validate = _variableCategoryRepository.Duplicate(variableCategory);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _variableCategoryRepository.AddOrUpdate(variableCategory);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Variable Category failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(variableCategory.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableCategoryRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.SystemType != null)
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            _variableCategoryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _variableCategoryRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _variableCategoryRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableCategoryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVariableCategoryDropDown")]
        public IActionResult GetVariableCategoryDropDown()
        {
            return Ok(_variableCategoryRepository.GetVariableCategoryDropDown());
        }
    }
}