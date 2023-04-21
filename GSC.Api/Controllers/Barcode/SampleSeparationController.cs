using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Respository.Barcode;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleSeparationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ISampleSeparationRepository _sampleSeparationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public SampleSeparationController(ISampleSeparationRepository sampleSeparationRepository,
            IJwtTokenAccesser jwtTokenAccesser,
        IUnitOfWork uow, IMapper mapper)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _sampleSeparationRepository = sampleSeparationRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetSampleSaparation/{SiteId}/{TemplateId}")]
        public IActionResult GetSampleSaparation(int SiteId,int TemplateId)
        {
            var sampleSaparationDetails = _sampleSeparationRepository.GetSampleDetails(SiteId,TemplateId);
            return Ok(sampleSaparationDetails);
        }


        [HttpPost]
        [Route("InsertSampleSaparationData")]
        public ActionResult InsertSampleSaparationData([FromBody] SampleSaveSeparationDto sampleSaveSeparationDto)
        {
            if (sampleSaveSeparationDto == null) return new UnprocessableEntityObjectResult(ModelState);

            _sampleSeparationRepository.StartSampleSaparation(sampleSaveSeparationDto);

            if (_uow.Save() <= 0) throw new Exception("Creating SampleSaparation Details failed on save.");
            return Ok();
        }


        [HttpPut]
        [Route("MissedSampleSaparation/{pkId}/{samplebarcode}/{AuditReasonId}/{ReasonOth}")]
        public ActionResult MissedSampleSaparation(int pkId,string samplebarcode, int AuditReasonId, string ReasonOth)
        {
            var record = _sampleSeparationRepository.All.Where(x => x.PKBarcodeId == pkId && x.SampleBarcodeString == samplebarcode).FirstOrDefault();

            if (record == null)
                return NotFound();

            record.ReasonOth = ReasonOth;
            record.AuditReasonId = AuditReasonId;
            record.Status = Helper.SampleSeparationFilter.Missed;

            _sampleSeparationRepository.Update(record);
            if (_uow.Save() <= 0) throw new Exception("Sample Saparation missed failed on save.");

            return Ok();
        }

        [HttpPut]
        [Route("HimolizedSampleSaparation/{pkId}/{AuditReasonId}/{ReasonOth}")]
        public ActionResult HimolizedSampleSaparation(int pkId, int AuditReasonId, string ReasonOth)
        {
            var item = _sampleSeparationRepository.All.Where(x => x.PKBarcodeId == pkId).ToList();

            if (item == null)
                return NotFound();

            foreach (var record in item)
            {
                record.ReasonOth = ReasonOth;
                record.AuditReasonId = AuditReasonId;
                record.Status = Helper.SampleSeparationFilter.Hemolized;

                _sampleSeparationRepository.Update(record);
                if (_uow.Save() <= 0) throw new Exception("Sample Saparation hemolized failed on save.");
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateForSaparation/{siteId}")]
        public IActionResult GetTemplateForSaparation(int siteId)
        {
            return Ok(_sampleSeparationRepository.GetTemplateForSaparation(siteId));
        }
    }
}
