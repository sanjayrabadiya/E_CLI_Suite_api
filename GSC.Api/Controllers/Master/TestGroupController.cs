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
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class TestGroupController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ITestGroupRepository _testGroupRepository;
        private readonly IUnitOfWork _uow;

        public TestGroupController(ITestGroupRepository testGroupRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _testGroupRepository = testGroupRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var testGroups = _testGroupRepository.GetTestGroupList(isDeleted);
            return Ok(testGroups);
            //var testGroups = _testGroupRepository
            //    .All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            //    ).OrderByDescending(x => x.Id).ToList();
            //return Ok(testGroupsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var testGroup = _testGroupRepository.Find(id);
            var testGroupDto = _mapper.Map<TestGroupDto>(testGroup);
            return Ok(testGroupDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TestGroupDto testGroupDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            testGroupDto.Id = 0;
            var testGroup = _mapper.Map<TestGroup>(testGroupDto);
            var validate = _testGroupRepository.Duplicate(testGroup);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _testGroupRepository.Add(testGroup);
            if (_uow.Save() <= 0) throw new Exception("Creating Test Group failed on save.");
            return Ok(testGroup.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TestGroupDto testGroupDto)
        {
            if (testGroupDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var testGroup = _mapper.Map<TestGroup>(testGroupDto);
            var validate = _testGroupRepository.Duplicate(testGroup);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(testGroup.Id);
            testGroup.Id = 0;
            _testGroupRepository.Add(testGroup);

            if (_uow.Save() <= 0) throw new Exception("Updating Test Group failed on save.");
            return Ok(testGroup.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _testGroupRepository.Find(id);

            if (record == null)
                return NotFound();

            _testGroupRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _testGroupRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _testGroupRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _testGroupRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTestGroupDropDown")]
        public IActionResult GetTestGroupDropDown()
        {
            return Ok(_testGroupRepository.GetTestGroupDropDown());
        }
    }
}