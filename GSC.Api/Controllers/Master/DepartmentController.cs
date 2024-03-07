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
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DepartmentController(IDepartmentRepository departmentRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var departments = _departmentRepository.GetDepartmentList(isDeleted);
            return Ok(departments);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var department = _departmentRepository.Find(id);
            var departmentDto = _mapper.Map<DepartmentDto>(department);
            return Ok(departmentDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] DepartmentDto departmentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            departmentDto.Id = 0;
            var clientType = _mapper.Map<Department>(departmentDto);
            var validate = _departmentRepository.Duplicate(clientType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _departmentRepository.Add(clientType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Client Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(clientType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DepartmentDto departmentDto)
        {
            if (departmentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var department = _mapper.Map<Department>(departmentDto);
            department.Id = departmentDto.Id;
            var validate = _departmentRepository.Duplicate(department);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _departmentRepository.AddOrUpdate(department);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Department failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(department.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _departmentRepository.Find(id);

            if (record == null)
                return NotFound();

            _departmentRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _departmentRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _departmentRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _departmentRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDepartmentDropDown")]
        public IActionResult GetDepartmentDropDown()
        {
            return Ok(_departmentRepository.GetDepartmentDropDown());
        }
    }
}