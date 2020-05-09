using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    public class NoneRegisterController : BaseController
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly INoneRegisterRepository _noneRegisterRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public NoneRegisterController(INoneRegisterRepository noneRegisterterRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository)
        {
            _noneRegisterRepository = noneRegisterterRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _attendanceRepository = attendanceRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var volunteerNonregisters = _noneRegisterRepository.All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && x.IsDeleted == isDeleted
            ).OrderByDescending(t => t.Id).ToList();
            var volunteerNonregisterDto = _mapper.Map<IEnumerable<NoneRegisterDto>>(volunteerNonregisters);
            return Ok(volunteerNonregisterDto);
        }

        [HttpGet("GetNonRegisterList/{projectId}")]
        public IActionResult GetNonRegisterList(int projectId)
        {
            return Ok(_noneRegisterRepository.GetNonRegisterList(projectId));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var volunteerNonregister = _noneRegisterRepository.Find(id);
            var volunteerNonregisterDto = _mapper.Map<NoneRegisterDto>(volunteerNonregister);
            return Ok(volunteerNonregisterDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] NoneRegisterDto noneRegisterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var projectDesignPeriod = _projectDesignPeriodRepository.FindBy(x =>
                x.DeletedDate == null && x.ProjectDesign.DeletedDate == null &&
                x.ProjectDesign.ProjectId == noneRegisterDto.ParentProjectId).FirstOrDefault();

            if (projectDesignPeriod == null)
            {
                ModelState.AddModelError("Message", "Design is not complete");
                return BadRequest(ModelState);
            }

            noneRegisterDto.ProjectDesignPeriodId = projectDesignPeriod.Id;
            var noneRegister = _mapper.Map<NoneRegister>(noneRegisterDto);

            var validate = _noneRegisterRepository.Duplicate(noneRegister, noneRegisterDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _noneRegisterRepository.SaveNonRegister(noneRegister, noneRegisterDto);
            if (_uow.Save() <= 0) throw new Exception("Creating None register failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] NoneRegisterDto volunteerNonregisterDto)
        {
            if (volunteerNonregisterDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerNonregisterDto.Initial = volunteerNonregisterDto.Initial.PadRight(3, '-');
            var nonregister = _mapper.Map<NoneRegister>(volunteerNonregisterDto);

            var validate = _noneRegisterRepository.Duplicate(nonregister, volunteerNonregisterDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _noneRegisterRepository.Update(nonregister);
            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");
            return Ok(nonregister.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _noneRegisterRepository
                .FindByInclude(x => x.Id == id && x.DeletedDate == null, x => x.Attendance).FirstOrDefault();

            if (record == null)
                return NotFound();

            if (record.Attendance.IsProcessed)
            {
                ModelState.AddModelError("Message", "Can not delete , because this record in under process.");
                return BadRequest(ModelState);
            }

            _attendanceRepository.Delete(record.Attendance);
            _noneRegisterRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _noneRegisterRepository
                .FindByInclude(x => x.Id == id && x.DeletedDate == null, x => x.Attendance).FirstOrDefault();

            if (record == null)
                return NotFound();
            _noneRegisterRepository.Active(record);
            _attendanceRepository.Active(record.Attendance);
            _uow.Save();
            return Ok();
        }
    }
}