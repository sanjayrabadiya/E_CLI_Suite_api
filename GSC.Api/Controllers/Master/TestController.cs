﻿using System;
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
    public class TestController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ITestRepository _testRepository;
        private readonly ITestGroupRepository _testGroupRepository;
        private readonly IUnitOfWork _uow;

        public TestController(ITestRepository testRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ITestGroupRepository testGroupRepository)
        {
            _testRepository = testRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _testGroupRepository = testGroupRepository;
        }

        [HttpGet]
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var tests = _testRepository.GetTestList(isDeleted);
            tests.ForEach(b =>
            {
                b.TestGroup = _testGroupRepository.Find((int)b.TestGroupId);
            });
            return Ok(tests);
            
            //var tests = _testRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null, x => x.TestGroup)
            //    .OrderByDescending(x => x.Id).ToList();
            //return Ok(testsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var test = _testRepository.Find(id);
            var testDto = _mapper.Map<TestDto>(test);
            return Ok(testDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TestDto testDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            testDto.Id = 0;
            var test = _mapper.Map<Test>(testDto);
            var validate = _testRepository.Duplicate(test);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _testRepository.Add(test);
            if (_uow.Save() <= 0) throw new Exception("Creating Test failed on save.");
            return Ok(test.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TestDto testDto)
        {
            if (testDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var test = _mapper.Map<Test>(testDto);
            var validate = _testRepository.Duplicate(test);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _testRepository.AddOrUpdate(test);

            if (_uow.Save() <= 0) throw new Exception("Updating Test failed on save.");
            return Ok(test.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _testRepository.Find(id);

            if (record == null)
                return NotFound();

            _testRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _testRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _testRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _testRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTestDropDown")]
        public IActionResult GetTestDropDown()
        {
            return Ok(_testRepository.GetTestDropDown());
        }

        [HttpGet]
        [Route("GetTestDropDownByTestGroup/{id}")]
        public IActionResult GetTestDropDownByTestGroup(int id)
        {
            return Ok(_testRepository.GetTestDropDownByTestGroup(id));
        }
    }
}