using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class NumberFormatController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IUnitOfWork _uow;

        public NumberFormatController(INumberFormatRepository numberFormatRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _numberFormatRepository = numberFormatRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var numberFormats = _numberFormatRepository.All.Where(x =>
                x.CompanyId == _jwtTokenAccesser.CompanyId
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var numberFormatsDto = _mapper.Map<IEnumerable<NumberFormatDto>>(numberFormats);
            return Ok(numberFormatsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var numberFormat = _numberFormatRepository.Find(id);
            var numberFormatDto = _mapper.Map<NumberFormatDto>(numberFormat);
            return Ok(numberFormatDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] NumberFormatDto numberFormatDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            numberFormatDto.Id = 0;
            var numberFormat = _mapper.Map<NumberFormat>(numberFormatDto);
            _numberFormatRepository.Add(numberFormat);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Upload Setting failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(numberFormat.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] NumberFormatDto numberFormatDto)
        {
            if (numberFormatDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var numberFormat = _mapper.Map<NumberFormat>(numberFormatDto);

            _numberFormatRepository.Update(numberFormat);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Upload Setting failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(numberFormat.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _numberFormatRepository.Find(id);

            if (record == null)
                return NotFound();

            _numberFormatRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _numberFormatRepository.Find(id);

            if (record == null)
                return NotFound();
            _numberFormatRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}