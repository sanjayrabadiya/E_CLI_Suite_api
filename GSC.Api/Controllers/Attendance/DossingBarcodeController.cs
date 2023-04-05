
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Master;
using GSC.Respository.Attendance;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    public class DossingBarcodeController : BaseController
    {
        private readonly IDossingBarcodeRepository _dossingBarcodeRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public DossingBarcodeController(IDossingBarcodeRepository dossingBarcodeRepository, IUnitOfWork uow, IMapper mapper)
        {
            _dossingBarcodeRepository = dossingBarcodeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var pkbarcodeList = _dossingBarcodeRepository.GetDossingBarcodeList(isDeleted);
            return Ok(pkbarcodeList);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var DossingBarcode = _dossingBarcodeRepository.Find(id);
            var DossingBarcodeDto = _mapper.Map<DossingBarcodeDto>(DossingBarcode);
            return Ok(DossingBarcodeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] DossingBarcodeDto dossingBarcodeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            dossingBarcodeDto.Id = 0;
            var barcode = _mapper.Map<DossingBarcode>(dossingBarcodeDto);
            var validate = _dossingBarcodeRepository.Duplicate(dossingBarcodeDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            //barcode.BarcodeString = _dossingBarcodeRepository.GenerateBarcodeString(dossingBarcodeDto);
            _dossingBarcodeRepository.Add(barcode);
            if (_uow.Save() <= 0) throw new Exception("Creating dossing barcode failed on save.");
            return Ok(barcode.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DossingBarcodeDto DossingBarcodeDto)
        {
            if (DossingBarcodeDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var barcode = _mapper.Map<DossingBarcode>(DossingBarcodeDto);
            //var validate = _dossingBarcodeRepository.Duplicate(DossingBarcodeDto);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            _dossingBarcodeRepository.Update(barcode);
            if (_uow.Save() <= 0) throw new Exception("Updating Contact Type failed on save.");
            return Ok(barcode.Id);
        }

        [HttpPost("UpdateBarcode")]
        public IActionResult UpdateBarcode([FromBody] int[] ids)
        {
            var gridDtos = _dossingBarcodeRepository.UpdateBarcode(ids.ToList());
            return Ok(gridDtos);
        }

        [HttpPost("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] int[] ids)
        {
            _dossingBarcodeRepository.DeleteBarcode(ids.ToList());
            return Ok(1);
        }

        [HttpPost("ReprintBarcode")]
        public IActionResult ReprintBarcode([FromBody] int[] ids)
        {
            _dossingBarcodeRepository.BarcodeReprint(ids.ToList());
            return Ok(1);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _dossingBarcodeRepository.Find(id);

            if (record == null)
                return NotFound();

            _dossingBarcodeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _dossingBarcodeRepository.Find(id);
            if (record == null)
                return NotFound();
            _dossingBarcodeRepository.Active(record);
            _uow.Save();
            return Ok();
        }
    }
}
