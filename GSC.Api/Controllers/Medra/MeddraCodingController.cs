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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IAttendanceHistoryRepository _attendanceHistoryRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly INoneRegisterRepository _noneRegisterRepository;
        private readonly IStudyScopingRepository _studyScopingRepository;
        private readonly IMeddraLowLevelTermRepository _meddraLowLevelTermRepository;


        public MeddraCodingController(IMeddraCodingRepository meddraCodingRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            INoneRegisterRepository noneRegisterterRepository,
            IProjectSubjectRepository projectSubjectRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository,
            IStudyScopingRepository studyScopingRepository,
            IMeddraLowLevelTermRepository meddraLowLevelTermRepository)

        {
            _meddraCodingRepository = meddraCodingRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _noneRegisterRepository = noneRegisterterRepository;
            _projectSubjectRepository = projectSubjectRepository;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _studyScopingRepository = studyScopingRepository;
            _meddraLowLevelTermRepository = meddraLowLevelTermRepository;
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] MeddraCodingSearchDto search)
        {
            var variables = _meddraCodingRepository.SearchMain(search);
            return Ok(variables);
        }

        [HttpGet]
        [Route("GetVariableCount")]
        public IActionResult GetVariableCount([FromQuery] MeddraCodingSearchDto filters)
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

        [HttpGet]
        [Route("GetMedDRACodingDetails")]
        public IActionResult GetMedDRACodingDetails([FromQuery] MeddraCodingSearchDto filters)
        {
            if (filters.ProjectDesignVariableId <= 0) return BadRequest();
            return Ok(_meddraCodingRepository.GetMedDRACodingDetails(filters));
        }

        [HttpGet]
        [Route("GetAutoCodes")]
        public IActionResult AutoCodes([FromQuery] MeddraCodingSearchDto filters)
        {
            if (filters.ProjectDesignVariableId <= 0) return BadRequest();
            return Ok(_meddraCodingRepository.AutoCodes(filters));
        }

        [HttpPost]
        [Route("AddAutoCodes")]
        public IActionResult AddAutoCodes([FromBody] MeddraCodingDto data)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            data.Id = 0;
            data.ModifiedDate = DateTime.Now;
            data.ModifiedBy = _jwtTokenAccesser.UserId;
            var autoCode = _mapper.Map<MeddraCoding>(data);
            _meddraCodingRepository.Add(autoCode);
            if (_uow.Save() <= 0) throw new Exception("Creating Drug failed on save.");
            return Ok(autoCode.Id);
        }

        [HttpGet]
        [Route("GetManualCodes")]
        public IActionResult GetManualCodes([FromQuery] MeddraCodingSearchDto filters)
        {
            return Ok(_meddraLowLevelTermRepository.GetManualCodes(filters));
        }
    }
}