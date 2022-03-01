using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportVariableValueController : BaseController
    {
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IStudyLevelFormVariableRepository _studyLevelFormVariableRepository;
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly ICtmsMonitoringReportVariableValueAuditRepository _ctmsMonitoringReportVariableValueAuditRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringReportVariableValueController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IStudyLevelFormRepository studyLevelFormRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            ICtmsMonitoringReportVariableValueAuditRepository ctmsMonitoringReportVariableValueAuditRepository,
            ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository,
            IStudyLevelFormVariableRepository studyLevelFormVariableRepository,
            IVariableTemplateRepository variableTemplateRepository)
        {
            _studyLevelFormRepository = studyLevelFormRepository;
            _studyLevelFormVariableRepository = studyLevelFormVariableRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _ctmsMonitoringReportVariableValueAuditRepository = ctmsMonitoringReportVariableValueAuditRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        /// Get Form & Variable & VariableValue by manageMonitoringReportId
        /// Created By Swati
        [HttpGet]
        [Route("GetReportFormVariable/{Id}/{CtmsMonitoringReportId}")]
        public IActionResult GetReportFormVariable([FromRoute] int Id, int CtmsMonitoringReportId)
        {
            var designTemplate = _studyLevelFormRepository.GetReportFormVariable(Id);

            return Ok(_ctmsMonitoringReportRepository.GetCtmsMonitoringReportVariableValue(designTemplate, CtmsMonitoringReportId));
        }

        /// Save Variable value 
        /// Created By Swati
        [HttpPut]
        [TransactionRequired]
        [Route("SaveVariableValue")]
        public IActionResult SaveVariableValue([FromBody] CtmsMonitoringReportVariableValueSaveDto ctmsMonitoringReportVariableValueSaveDto)
        {
            if (ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList != null)
            {
                foreach (var item in ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList)
                {
                    var value = _ctmsMonitoringReportVariableValueRepository.GetValueForAudit(item);
                    var ctmsMonitoringReportVariableValue = _mapper.Map<CtmsMonitoringReportVariableValue>(item);

                    var Exists = _ctmsMonitoringReportVariableValueRepository.All.Where(x => x.DeletedDate == null && x.CtmsMonitoringReportId == ctmsMonitoringReportVariableValue.CtmsMonitoringReportId && x.StudyLevelFormVariableId == item.StudyLevelFormVariableId).FirstOrDefault();

                    ManageMonitoringReportVariableAudit audit = new ManageMonitoringReportVariableAudit();

                    if (Exists == null)
                    {
                        ctmsMonitoringReportVariableValue.Id = 0;
                        _ctmsMonitoringReportVariableValueRepository.Add(ctmsMonitoringReportVariableValue);

                        var aduit = new CtmsMonitoringReportVariableValueAudit
                        {
                            CtmsMonitoringReportVariableValue = ctmsMonitoringReportVariableValue,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                        _ctmsMonitoringReportVariableValueChildRepository.Save(ctmsMonitoringReportVariableValue);
                    }
                    else
                    {
                        var aduit = new CtmsMonitoringReportVariableValueAudit
                        {
                            CtmsMonitoringReportVariableValueId = Exists.Id,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                        if (item.IsDeleted)
                            _ctmsMonitoringReportVariableValueRepository.DeleteChild(Exists.Id);

                        _ctmsMonitoringReportVariableValueChildRepository.Save(ctmsMonitoringReportVariableValue);

                        ctmsMonitoringReportVariableValue.Id = Exists.Id;
                        _ctmsMonitoringReportVariableValueRepository.Update(ctmsMonitoringReportVariableValue);
                    }
                }

                var ctmsMonitoringReport = _ctmsMonitoringReportRepository.Find(ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList[0].CtmsMonitoringReportId);
                ctmsMonitoringReport.ReportStatus = MonitoringReportStatus.Initiated;
                _ctmsMonitoringReportRepository.Update(ctmsMonitoringReport);
            }
            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList[0].Id);
        }

        [HttpGet("GetQueryStatusByReportId/{id}")]
        public IActionResult GetQueryStatusByReportId(int id)
        {
            return Ok();
            //return Ok(_manageMonitoringReportVariableRepository.GetQueryStatusByReportId(id));
        }
    }
}