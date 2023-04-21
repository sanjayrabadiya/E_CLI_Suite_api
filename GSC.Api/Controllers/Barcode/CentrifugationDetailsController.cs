using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Barcode;
using GSC.Helper;
using GSC.Respository.Barcode;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentrifugationDetailsController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ICentrifugationDetailsRepository _centrifugationDetailsRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CentrifugationDetailsController(ICentrifugationDetailsRepository centrifugationDetailsRepository,
            IJwtTokenAccesser jwtTokenAccesser,
        IUnitOfWork uow, IMapper mapper)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _centrifugationDetailsRepository = centrifugationDetailsRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCentrifugationDetails/{SiteId}")]
        public IActionResult GetCentrifugationDetails(int SiteId)
        {
            var centrifugationDetails = _centrifugationDetailsRepository.GetCentrifugationDetails(SiteId);
            return Ok(centrifugationDetails);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CentrifugationDetailsDto centrifugationDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            centrifugationDetailsDto.Id = 0;
            var centrifugationDetails = _mapper.Map<CentrifugationDetails>(centrifugationDetailsDto);

            _centrifugationDetailsRepository.Add(centrifugationDetails);
            if (_uow.Save() <= 0) throw new Exception("Creating Centrifugation Details failed on save.");
            return Ok(centrifugationDetails.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CentrifugationDetailsDto centrifugationDetailsDto)
        {
            if (centrifugationDetailsDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var centrifugationDetails = _mapper.Map<CentrifugationDetails>(centrifugationDetailsDto);
            _centrifugationDetailsRepository.AddOrUpdate(centrifugationDetails);
            if (_uow.Save() <= 0) throw new Exception("Updating Centrifugation Details failed on save.");
            return Ok(centrifugationDetails.Id);
        }

       

        [HttpGet]
        [Route("GetCentrifugationDetailsByPKBarcode/{PkBarcodeString}")]
        public IActionResult GetCentrifugationDetailsByPKBarcode(string PkBarcodeString)
        {
            var centrifugationDetails = _centrifugationDetailsRepository.GetCentrifugationDetailsByPKBarcode(PkBarcodeString);
            return Ok(centrifugationDetails);
        }


        [HttpPost]
        [TransactionRequired]
        [Route("InsertCentrifugationData")]
        public ActionResult InsertCentrifugationData([FromBody] int[] ids)
        {
            if (ids==null) return new UnprocessableEntityObjectResult(ModelState);

            _centrifugationDetailsRepository.StartCentrifugation(ids.ToList());
         
            if (_uow.Save() <= 0) throw new Exception("Creating Centrifugation Details failed on save.");
            return Ok();
        }

        [HttpPost]
        [TransactionRequired]
        [Route("InsertReCentrifugationData")]
        public ActionResult InsertReCentrifugationData([FromBody] int[] ids)
        {
            if (ids == null) return new UnprocessableEntityObjectResult(ModelState);

            _centrifugationDetailsRepository.StartReCentrifugation(ids.ToList());

            if (_uow.Save() <= 0) throw new Exception("Creating Re-Centrifugation Details failed on save.");
            return Ok();
        }


        [HttpPut]
        [Route("MissedCentrifugation/{pkId}/{AuditReasonId}/{ReasonOth}")]
        public ActionResult MissedCentrifugation(int pkId, int AuditReasonId, string ReasonOth)
        {
            var record = _centrifugationDetailsRepository.All.Where(x=>x.PKBarcodeId==pkId).FirstOrDefault();

            if (record == null)
                return NotFound();

            record.ReasonOth = ReasonOth;
            record.AuditReasonId = AuditReasonId;
            record.Status = Helper.CentrifugationFilter.Missed;
            record.MissedOn = _jwtTokenAccesser.GetClientDate();
            record.MissedBy = _jwtTokenAccesser.UserId;

            _centrifugationDetailsRepository.Update(record);
            if (_uow.Save() <= 0) throw new Exception("Centrifugation missed failed on save.");

            return Ok();
        }

    }
}
