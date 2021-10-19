using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ManageMonitoringReportVariableController : BaseController
    {
        private readonly IManageMonitoringReportVariableRepository _manageMonitoringReportVariableRepository;
        private readonly IManageMonitoringReportVariableAuditRepository _manageMonitoringReportVariableAuditRepository;
        private readonly IManageMonitoringReportVariableChildRepository _manageMonitoringReportVariableChildRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringReportVariableController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IManageMonitoringReportVariableRepository manageMonitoringReportVariableRepository,
            IManageMonitoringReportVariableAuditRepository manageMonitoringReportVariableAuditRepository,
            IManageMonitoringReportVariableChildRepository manageMonitoringReportVariableChildRepository,
            IVariableTemplateRepository variableTemplateRepository)
        {
            _manageMonitoringReportVariableRepository = manageMonitoringReportVariableRepository;
            _manageMonitoringReportVariableAuditRepository = manageMonitoringReportVariableAuditRepository;
            _manageMonitoringReportVariableChildRepository = manageMonitoringReportVariableChildRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ManageMonitoringReportVariableDto manageMonitoringReportVariableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageMonitoringReportVariableDto.Id = 0;
            var manageMonitoringReportVariable = _mapper.Map<ManageMonitoringReportVariable>(manageMonitoringReportVariableDto);

            _manageMonitoringReportVariableRepository.Add(manageMonitoringReportVariable);
            if (_uow.Save() <= 0) throw new Exception("Creating value failed on save.");
            return Ok(manageMonitoringReportVariable.Id);

        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ManageMonitoringReportVariableDto manageMonitoringReportVariableDto)
        {
            if (manageMonitoringReportVariableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoringReportVariable = _mapper.Map<ManageMonitoringReportVariable>(manageMonitoringReportVariableDto);

            _manageMonitoringReportVariableRepository.AddOrUpdate(manageMonitoringReportVariable);

            if (_uow.Save() <= 0) throw new Exception("Updating value failed on save.");
            return Ok(manageMonitoringReportVariable.Id);
        }

        [HttpGet]
        [Route("GetTemplate/{id}/{manageMonitoringReportId}")]
        public IActionResult GetTemplate(int id, int manageMonitoringReportId)
        {
            var designTemplate = _variableTemplateRepository.GetReportTemplate(id);
            return Ok(_manageMonitoringReportVariableRepository.GetReportTemplateVariable(designTemplate, manageMonitoringReportId));
        }

        [HttpPut]
        [TransactionRequired]
        [Route("SaveVariableValue")]
        public IActionResult SendByApprover([FromBody] ManageMonitoringReportValueSaveDto manageMonitoringReportValueSaveDto)
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
            }
            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(manageMonitoringReportValueSaveDto.MonitoringVariableValueList[0].Id);
        }
    }
}