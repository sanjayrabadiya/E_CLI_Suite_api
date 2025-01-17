﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using GSC.Data.Entities.CTMS;
using Microsoft.AspNetCore.Mvc;
using System;
using GSC.Shared.JWTAuth;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class ProcedureController : BaseController
    {
        private readonly IProcedureRepository _procedureRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProcedureController(IProcedureRepository procedureRepository, IJwtTokenAccesser jwtTokenAccesser,

            IUnitOfWork uow, IMapper mapper)
        {
            _procedureRepository = procedureRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var procedure = _procedureRepository.GetProcedureList(isDeleted);
            return Ok(procedure);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var procedure = _procedureRepository.Find(id);
            var procedureDto = _mapper.Map<ProcedureDto>(procedure);
            return Ok(procedureDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProcedureDto procedureDtoDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            procedureDtoDto.Id = 0;
            var procedure = _mapper.Map<Procedure>(procedureDtoDto);
            var validatecode = _procedureRepository.Duplicate(procedure);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            procedure.IpAddress = _jwtTokenAccesser.IpAddress;
            procedure.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _procedureRepository.Add(procedure);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Procedure failed on save."));
            return Ok(procedure.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProcedureDto procedureDto)
        {
            if (procedureDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var procedure = _mapper.Map<Procedure>(procedureDto);
            var validatecode = _procedureRepository.Duplicate(procedure);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            procedure.IpAddress = _jwtTokenAccesser.IpAddress;
            procedure.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _procedureRepository.Update(procedure);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating Procedure failed on save."));
            return Ok(procedure.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _procedureRepository.Find(id);

            if (record == null)
                return NotFound();

            _procedureRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _procedureRepository.Find(id);

            var validatecode = _procedureRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }

            if (record == null)
                return NotFound();


            _procedureRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProcedureDropDown")]
        public IActionResult GetProcedureDropDown()
        {
            return Ok(_procedureRepository.GetParentProjectDropDown());
        }
    }
}
