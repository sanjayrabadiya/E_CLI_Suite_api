﻿using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PassthroughMilestoneInvoiceController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPassthroughMilestoneInvoiceRepository _passthroughMilestoneInvoiceRepository;
        private readonly IUnitOfWork _uow;

        public PassthroughMilestoneInvoiceController(IPassthroughMilestoneInvoiceRepository passthroughMilestoneInvoiceRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _passthroughMilestoneInvoiceRepository = passthroughMilestoneInvoiceRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var passthroughMilestoneInvoices = _passthroughMilestoneInvoiceRepository.GetPassthroughMilestoneInvoiceList(isDeleted);
            return Ok(passthroughMilestoneInvoices);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var passthroughMilestoneInvoice = _passthroughMilestoneInvoiceRepository.Find(id);
            var passthroughMilestoneInvoiceDto = _mapper.Map<PassthroughMilestoneInvoiceDto>(passthroughMilestoneInvoice);
            return Ok(passthroughMilestoneInvoiceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PassthroughMilestoneInvoiceDto passthroughMilestoneInvoiceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            passthroughMilestoneInvoiceDto.Id = 0;
            var passthroughMilestoneInvoice = _mapper.Map<PassthroughMilestoneInvoice>(passthroughMilestoneInvoiceDto);
            var validate = _passthroughMilestoneInvoiceRepository.Duplicate(passthroughMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _passthroughMilestoneInvoiceRepository.Add(passthroughMilestoneInvoice);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Passthrough Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(passthroughMilestoneInvoice.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PassthroughMilestoneInvoiceDto passthroughMilestoneInvoiceDto)
        {
            if (passthroughMilestoneInvoiceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var passthroughMilestoneInvoice = _mapper.Map<PassthroughMilestoneInvoice>(passthroughMilestoneInvoiceDto);
            var validate = _passthroughMilestoneInvoiceRepository.Duplicate(passthroughMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _passthroughMilestoneInvoiceRepository.AddOrUpdate(passthroughMilestoneInvoice);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Passthrough Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(passthroughMilestoneInvoice.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _passthroughMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            _passthroughMilestoneInvoiceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _passthroughMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _passthroughMilestoneInvoiceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _passthroughMilestoneInvoiceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetPassthroughMilestoneById/{milestoneId}")]
        public ActionResult GetPassthroughMilestoneById(int milestoneId)
        {
            var record = _passthroughMilestoneInvoiceRepository.GetPassthroughMilestoneById(milestoneId);
            return Ok(record);
        }
    }
}