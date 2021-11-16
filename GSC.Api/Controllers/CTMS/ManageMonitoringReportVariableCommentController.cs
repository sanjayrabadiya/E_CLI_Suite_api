using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
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
        private readonly IManageMonitoringReportReviewRepository _manageMonitoringReportReviewRepository;
        private readonly IManageMonitoringReportVariableRepository _manageMonitoringReportVariableRepository;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringReportVariableCommentController(
            IManageMonitoringReportVariableCommentRepository manageMonitoringReportVariableCommentRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser,
            IManageMonitoringReportReviewRepository manageMonitoringReportReviewRepository,
            IManageMonitoringReportVariableRepository manageMonitoringReportVariableRepository)
        {
            _manageMonitoringReportVariableCommentRepository = manageMonitoringReportVariableCommentRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _manageMonitoringReportReviewRepository = manageMonitoringReportReviewRepository;
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
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

        [HttpPost("update")]
        [TransactionRequired]
        public IActionResult Update([FromBody] ManageMonitoringReportVariableCommentDto manageMonitoringReportVariableCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoringReportVariable = _manageMonitoringReportVariableRepository.Find(manageMonitoringReportVariableCommentDto.ManageMonitoringReportVariableId);
            //if (screeningTemplateValue.com == QueryStatus.Answered ||
            //    screeningTemplateValue.QueryStatus == QueryStatus.Resolved)
            //{
            //    ModelState.AddModelError("Message", "Query is already updated.");
            //    return BadRequest(ModelState);
            //}

            var comment = _manageMonitoringReportVariableCommentRepository.All.Where(x => x.ManageMonitoringReportVariableId == manageMonitoringReportVariableCommentDto.ManageMonitoringReportVariableId).OrderByDescending(x=>x.Id).FirstOrDefault();

            manageMonitoringReportVariableCommentDto.Id = comment.Id;
            manageMonitoringReportVariableCommentDto.Comment = comment.Comment;
            var manageMonitoringReportVariableComment = _mapper.Map<ManageMonitoringReportVariableComment>(manageMonitoringReportVariableCommentDto);

            _manageMonitoringReportVariableCommentRepository.UpdateQuery(manageMonitoringReportVariableCommentDto, manageMonitoringReportVariableComment, manageMonitoringReportVariable);

            _uow.Save();

            return Ok(manageMonitoringReportVariableComment.Id);
        }
    }
}