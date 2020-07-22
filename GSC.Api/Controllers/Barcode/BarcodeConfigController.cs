using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Respository.Barcode;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    public class BarcodeConfigController : BaseController
    {
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BarcodeConfigController(IBarcodeConfigRepository barcodeConfigRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _barcodeConfigRepository = barcodeConfigRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_barcodeConfigRepository.GetBarcodeConfig(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var barcodeConfig = _barcodeConfigRepository.Find(id);
            var barcodeConfigDto = _mapper.Map<BarcodeConfigDto>(barcodeConfig);
            return Ok(barcodeConfigDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] BarcodeConfigDto barcodeConfigDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            barcodeConfigDto.Id = 0;
            var barcodeConfig = _mapper.Map<BarcodeConfig>(barcodeConfigDto);

            _barcodeConfigRepository.Add(barcodeConfig);
            if (_uow.Save() <= 0) throw new Exception("Creating barcode config failed on save.");
            return Ok(barcodeConfig.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] BarcodeConfigDto barcodeConfigDto)
        {
            if (barcodeConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeConfig = _mapper.Map<BarcodeConfig>(barcodeConfigDto);

            _barcodeConfigRepository.Update(barcodeConfig);

            if (_uow.Save() <= 0) throw new Exception("Updating barcode config failed on save.");
            return Ok(barcodeConfig.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _barcodeConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeConfigRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _barcodeConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeConfigRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}