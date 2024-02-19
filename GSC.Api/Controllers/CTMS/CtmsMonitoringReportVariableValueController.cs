using System;
using System.Linq;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportVariableValueController : BaseController
    {
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly ICtmsMonitoringReportVariableValueRepository _ctmsMonitoringReportVariableValueRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IGSCContext _context;
        public CtmsMonitoringReportVariableValueController(IUnitOfWork uow,
            IStudyLevelFormRepository studyLevelFormRepository,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository,
            ICtmsMonitoringReportVariableValueRepository ctmsMonitoringReportVariableValueRepository,
            IUploadSettingRepository uploadSettingRepository, IGSCContext context)
        {
            _studyLevelFormRepository = studyLevelFormRepository;
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _ctmsMonitoringReportVariableValueRepository = ctmsMonitoringReportVariableValueRepository;
            _uow = uow;
            _uploadSettingRepository = uploadSettingRepository;
            _context = context;
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
            _ctmsMonitoringReportVariableValueRepository.SaveVariableValue(ctmsMonitoringReportVariableValueSaveDto);

            if (_uow.Save() <= 0) throw new Exception("Updating Variable failed on save.");
            return Ok(ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList);
        }

        [HttpGet("GetQueryStatusByReportId/{id}")]
        public IActionResult GetQueryStatusByReportId(int id)
        {
            return Ok(_ctmsMonitoringReportVariableValueRepository.GetQueryStatusByReportId(id));
        }

        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] CtmsMonitoringReportVariableValueDto ctmsMonitoringReportVariableValueSaveDto)
        {
            if (ctmsMonitoringReportVariableValueSaveDto.Id <= 0) return BadRequest();

            var screeningTemplateValue = _context.CtmsMonitoringReportVariableValue
                                         .Include(x => x.CtmsMonitoringReport)
                                         .ThenInclude(x => x.CtmsMonitoring)
                                         .ThenInclude(x => x.Project)
                                         .Where(x => x.Id == ctmsMonitoringReportVariableValueSaveDto.Id).FirstOrDefault();
            if (screeningTemplateValue != null)
            {
                if (ctmsMonitoringReportVariableValueSaveDto.FileModel?.Base64?.Length > 0)
                {
                    var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit((int)screeningTemplateValue.CtmsMonitoringReport.CtmsMonitoring.Project.ParentProjectId);
                    if (!string.IsNullOrEmpty(validateuploadlimit))
                    {
                        ModelState.AddModelError("Message", validateuploadlimit);
                        return BadRequest(ModelState);
                    }
                }

                _ctmsMonitoringReportVariableValueRepository.UploadDocument(ctmsMonitoringReportVariableValueSaveDto);
            }

            return Ok(ctmsMonitoringReportVariableValueSaveDto);
        }
    }
}