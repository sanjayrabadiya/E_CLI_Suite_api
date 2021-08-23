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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IBarcodeAuditRepository _barcodeAuditRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BarcodeAuditController(IBarcodeAuditRepository barcodeAuditRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _barcodeAuditRepository = barcodeAuditRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetBarodeAuditDetails/{ProjectId}")]
        public IActionResult GetMeddraAuditDetails(int ProjectId)
        {
            return Ok(_barcodeAuditRepository.GetBarcodeAuditDetails(ProjectId));
        }
    }
}
