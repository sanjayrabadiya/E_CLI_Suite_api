
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Respository.Attendance;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    [ApiController]
    public class PKBarcodeController : BaseController
    {
        private readonly IPKBarcodeRepository _pkBarcodeRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public PKBarcodeController(IPKBarcodeRepository pKBarcodeRepository, IUnitOfWork uow, IMapper mapper)
        {
            _pkBarcodeRepository = pKBarcodeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var pkbarcodeList = _pkBarcodeRepository.GetPKBarcodeList(isDeleted);
            return Ok(pkbarcodeList);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var pkBarcode = _pkBarcodeRepository.Find(id);
            var pkBarcodeDto = _mapper.Map<PKBarcodeDto>(pkBarcode);
            return Ok(pkBarcodeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PKBarcodeDto pkBarcodeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pkBarcodeDto.Id = 0;
            var barcode = _mapper.Map<PKBarcode>(pkBarcodeDto);
            var validate = _pkBarcodeRepository.Duplicate(pkBarcodeDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            barcode.BarcodeString = _pkBarcodeRepository.GenerateBarcodeString(pkBarcodeDto);
            _pkBarcodeRepository.Add(barcode);
            if (_uow.Save() <= 0) throw new Exception("Creating PK barcode failed on save.");
            return Ok(barcode.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PKBarcodeDto pkBarcodeDto)
        {
            if (pkBarcodeDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var barcode = _mapper.Map<PKBarcode>(pkBarcodeDto);
            //var validate = _pkBarcodeRepository.Duplicate(pkBarcodeDto);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            _pkBarcodeRepository.Update(barcode);
            if (_uow.Save() <= 0) throw new Exception("Updating Contact Type failed on save.");
            return Ok(barcode.Id);
        }

        [HttpPost("UpdateBarcode")]
        public IActionResult UpdateBarcode([FromBody] int[] ids)
        {
            _pkBarcodeRepository.UpdateBarcode(ids.ToList());
            return Ok(1);
        }

        [HttpPost("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] int[] ids)
        {
            _pkBarcodeRepository.DeleteBarcode(ids.ToList());
            return Ok(1);
        }

        [HttpPost("ReprintBarcode")]
        public IActionResult ReprintBarcode([FromBody] int[] ids)
        {
            _pkBarcodeRepository.BarcodeReprint(ids.ToList());
            return Ok(1);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pkBarcodeRepository.Find(id);

            if (record == null)
                return NotFound();

            _pkBarcodeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pkBarcodeRepository.Find(id);
            if (record == null)
                return NotFound();
            _pkBarcodeRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetPkSubjectDetails/{siteId}/{templateId}")]
        public IActionResult GetPkSubjectDetails(int siteId, int templateId)
        {
            return Ok(_pkBarcodeRepository.GetPkSubjectDetails(siteId, templateId));
        }

    }
}
