using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Medra;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeddraCodingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        private readonly IMeddraCodingAuditRepository _meddraCodingAuditRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IAttendanceHistoryRepository _attendanceHistoryRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IStudyScopingRepository _studyScopingRepository;
        private readonly IMeddraLowLevelTermRepository _meddraLowLevelTermRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;


        public MeddraCodingController(IMeddraCodingRepository meddraCodingRepository,
            IMeddraCodingAuditRepository meddraCodingAuditRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IRandomizationRepository randomizationRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository,
            IStudyScopingRepository studyScopingRepository,
            IMeddraLowLevelTermRepository meddraLowLevelTermRepository,
            IScreeningEntryRepository screeningEntryRepository,
            IProjectDesignRepository projectDesignRepository)

        {
            _meddraCodingRepository = meddraCodingRepository;
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _randomizationRepository = randomizationRepository;
            _projectSubjectRepository = projectSubjectRepository;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _studyScopingRepository = studyScopingRepository;
            _meddraLowLevelTermRepository = meddraLowLevelTermRepository;
            _screeningEntryRepository = screeningEntryRepository;
            _projectDesignRepository = projectDesignRepository;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] MeddraCodingSearchDto search)
        {
            var variables = _meddraCodingRepository.SearchMain(search);
            return Ok(variables);
        }

        [HttpPost]
        [Route("GetVariableCount")]
        public IActionResult GetVariableCount([FromBody] MeddraCodingSearchDto filters)
        {
            var variables = _meddraCodingRepository.GetVariableCount(filters);
            return Ok(variables);
        }

        [HttpPost]
        [Route("SearchCodingDetails")]
        public IActionResult SearchCodingDetails([FromBody] MeddraCodingSearchDto search)
        {
            var variables = _meddraCodingRepository.SearchMain(search);
            return Ok(variables);
        }

        [HttpGet]
        [Route("MeddraCodingVariableDropDown/{ProjectId}")]
        public IActionResult MeddraCodingVariableDropDown(int ProjectId)
        {
            return Ok(_meddraCodingRepository.MeddraCodingVariableDropDown(ProjectId));
        }

        [HttpPost]
        [Route("GetMedDRACodingDetails")]
        public IActionResult GetMedDRACodingDetails([FromBody] MeddraCodingSearchDto filters)
        {
            if (filters.ProjectDesignVariableId <= 0) return BadRequest();
            return Ok(_meddraCodingRepository.GetMedDRACodingDetails(filters));
        }

        [HttpPost]
        [Route("GetAutoCodes")]
        public IActionResult AutoCodes([FromBody] MeddraCodingSearchDto filters)
        {
            if (filters.ProjectDesignVariableId <= 0) return BadRequest();
            return Ok(_meddraCodingRepository.AutoCodes(filters));
        }

        [HttpPost]
        [Route("AddAutoCodes")]
        public IActionResult AddAutoCodes([FromBody] MeddraCodingDto data)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var recodeData = _meddraCodingRepository.CheckForRecode(data.ScreeningTemplateValueId);

            if (recodeData != null)
            {
                recodeData.ModifiedDate = DateTime.Now;
                recodeData.ModifiedBy = _jwtTokenAccesser.UserId;
                recodeData.CreatedRole = _jwtTokenAccesser.RoleId;
                recodeData.MeddraLowLevelTermId = data.MeddraLowLevelTermId;
                recodeData.MeddraSocTermId = data.MeddraSocTermId;
                var medra = _mapper.Map<MeddraCoding>(recodeData);
                _meddraCodingRepository.Update(medra);

                if (_uow.Save() <= 0)
                {
                    throw new Exception($"Coding failed on save.");
                }
                var meddraCodingAudit = _meddraCodingAuditRepository.SaveAudit(null, medra.Id, data.MeddraLowLevelTermId, data.MeddraSocTermId, "Recoded By Auto Coded", null, null);
                _uow.Save();
                return Ok(medra.Id);
            }
            else
            {
                data.ModifiedDate = DateTime.Now;
                data.ModifiedBy = _jwtTokenAccesser.UserId;
                data.CreatedRole = _jwtTokenAccesser.RoleId;
                data.CodedType = CodedType.AutoCoded;
                data.CodingType= CodedType.AutoCoded;
                data.IsApproved = false;
                var autoCode = _mapper.Map<MeddraCoding>(data);
                _meddraCodingRepository.Add(autoCode);
                if (_uow.Save() <= 0) throw new Exception("Coding failed on save.");
                var meddraCodingAudit = _meddraCodingAuditRepository.SaveAudit(null, autoCode.Id, data.MeddraLowLevelTermId, data.MeddraSocTermId, "Added By Auto Coded", null, null);
                _uow.Save();
                return Ok(autoCode.Id);
            }
        }

        [HttpPost]
        [Route("AddManualCodes")]
        public IActionResult AddManualCodes([FromBody] MeddraCodingDto data)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var item in data.ScreeningTemplateValueIds)
            {
                data.Id = 0;
                data.ScreeningTemplateValueId = (int)item;
                var recodeData = _meddraCodingRepository.CheckForRecode((int)item);
                if (recodeData != null)
                {
                    recodeData.ModifiedDate = DateTime.Now;
                    recodeData.ModifiedBy = _jwtTokenAccesser.UserId;
                    recodeData.CreatedRole = _jwtTokenAccesser.RoleId;
                    recodeData.MeddraLowLevelTermId = data.MeddraLowLevelTermId;
                    recodeData.MeddraSocTermId = data.MeddraSocTermId;
                    var medra = _mapper.Map<MeddraCoding>(recodeData);
                    _meddraCodingRepository.Update(medra);
                    if (_uow.Save() <= 0)
                    {
                        throw new Exception($"Coding failed on save.");
                    }
                    var meddraCodingAudit = _meddraCodingAuditRepository.SaveAudit(null, medra.Id, data.MeddraLowLevelTermId, data.MeddraSocTermId, "Recoded By Manual Coded", null, null);
                    _uow.Save();
                }
                else
                {
                    data.CodedType = CodedType.ManualCoded;
                    data.CodingType = CodedType.ManualCoded;
                    data.IsApproved = false;
                    data.ModifiedDate = DateTime.Now;
                    data.ModifiedBy = _jwtTokenAccesser.UserId;
                    data.CreatedRole = _jwtTokenAccesser.RoleId;
                    var autoCode = _mapper.Map<MeddraCoding>(data);
                    _meddraCodingRepository.Add(autoCode);
                    if (_uow.Save() <= 0) throw new Exception("Coding failed on save.");
                    var meddraCodingAudit = _meddraCodingAuditRepository.SaveAudit(null, autoCode.Id, data.MeddraLowLevelTermId, data.MeddraSocTermId, "Added By Manual Coded", null, null);
                    _uow.Save();
                }
            }
            return Ok();
        }

        [HttpPost]
        [Route("ApproveCoding")]
        public IActionResult ApproveCoding([FromBody] MeddraCodingDto data)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var item in data.ScreeningTemplateValueIds)
            {
                data.Id = 0;
                data.ScreeningTemplateValueId = (int)item;
                var recodeData = _meddraCodingRepository.GetRecordForComment((int)item);
                recodeData.ApproveDate = DateTime.Now;
                recodeData.ApprovedBy = _jwtTokenAccesser.UserId;
                recodeData.IsApproved = true;
                var medra = _mapper.Map<MeddraCoding>(recodeData);
                _meddraCodingRepository.Update(medra);
                var meddraCodingAudit = _meddraCodingAuditRepository.SaveAudit(null, medra.Id, null, null, "Approval Code", null, null);

                if (_uow.Save() <= 0)
                {
                    throw new Exception($"Coding failed on save.");
                }
            }
            return Ok();
        }

        [HttpPost]
        [Route("GetManualCodes")]
        public IActionResult GetManualCodes([FromBody] MeddraCodingSearchDto filters)
        {
            return Ok(_meddraLowLevelTermRepository.GetManualCodes(filters));
        }

        [HttpGet("GetProjectStatusAndLevelDropDown/{ProjectDesignId}")]
        public IActionResult GetProjectStatusAndLevelDropDown(int ProjectDesignId)
        {
            var parentProjectId = _projectDesignRepository.GetParentProjectDetail(ProjectDesignId);
            var screeningSummaryDto = _screeningEntryRepository.GetProjectStatusAndLevelDropDown(parentProjectId);
            return Ok(screeningSummaryDto);
        }

        [HttpGet]
        [Route("GetCoderandApprovalProfile/{ProjectDesignVariableId}")]
        public IActionResult GetCoderandApprovalProfile(int ProjectDesignVariableId)
        {
            var variables = _meddraCodingRepository.GetCoderandApprovalProfile(ProjectDesignVariableId);
            return Ok(variables);
        }
    }
}