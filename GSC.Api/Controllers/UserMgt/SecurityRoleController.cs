using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class SecurityRoleController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IRoleRepository _securityRoleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _uow;

        public SecurityRoleController(IRoleRepository securityRoleRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser,
            IUserRoleRepository userRoleRepository)
        {
            _securityRoleRepository = securityRoleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRoleRepository = userRoleRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var securityRoles = _securityRoleRepository.GetSecurityRolesList(isDeleted);
            return Ok(securityRoles);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var securityRole = _securityRoleRepository.Find(id);
            var securityRoleDto = _mapper.Map<SecurityRoleDto>(securityRole);
            return Ok(securityRoleDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] SecurityRoleDto securityRoleDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            securityRoleDto.Id = 0;
            var securityrole = _mapper.Map<SecurityRole>(securityRoleDto);

            var validate = _securityRoleRepository.ValidateRole(securityrole);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _securityRoleRepository.Add(securityrole);
            if (_uow.Save() <= 0) throw new Exception("Creating Occupation failed on save.");

            var i = securityrole.Id;
            var permissionDtos = _rolePermissionRepository.GetByRoleId(i);

            return Ok(permissionDtos);
            //return Ok(securityrole.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] SecurityRoleUpdationDto securityRoleDto)
        {
            if (securityRoleDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var securityRole = _mapper.Map<SecurityRole>(securityRoleDto);
            securityRole.Id = securityRoleDto.Id;

            var validateMessage = _securityRoleRepository.ValidateRole(securityRole);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _securityRoleRepository.Update(securityRole);
            if (_uow.Save() <= 0) throw new Exception("Updating a Role failed on save.");
            return Ok(securityRole.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _securityRoleRepository.Find(id);

            if (record == null)
                return NotFound();

            var users = _userRoleRepository.FindBy(x => x.UserRoleId == id).ToList();
            if (users.Count > 0)
            {
                ModelState.AddModelError("Message", "Role is assigned to user so delete not allow");
                return BadRequest(ModelState);
            }

            _securityRoleRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _securityRoleRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _securityRoleRepository.ValidateRole(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _securityRoleRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetSecurityRoleDropDown")]
        public IActionResult GetSecurityRoleDropDown()
        {
            return Ok(_securityRoleRepository.GetSecurityRoleDropDown());
        }
    }
}