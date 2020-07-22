using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class EmailTemplateController : BaseController
    {
        private readonly IEmailTemplateRepository _emailTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public EmailTemplateController(IEmailTemplateRepository emailTemplateRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _emailTemplateRepository = emailTemplateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var emailTemplates = _emailTemplateRepository.All.Where(x =>
                x.CompanyId == _jwtTokenAccesser.CompanyId &&
                isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var emailTemplatesDto = _mapper.Map<IEnumerable<EmailTemplateDto>>(emailTemplates);
            return Ok(emailTemplatesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var emailTemplate = _emailTemplateRepository.Find(id);
            var emailTemplateDto = _mapper.Map<EmailTemplateDto>(emailTemplate);
            return Ok(emailTemplateDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] EmailTemplateDto emailTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            emailTemplateDto.Id = 0;
            var emailTemplate = _mapper.Map<EmailTemplate>(emailTemplateDto);
            _emailTemplateRepository.Add(emailTemplate);
            if (_uow.Save() <= 0) throw new Exception("Creating Email Template failed on save.");
            return Ok(emailTemplate.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EmailTemplateDto emailTemplateDto)
        {
            if (emailTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var emailTemplate = _mapper.Map<EmailTemplate>(emailTemplateDto);

            _emailTemplateRepository.Update(emailTemplate);
            if (_uow.Save() <= 0) throw new Exception("Updating Email Template failed on save.");
            return Ok(emailTemplate.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _emailTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            _emailTemplateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _emailTemplateRepository.Find(id);

            if (record == null)
                return NotFound();
            _emailTemplateRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}