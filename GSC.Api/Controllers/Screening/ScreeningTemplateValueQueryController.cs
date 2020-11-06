using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.Medra;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateValueQueryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IEditCheckImpactRepository _editCheckImpactRepository;
        private readonly IScheduleRuleRespository _scheduleRuleRespository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        public ScreeningTemplateValueQueryController(
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IMeddraCodingRepository meddraCodingRepository,
            IScheduleRuleRespository scheduleRuleRespository,
            IEditCheckImpactRepository editCheckImpactRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _meddraCodingRepository = meddraCodingRepository;
            _uow = uow;
            _scheduleRuleRespository = scheduleRuleRespository;
            _mapper = mapper;
            _screeningTemplateRepository = screeningTemplateRepository;
            _editCheckImpactRepository = editCheckImpactRepository;
            _screeningVisitRepository = screeningVisitRepository;
        }

        [HttpGet("{screeningTemplateValueId}")]
        public IActionResult Get(int screeningTemplateValueId)
        {
            if (screeningTemplateValueId <= 0) return BadRequest();

            var auditsDto = _screeningTemplateValueQueryRepository.GetQueries(screeningTemplateValueId);

            return Ok(auditsDto);
        }

        [HttpPost("generate")]
        public IActionResult Generate([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningTemplateValue =
                _screeningTemplateValueRepository.Find(screeningTemplateValueQueryDto.ScreeningTemplateValueId);
            if (screeningTemplateValue.QueryStatus == QueryStatus.Open)
            {
                ModelState.AddModelError("Message", "Query is already generated.");
                return BadRequest(ModelState);
            }

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);

            _screeningTemplateValueQueryRepository.GenerateQuery(screeningTemplateValueQueryDto,
                screeningTemplateValueQuery, screeningTemplateValue);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Template Value Query failed on save.");

            return Ok(screeningTemplateValueQuery.Id);
        }

        [HttpPost("update")]
        [TransactionRequired]
        public IActionResult Update([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningTemplateValue =
                _screeningTemplateValueRepository.Find(screeningTemplateValueQueryDto.ScreeningTemplateValueId);
            if (screeningTemplateValue.QueryStatus == QueryStatus.Answered ||
                screeningTemplateValue.QueryStatus == QueryStatus.Resolved)
            {
                ModelState.AddModelError("Message", "Query is already updated.");
                return BadRequest(ModelState);
            }

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);

            _screeningTemplateValueQueryRepository.UpdateQuery(screeningTemplateValueQueryDto,
                screeningTemplateValueQuery, screeningTemplateValue);

            if (screeningTemplateValue.QueryStatus == QueryStatus.Resolved)
                // _meddraCodingRepository.UpdateSelfCorrection(screeningTemplateValueQueryDto.ScreeningTemplateValueId);

                if (_uow.Save() <= 0) throw new Exception("Updating Screening Template Value Query failed on save.");

            return Ok(screeningTemplateValueQuery.Id);
        }

        [HttpPost("review")]
        public IActionResult Review([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;

            var screeningTemplateValue =
                _screeningTemplateValueRepository.Find(screeningTemplateValueQueryDto.ScreeningTemplateValueId);
            if (screeningTemplateValue.QueryStatus == QueryStatus.Closed ||
                screeningTemplateValue.QueryStatus == QueryStatus.Reopened)
            {
                ModelState.AddModelError("Message",
                    "Query is already " + screeningTemplateValue.QueryStatus.GetDescription() + ".");
                return BadRequest(ModelState);
            }

            if (screeningTemplateValue.ReviewLevel != screeningTemplateValue.AcknowledgeLevel)
            {
                ModelState.AddModelError("Message", "Acknowledge Level pending!");
                return BadRequest(ModelState);
            }

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;
            screeningTemplateValueQuery.Value = value;

            _screeningTemplateValueQueryRepository.ReviewQuery(screeningTemplateValue, screeningTemplateValueQuery);

            if (_uow.Save() <= 0) throw new Exception("Reviewing Screening Template Value Query failed on save.");

            return Ok(screeningTemplateValueQuery.Id);
        }

        [HttpPost("self-generate")]
        [TransactionRequired]
        public IActionResult SelfGenerate([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(screeningTemplateValueQueryDto.ScreeningTemplateValueId);

            if (!screeningTemplateValueQueryDto.IsNa)
                if (!string.IsNullOrEmpty(value) &&
                    (screeningTemplateValueQueryDto.Children == null ||
                     screeningTemplateValueQueryDto.Children.Count == 0) &&
                    screeningTemplateValueQueryDto.Value == screeningTemplateValue.Value)
                {
                    ModelState.AddModelError("Message", "Same value not allowed!");
                    return BadRequest(ModelState);
                }

            var screeningTemplate = _screeningTemplateRepository.Find(screeningTemplateValue.ScreeningTemplateId);

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;
            screeningTemplateValueQuery.Value = value;
            _screeningTemplateValueQueryRepository.SelfGenerate(screeningTemplateValueQuery,
                screeningTemplateValueQueryDto, screeningTemplateValue, screeningTemplate);

            _meddraCodingRepository.UpdateSelfCorrection(screeningTemplateValueQueryDto.ScreeningTemplateValueId);

            if (_uow.Save() <= 0)
                throw new Exception("Creating Self Generate Screening Template Value Query failed on save.");

            var screeningEntryId = _screeningTemplateRepository.GeScreeningEntryId(screeningTemplate.Id);

            if (screeningTemplateValue.Children != null && screeningTemplateValue.Children.Count > 0)
                screeningTemplateValueQueryDto.Value = string.Join(",", _uow.Context.ScreeningTemplateValueChild.Where(x => x.ScreeningTemplateValueId == screeningTemplateValue.Id && x.Value == "true").Select(t => t.ProjectDesignVariableValueId));

            var editResult = _editCheckImpactRepository.VariableValidateProcess(screeningEntryId, screeningTemplate.Id,
                screeningTemplateValueQueryDto.IsNa ? "NA" : screeningTemplateValueQueryDto.Value, screeningTemplate.ProjectDesignTemplateId,
                screeningTemplateValue.ProjectDesignVariableId, screeningTemplateValueQueryDto.EditCheckIds, true, screeningTemplate.ScreeningVisitId);

            List<ScheduleCheckValidateDto> scheduleResult = null;
            if (screeningTemplateValueQueryDto.CollectionSource == CollectionSources.Date ||
                screeningTemplateValueQueryDto.CollectionSource == CollectionSources.DateTime ||
                screeningTemplateValueQueryDto.CollectionSource == CollectionSources.Time)
            {
                scheduleResult = _scheduleRuleRespository.ValidateByVariable(screeningEntryId, screeningTemplate.Id,
                   screeningTemplateValueQueryDto.Value, screeningTemplate.ProjectDesignTemplateId,
                   screeningTemplateValue.ProjectDesignVariableId, true);
            }

            var result = _scheduleRuleRespository.VariableResultProcess(editResult, scheduleResult);

            _screeningVisitRepository.AutomaticStatusUpdate(screeningTemplate.Id);

            _uow.Save();

            return Ok(result);
        }

        [HttpPost("AcknowledgeQuery")]
        public IActionResult AcknowledgeQuery([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var value = string.IsNullOrEmpty(screeningTemplateValueQueryDto.ValueName)
                ? screeningTemplateValueQueryDto.Value
                : screeningTemplateValueQueryDto.ValueName;

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);
            screeningTemplateValueQuery.OldValue = screeningTemplateValueQueryDto.OldValue;
            screeningTemplateValueQuery.Value = value;
            _screeningTemplateValueQueryRepository.AcknowledgeQuery(screeningTemplateValueQuery);

            if (_uow.Save() <= 0) throw new Exception("Acknowledge query failed on save!");

            return Ok(screeningTemplateValueQuery.Id);
        }



        [HttpPost("delete-query")]
        public IActionResult DeleteQuery([FromBody] ScreeningTemplateValueQueryDto screeningTemplateValueQueryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningTemplateValue =
                _screeningTemplateValueRepository.Find(screeningTemplateValueQueryDto.ScreeningTemplateValueId);
            if (screeningTemplateValue.QueryStatus == QueryStatus.Answered ||
                screeningTemplateValue.QueryStatus == QueryStatus.Resolved)
            {
                ModelState.AddModelError("Message", "Query is already updated.");
                return BadRequest(ModelState);
            }

            var screeningTemplateValueQuery = _mapper.Map<ScreeningTemplateValueQuery>(screeningTemplateValueQueryDto);
            screeningTemplateValueQuery.QueryStatus = QueryStatus.Closed;
            _screeningTemplateValueQueryRepository.ReviewQuery(screeningTemplateValue, screeningTemplateValueQuery);

            if (_uow.Save() <= 0) throw new Exception("DeleteQuery failed!");

            return Ok();
        }

        //[HttpGet]
        //[Route("GetDashboardQueryStatusByVisit/{projectId}")]
        //public IActionResult GetDashboardQueryStatusByVisit(int projectId)
        //{
        //    return Ok(_screeningTemplateValueQueryRepository.GetDashboardQueryStatusByVisit(projectId));
        //}

        //[HttpGet]
        //[Route("GetDashboardQueryStatusBySite/{projectId}")]
        //public IActionResult GetDashboardQueryStatusBySite(int projectId)
        //{
        //    return Ok(_screeningTemplateValueQueryRepository.GetDashboardQueryStatusBySite(projectId));
        //}

        //[HttpGet]
        //[Route("GetDashboardQueryStatusByRolewise/{projectId}")]
        //public IActionResult GetDashboardQueryStatusByRolewise(int projectId)
        //{
        //    return Ok(_screeningTemplateValueQueryRepository.GetDashboardQueryStatusByRolewise(projectId));
        //}

        //[HttpGet]
        //[Route("GetDashboardQueryStatusByVisitwise/{projectId}")]
        //public IActionResult GetDashboardQueryStatusByVisitwise(int projectId)
        //{
        //    return Ok(_screeningTemplateValueQueryRepository.GetDashboardQueryStatusByVisitwise(projectId));
        //}
    }
}