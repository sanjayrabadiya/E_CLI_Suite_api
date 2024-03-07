using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.Barcode;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeAuditController : BaseController
    {
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;
        public BarcodeAuditController(IBarcodeAuditRepository barcodeAuditRepository)
        {
            _barcodeAuditRepository = barcodeAuditRepository;
        }

        [HttpGet]
        [Route("GetBarodeAuditDetails/{ProjectId}")]
        public IActionResult GetMeddraAuditDetails(int ProjectId)
        {
            return Ok(_barcodeAuditRepository.GetBarcodeAuditDetails(ProjectId));
        }
    }
}
