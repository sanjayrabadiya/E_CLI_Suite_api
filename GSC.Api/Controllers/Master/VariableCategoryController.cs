using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VariableCategoryController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IVariableCategoryRepository _variableCategoryRepository;

        public VariableCategoryController(IVariableCategoryRepository variableCategoryRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableCategoryRepository = variableCategoryRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var variableCategorys = _variableCategoryRepository.All.Where(x =>x.IsDeleted == isDeleted
            ).OrderByDescending(x => x.Id).ToList();
            var variableCategorysDto = _mapper.Map<IEnumerable<VariableCategoryDto>>(variableCategorys);
            variableCategorysDto.ForEach(b =>
            {
                if (b.CreatedBy != null)
                    b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });

            return Ok(variableCategorysDto);
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
            if (_uow.Save() <= 0) throw new Exception("Creating Variable Category failed on save.");
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

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(variableCategory.Id);
            variableCategory.Id = 0;
            _variableCategoryRepository.Add(variableCategory);

            if (_uow.Save() <= 0) throw new Exception("Updating Variable Category failed on save.");
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