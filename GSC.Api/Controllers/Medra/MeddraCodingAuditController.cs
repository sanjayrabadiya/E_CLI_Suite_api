using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Medra;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeddraCodingAuditController : BaseController
    {
        private readonly IMeddraCodingAuditRepository _meddraCodingAuditRepository;
        public MeddraCodingAuditController(IMeddraCodingAuditRepository meddraCodingAuditRepository)
        {
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
        }

        [HttpGet]
        [Route("GetMeddraAuditDetails/{MeddraCodingId}")]
        public IActionResult GetMeddraAuditDetails(int MeddraCodingId)
        {
            return Ok(_meddraCodingAuditRepository.GetMeddraAuditDetails(MeddraCodingId));
        }
    }
}