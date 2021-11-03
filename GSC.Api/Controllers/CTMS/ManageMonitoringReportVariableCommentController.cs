using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ManageMonitoringReportVariableCommentController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IManageMonitoringReportVariableCommentRepository _manageMonitoringReportVariableCommentRepository;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringReportVariableCommentController(
            IManageMonitoringReportVariableCommentRepository manageMonitoringReportVariableCommentRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
        {
            _manageMonitoringReportVariableCommentRepository = manageMonitoringReportVariableCommentRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{ManageMonitoringReportVariableId}")]
        public IActionResult Get(int manageMonitoringReportVariableId)
        {
            if (manageMonitoringReportVariableId <= 0) return BadRequest();

            var commentsDto = _manageMonitoringReportVariableCommentRepository.GetComments(manageMonitoringReportVariableId);

            return Ok(commentsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoringReportVariableComment = _mapper.Map<ManageMonitoringReportVariableComment>(manageMonitoringReportVariableCommentDto);

            manageMonitoringReportVariableComment.Id = 0;
            manageMonitoringReportVariableComment.RoleId = _jwtTokenAccesser.RoleId;

            _manageMonitoringReportVariableCommentRepository.Add(manageMonitoringReportVariableComment);

            if (_uow.Save() <= 0) throw new Exception("Creating comment failed on save.");
            return Ok(manageMonitoringReportVariableComment.Id);
        }
    }
}