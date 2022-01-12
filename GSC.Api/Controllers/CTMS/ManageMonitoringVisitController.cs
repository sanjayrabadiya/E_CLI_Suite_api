using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ManageMonitoringVisitController : BaseController
    {
        private readonly IManageMonitoringVisitRepository _manageMonitoringVisitRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringVisitController(IManageMonitoringVisitRepository manageMonitoringVisitRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _manageMonitoringVisitRepository = manageMonitoringVisitRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        /// Get Monitoring Visit list 
        /// Created By Swati
        [HttpGet]
        [Route("GetMonitoringVisit/{projectId}")]
        public IActionResult GetMonitoringVisit(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var result = _manageMonitoringVisitRepository.GetMonitoringVisit(projectId);
            return Ok(result);
        }

        /// Add Monitoring Visit
        /// Created By Swati
        [HttpPost]
        public IActionResult Post([FromBody] ManageMonitoringVisitDto manageMonitoringVisitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageMonitoringVisitDto.Id = 0;
            var manageMonitoringVisit = _mapper.Map<ManageMonitoringVisit>(manageMonitoringVisitDto);
            _manageMonitoringVisitRepository.Add(manageMonitoringVisit);
            if (_uow.Save() <= 0) throw new Exception("Creating Manage Monitoring Visit failed on save.");

            return Ok(manageMonitoringVisit.Id);
        }

        /// Update Monitoring Visit
        /// Created By Swati
        [HttpPut]
        public IActionResult Put([FromBody] ManageMonitoringVisitDto manageMonitoringVisitDto)
        {
            if (manageMonitoringVisitDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoringVisit = _mapper.Map<ManageMonitoringVisit>(manageMonitoringVisitDto);

            _manageMonitoringVisitRepository.Update(manageMonitoringVisit);
            if (_uow.Save() <= 0) throw new Exception("Updating Manage Monitoring Visit failed on save.");
            return Ok(manageMonitoringVisit.Id);
        }
    }
}