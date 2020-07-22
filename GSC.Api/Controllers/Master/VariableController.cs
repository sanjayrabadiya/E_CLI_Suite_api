using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Custom;
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
    public class VariableController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableValueRepository _variableValueRepository;

        public VariableController(IVariableRepository variableRepository,
            IVariableValueRepository variableValueRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableRepository = variableRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _variableValueRepository = variableValueRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var variables = _variableRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var variablesDto = _mapper.Map<IEnumerable<VariableDto>>(variables);
            variablesDto.ForEach(b =>
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
            return Ok(variablesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variable = _variableRepository.FindByInclude(t => t.Id == id, t => t.Values).FirstOrDefault();

            //if (variable.Values != null)
            //    variable.Values = variable.Values.Where(x => x.DeletedDate == null).ToList();

            var variableDto = _mapper.Map<VariableDto>(variable);
            return Ok(variableDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] VariableDto variableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            variableDto.Id = 0;
            var variable = _mapper.Map<Variable>(variableDto);
            var validate = _variableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableRepository.Add(variable);
            if (_uow.Save() <= 0) throw new Exception("Creating Variable failed on save.");
            return Ok(variable.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableDto variableDto)
        {
            if (variableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            variableDto.Values.Where(s => s.VariableId == null || s.VariableId == 0)
                .Select(s =>
                {
                    s.VariableId = variableDto.Id;
                    s.Id = 0;
                    return s;
                }).ToList();
            var variable = _mapper.Map<Variable>(variableDto);
            var validate = _variableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            UpdateVariableValues(variable);
            _variableRepository.Update(variable);

            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(variable.Id);
        }

        private void UpdateVariableValues(Variable variable)
        {
            var deleteValues = _variableValueRepository.FindBy(x => x.VariableId == variable.Id
                                                                    && !variable.Values.Any(c => c.Id == x.Id))
                .ToList();
            foreach (var value in deleteValues)
                _variableValueRepository.Remove(value);

        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.SystemType != null)
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            _variableRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _variableRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _variableRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVariableDropDown")]
        public IActionResult GetVariableDropDown()
        {
            return Ok(_variableRepository.GetVariableDropDown());
        }

        [HttpGet]
        [Route("GetVariableListByDomainId/{domainId}")]
        public IActionResult GetVariableListByDomainId(int domainId)
        {
            return Ok(_variableRepository.GetVariableListByDomainId(domainId));
        }

        [HttpGet]
        [Route("GetColumnList/{TableName}")]
        public IActionResult GetColumnList(string tableName)
        {
            //var result = _variableRepository.GetColumnName("");

            var sqlquery =
                "select ORDINAL_POSITION AS Id, COLUMN_NAME AS valueName, '' AS valueCode   from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME='" +
                tableName + "' AND Data_type = 'nvarchar'";
            var resulttt = _uow.FromSql<CustomTable>(sqlquery).ToList();

            var dtlist = new List<DropDownDto>();
            for (var i = 0; i < resulttt.Count; i++)
            {
                var dt = new DropDownDto();

                dt.Value = resulttt[i].ValueName;
                dt.Id = resulttt[i].Id;
                dtlist.Add(dt);
            }

            return Ok(dtlist);
        }

        [HttpGet]
        [Route("GetDataFromTableAndColumn/{TableName}/{ColumnName}/{ColumnCode}")]
        public IActionResult GetDataFromTableAndColumn(string tableName, string columnName, string columnCode)
        {
            var sqlquery = "SELECT Id," + columnName + " AS valueName," + columnCode + " AS valueCode  FROM " +
                           tableName + " WHERE DeletedDate IS null";
            var resulttt = _uow.FromSql<CustomTable>(sqlquery).ToList();
            return Ok(resulttt);
        }
    }
}