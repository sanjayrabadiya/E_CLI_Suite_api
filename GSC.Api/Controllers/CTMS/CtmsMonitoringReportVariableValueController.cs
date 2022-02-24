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
        private readonly IManageMonitoringReportVariableRepository _manageMonitoringReportVariableRepository;
        private readonly IManageMonitoringReportRepository _manageMonitoringReportRepository;
        private readonly IManageMonitoringReportVariableAuditRepository _manageMonitoringReportVariableAuditRepository;
        private readonly IManageMonitoringReportVariableChildRepository _manageMonitoringReportVariableChildRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringReportVariableValueController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IStudyLevelFormRepository studyLevelFormRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository,
            IStudyLevelFormVariableRepository studyLevelFormVariableRepository,
            IManageMonitoringReportVariableRepository manageMonitoringReportVariableRepository,
            IManageMonitoringReportVariableAuditRepository manageMonitoringReportVariableAuditRepository,
            IManageMonitoringReportVariableChildRepository manageMonitoringReportVariableChildRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IManageMonitoringReportRepository manageMonitoringReportRepository)
        {
            _studyLevelFormRepository = studyLevelFormRepository;
            _studyLevelFormVariableRepository = studyLevelFormVariableRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
            _manageMonitoringReportVariableAuditRepository = manageMonitoringReportVariableAuditRepository;
            _manageMonitoringReportVariableChildRepository = manageMonitoringReportVariableChildRepository;
            _manageMonitoringReportRepository = manageMonitoringReportRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        /// Get Form & Variable & VariableValue by manageMonitoringReportId
        /// Created By Swati
        [HttpGet]
        [Route("GetReportFormVariable/{Id}/{StudyLevelFormId}")]
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
        public IActionResult SaveVariableValue([FromBody] ManageMonitoringReportValueSaveDto manageMonitoringReportValueSaveDto)
        {
            if (manageMonitoringReportValueSaveDto.MonitoringVariableValueList != null)
            {
                foreach (var item in manageMonitoringReportValueSaveDto.MonitoringVariableValueList)
                {
                    var value = _manageMonitoringReportVariableRepository.GetValueForAudit(item);
                    var manageMonitoringReportVariable = _mapper.Map<ManageMonitoringReportVariable>(item);

                    var Exists = _manageMonitoringReportVariableRepository.All.Where(x => x.DeletedDate == null && x.ManageMonitoringReportId == manageMonitoringReportVariable.ManageMonitoringReportId && x.VariableId == item.VariableId).FirstOrDefault();

                    ManageMonitoringReportVariableAudit audit = new ManageMonitoringReportVariableAudit();

                    if (Exists == null)
                    {
                        manageMonitoringReportVariable.Id = 0;
                        _manageMonitoringReportVariableRepository.Add(manageMonitoringReportVariable);

                        var aduit = new ManageMonitoringReportVariableAudit
                        {
                            ManageMonitoringReportVariable = manageMonitoringReportVariable,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _manageMonitoringReportVariableAuditRepository.Save(aduit);
                        _manageMonitoringReportVariableChildRepository.Save(manageMonitoringReportVariable);
                    }
                    else
                    {
                        var aduit = new ManageMonitoringReportVariableAudit
                        {
                            ManageMonitoringReportVariableId = Exists.Id,
                            Value = item.IsNa ? "N/A" : value,
                            OldValue = item.OldValue,
                        };
                        _manageMonitoringReportVariableAuditRepository.Save(aduit);
                        if (item.IsDeleted)
                            _manageMonitoringReportVariableRepository.DeleteChild(Exists.Id);

                        _manageMonitoringReportVariableChildRepository.Save(manageMonitoringReportVariable);

                        manageMonitoringReportVariable.Id = Exists.Id;
                        _manageMonitoringReportVariableRepository.Update(manageMonitoringReportVariable);
                    }
                }

                var manageMonitoringReport = _manageMonitoringReportRepository.Find(manageMonitoringReportValueSaveDto.MonitoringVariableValueList[0].ManageMonitoringReportId);
                manageMonitoringReport.Status = MonitoringReportStatus.Initiated;
                _manageMonitoringReportRepository.Update(manageMonitoringReport);
            }
            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(manageMonitoringReportValueSaveDto.MonitoringVariableValueList[0].Id);
        }

        [HttpGet("GetQueryStatusByReportId/{id}")]
        public IActionResult GetQueryStatusByReportId(int id)
        {
            return Ok(_manageMonitoringReportVariableRepository.GetQueryStatusByReportId(id));
        }
    }
}