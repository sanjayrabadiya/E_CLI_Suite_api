
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
    public class SampleBarcodeController : BaseController
    {
        private readonly ISampleBarcodeRepository _sampleBarcodeRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public SampleBarcodeController(ISampleBarcodeRepository sampleBarcodeRepository, IUnitOfWork uow, IMapper mapper)
        {
            _sampleBarcodeRepository = sampleBarcodeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var sampleBarcodeList = _sampleBarcodeRepository.GetSampleBarcodeList(isDeleted);
            return Ok(sampleBarcodeList);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var sampleBarcode = _sampleBarcodeRepository.Find(id);
            var sampleBarcodeDto = _mapper.Map<SampleBarcodeDto>(sampleBarcode);
            return Ok(sampleBarcodeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SampleBarcodeDto sampleBarcodeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            sampleBarcodeDto.Id = 0;
            var barcode = _mapper.Map<SampleBarcode>(sampleBarcodeDto);
            var validate = _sampleBarcodeRepository.Duplicate(sampleBarcodeDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _sampleBarcodeRepository.Add(barcode);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Contact Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(barcode.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SampleBarcodeDto sampleBarcodeDto)
        {
            if (sampleBarcodeDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var barcode = _mapper.Map<SampleBarcode>(sampleBarcodeDto);
            _sampleBarcodeRepository.Update(barcode);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Sample Barcode failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(barcode.Id);
        }

        [HttpPost("UpdateBarcode")]
        public IActionResult UpdateBarcode([FromBody] int[] ids)
        {
            _sampleBarcodeRepository.UpdateBarcode(ids.ToList());
            return Ok(1);
        }

        [HttpPost("DeleteBarcode")]
        public IActionResult DeleteBarcode([FromBody] int[] ids)
        {
            _sampleBarcodeRepository.DeleteBarcode(ids.ToList());
            return Ok(1);
        }

        [HttpPost("ReprintBarcode")]
        public IActionResult ReprintBarcode([FromBody] int[] ids)
        {
            _sampleBarcodeRepository.BarcodeReprint(ids.ToList());
            return Ok(1);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _sampleBarcodeRepository.Find(id);

            if (record == null)
                return NotFound();

            _sampleBarcodeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _sampleBarcodeRepository.Find(id);
            if (record == null)
                return NotFound();
            _sampleBarcodeRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpGet("GetProjectList")]
        public ActionResult GetProjectList()
        {
            var projectList = _sampleBarcodeRepository.GetProjectDropdown();
            return Ok(projectList);
        }

        [HttpGet("GetSiteList/{projectId}")]
        public ActionResult GetSiteList(int projectId)
        {
            var projectList = _sampleBarcodeRepository.GetChildProjectDropDown(projectId);
            return Ok(projectList);
        }

        [HttpGet("GetVisitList/{projectId}/{siteId}")]
        public ActionResult GetVisitList(int projectId, int siteId)
        {
            var projectList = _sampleBarcodeRepository.GetVisitList(projectId, siteId);
            return Ok(projectList);
        }
        [HttpGet("GetTemplateList/{projectId}/{siteId}/{visitId}")]
        public ActionResult GetTemplateList(int projectId, int siteId, int visitId)
        {
            var projectList = _sampleBarcodeRepository.GetTemplateList(projectId, siteId, visitId);
            return Ok(projectList);
        }
        [HttpGet("GetVolunteerList/{siteId}")]
        public ActionResult GetVolunteerList(int siteId)
        {
            var projectList = _sampleBarcodeRepository.GetVolunteerList(siteId);
            return Ok(projectList);
        }
    }
}
