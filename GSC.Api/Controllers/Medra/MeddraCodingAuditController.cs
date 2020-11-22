using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Medra;
using GSC.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeddraCodingAuditController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMeddraCodingAuditRepository _meddraCodingAuditRepository;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        private readonly IMeddraSocTermRepository _meddraSocTermRepository;
        private readonly IStudyScopingRepository _studyScopingRepository;
        private readonly IMeddraCodingCommentRepository _meddraCodingCommentRepository;
        private readonly IMeddraMdHierarchyRepository _meddraMdHierarchyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public MeddraCodingAuditController(IMeddraCodingRepository meddraCodingRepository,
            IMeddraCodingAuditRepository meddraCodingAuditRepository,
            IMeddraCodingCommentRepository meddraCodingCommentRepository,
            IStudyScopingRepository studyScopingRepository,
            IMeddraSocTermRepository meddraSocTermRepository,
            IMeddraMdHierarchyRepository meddraMdHierarchyRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _meddraCodingRepository = meddraCodingRepository;
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
            _meddraCodingCommentRepository = meddraCodingCommentRepository;
            _meddraMdHierarchyRepository = meddraMdHierarchyRepository;
            _studyScopingRepository = studyScopingRepository;
            _meddraSocTermRepository = meddraSocTermRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetMeddraAuditDetails/{MeddraCodingId}")]
        public IActionResult GetMeddraAuditDetails(int MeddraCodingId)
        {
            return Ok(_meddraCodingAuditRepository.GetMeddraAuditDetails(MeddraCodingId));
        }
    }
}