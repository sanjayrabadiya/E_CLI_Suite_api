using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LogReport;
using GSC.Data.Entities.LogReport;
using GSC.Domain.Context;
using GSC.Respository.LogReport;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LogReport
{
    [Route("api/[controller]")]
    public class UserLoginReportController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserLoginReportRespository _userLoginReportRepository;

        public UserLoginReportController(
            IUserLoginReportRespository userLoginReportRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _userLoginReportRepository = userLoginReportRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var userLoginReports = _userLoginReportRepository.All.Where(x =>
                x.user.CompanyId == _jwtTokenAccesser.CompanyId
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).ToList();
            var userLoginReportsDto = _mapper.Map<IEnumerable<UserLoginReportDto>>(userLoginReports);
            return Ok(userLoginReportsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var userLoginReport = _userLoginReportRepository.Find(id);
            var userLoginReportDto = _mapper.Map<UserLoginReportDto>(userLoginReport);
            return Ok(userLoginReportDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UserLoginReportDto userLoginReportDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            userLoginReportDto.Id = 0;
            var userLoginReport = _mapper.Map<UserLoginReport>(userLoginReportDto);
            _userLoginReportRepository.Add(userLoginReport);
            if (_uow.Save() <= 0) throw new Exception("Creating User Login Report failed on save.");
            return Ok(userLoginReport.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] UserLoginReportDto userLoginReportDto)
        {
            if (userLoginReportDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var userLoginReport = _mapper.Map<UserLoginReport>(userLoginReportDto);

            _userLoginReportRepository.Update(userLoginReport);
            if (_uow.Save() <= 0) throw new Exception("Updating User Login Report failed on save.");
            return Ok(userLoginReport.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _userLoginReportRepository.Find(id);

            if (record == null)
                return NotFound();

            _userLoginReportRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _userLoginReportRepository.Find(id);

            if (record == null)
                return NotFound();
            _userLoginReportRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}