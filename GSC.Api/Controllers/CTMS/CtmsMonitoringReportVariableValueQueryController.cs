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
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportVariableValueQueryController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ICtmsMonitoringReportVariableValueQueryRepository _ctmsMonitoringReportVariableValueQueryRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringReportVariableValueQueryController(
            ICtmsMonitoringReportVariableValueQueryRepository ctmsMonitoringReportVariableValueQueryRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository)
        {
            _ctmsMonitoringReportVariableValueQueryRepository = ctmsMonitoringReportVariableValueQueryRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
        }

        /// Get comment by CtmsMonitoringReportVariableValueId
        /// Created By Swati
        [HttpGet("{CtmsMonitoringReportVariableValueId}")]
        public IActionResult Get(int CtmsMonitoringReportVariableValueId)
        {
            if (CtmsMonitoringReportVariableValueId <= 0) return BadRequest();

            var commentsDto = _ctmsMonitoringReportVariableValueQueryRepository.GetQueries(CtmsMonitoringReportVariableValueId);
            return Ok(commentsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoringReportVariableValueQuery = _mapper.Map<CtmsMonitoringReportVariableValueQuery>(ctmsMonitoringReportVariableValueQueryDto);

            ctmsMonitoringReportVariableValueQuery.Id = 0;

            _ctmsMonitoringReportVariableValueQueryRepository.Add(ctmsMonitoringReportVariableValueQuery);

            if (_uow.Save() <= 0) throw new Exception("Creating query failed on save.");
            return Ok(ctmsMonitoringReportVariableValueQuery.Id);
        }

        /// Generate Query
        /// Created By Swati
        [HttpPost("generate")]
        public IActionResult Generate([FromBody] CtmsMonitoringReportVariableValueQueryDto ctmsMonitoringReportVariableValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoringReportVariableValue = _ctmsMonitoringReportVariableValueRepository.Find(ctmsMonitoringReportVariableValueQueryDto.CtmsMonitoringReportVariableValueId);
            var comment = _ctmsMonitoringReportVariableValueQueryRepository.All.Where(x => x.CtmsMonitoringReportVariableValueId == ctmsMonitoringReportVariableValueQueryDto.CtmsMonitoringReportVariableValueId).OrderByDescending(x => x.Id).FirstOrDefault();
            if (comment != null && comment.QueryStatus == CtmsCommentStatus.Open)
            {
                ModelState.AddModelError("Message", "Query is already generated.");
                return BadRequest(ModelState);
            }

            var ctmsMonitoringReportVariableValueQuery = _mapper.Map<CtmsMonitoringReportVariableValueQuery>(ctmsMonitoringReportVariableValueQueryDto);
            _ctmsMonitoringReportVariableValueQueryRepository.GenerateQuery(ctmsMonitoringReportVariableValueQueryDto, ctmsMonitoringReportVariableValueQuery, ctmsMonitoringReportVariableValue);

            var ctmsMonitoringReport = _ctmsMonitoringReportRepository.Find(ctmsMonitoringReportVariableValue.CtmsMonitoringReportId);
            //Changes made by Sachin
            ctmsMonitoringReport.ReportStatus = MonitoringReportStatus.ReviewInProgress;
            _ctmsMonitoringReportRepository.Update(ctmsMonitoringReport);

            _uow.Save();

            return Ok(ctmsMonitoringReportVariableValueQuery.Id);
        }

        /// Update Query
        /// Created By Swati
        [HttpPost("update")]
        [TransactionRequired]
        public IActionResult Update([FromBody] CtmsMonitoringReportVariableValueQueryDto CtmsMonitoringReportVariableValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoringReportVariable = _ctmsMonitoringReportVariableValueRepository.Find(CtmsMonitoringReportVariableValueQueryDto.CtmsMonitoringReportVariableValueId);
            if (manageMonitoringReportVariable.QueryStatus == Helper.CtmsCommentStatus.Answered)
            {
                ModelState.AddModelError("Message", "Query is already updated.");
                return BadRequest(ModelState);
            }

            var CtmsMonitoringReportVariableValueQuery = _mapper.Map<CtmsMonitoringReportVariableValueQuery>(CtmsMonitoringReportVariableValueQueryDto);

            _ctmsMonitoringReportVariableValueQueryRepository.UpdateQuery(CtmsMonitoringReportVariableValueQueryDto, CtmsMonitoringReportVariableValueQuery, manageMonitoringReportVariable);

            _uow.Save();
            return Ok(CtmsMonitoringReportVariableValueQuery.Id);
        }

        /// Delete Query
        /// Created By Swati
        [HttpPost("delete")]
        public IActionResult Delete([FromBody] CtmsMonitoringReportVariableValueQueryDto CtmsMonitoringReportVariableValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoringReportVariableValue = _ctmsMonitoringReportVariableValueRepository.Find(CtmsMonitoringReportVariableValueQueryDto.CtmsMonitoringReportVariableValueId);
            //if (manageMonitoringReportVariable.QueryStatus == CtmsCommentStatus.Answered ||
            //    manageMonitoringReportVariable.QueryStatus == CtmsCommentStatus.Resolved)
            //{
            //    ModelState.AddModelError("Message", "Query is already updated.");
            //    return BadRequest(ModelState);
            //}

            var CtmsMonitoringReportVariableValueQuery = _mapper.Map<CtmsMonitoringReportVariableValueQuery>(CtmsMonitoringReportVariableValueQueryDto);
            CtmsMonitoringReportVariableValueQuery.QueryStatus = CtmsCommentStatus.Closed;
            _ctmsMonitoringReportVariableValueQueryRepository.SaveCloseQuery(CtmsMonitoringReportVariableValueQuery);

            ctmsMonitoringReportVariableValue.QueryStatus = CtmsCommentStatus.Closed;
            _ctmsMonitoringReportVariableValueRepository.Update(ctmsMonitoringReportVariableValue);

            if (_uow.Save() <= 0) throw new Exception("Delete Query failed!");

            return Ok();
        }
    }
}