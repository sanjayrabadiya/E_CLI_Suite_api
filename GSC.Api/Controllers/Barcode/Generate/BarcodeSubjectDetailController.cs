using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Domain.Context;
using GSC.Respository.Barcode.Generate;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode.Generate
{
    [Route("api/[controller]")]
    public class BarcodeSubjectDetailController : BaseController
    {
        private readonly IBarcodeSubjectDetailRepository _barcodeSubjectDetailRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BarcodeSubjectDetailController(IBarcodeSubjectDetailRepository barcodeSubjectDetailRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _barcodeSubjectDetailRepository = barcodeSubjectDetailRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_barcodeSubjectDetailRepository.GetBarcodeSubjectDetail(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var barcodeSubjectDetail = _barcodeSubjectDetailRepository.Find(id);
            var barcodeSubjectDetailsDto = _mapper.Map<BarcodeSubjectDetailDto>(barcodeSubjectDetail);
            return Ok(barcodeSubjectDetailsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] BarcodeSubjectDetailDto barcodeSubjectDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            barcodeSubjectDetailDto.Id = 0;
            var barcodeSubjectDetail = _mapper.Map<BarcodeSubjectDetail>(barcodeSubjectDetailDto);

            _barcodeSubjectDetailRepository.Add(barcodeSubjectDetail);
            if (_uow.Save() <= 0) throw new Exception("Creating barcode Subject details failed on save.");
            return Ok(barcodeSubjectDetail.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] BarcodeSubjectDetailDto barcodeSubjectDetailDto)
        {
            if (barcodeSubjectDetailDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeSubjectDetail = _mapper.Map<BarcodeSubjectDetail>(barcodeSubjectDetailDto);

            _barcodeSubjectDetailRepository.Update(barcodeSubjectDetail);

            if (_uow.Save() <= 0) throw new Exception("Updating barcode Subject details failed on save.");
            return Ok(barcodeSubjectDetail.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _barcodeSubjectDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeSubjectDetailRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _barcodeSubjectDetailRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeSubjectDetailRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}