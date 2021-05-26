using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanController : ControllerBase
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IGSCContext _context;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;
        private readonly ITaskMasterRepository _taskMasterRepository;

        public StudyPlanController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IStudyPlanRepository studyPlanRepository, IGSCContext context, IStudyPlanTaskRepository studyPlanTaskRepository, ITaskMasterRepository taskMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _studyPlanRepository = studyPlanRepository;
            _context = context;
            _studyPlanTaskRepository = studyPlanTaskRepository;
            _taskMasterRepository = taskMasterRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var studyplan = _studyPlanRepository.GetStudyplanList(isDeleted);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var studyplan = _studyPlanRepository.Find(id);
            var studyplandetail = _mapper.Map<StudyPlanDto>(studyplan);
            return Ok(studyplandetail);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] StudyPlanDto studyplanDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyplanDto.Id = 0;
            var studyplan = _mapper.Map<StudyPlan>(studyplanDto);
            var validatecode = _studyPlanRepository.Duplicate(studyplan);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            _studyPlanRepository.Add(studyplan);
            if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");

            var validate = _studyPlanRepository.ImportTaskMasterData(studyplan);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    _studyPlanRepository.Remove(studyplan);
            //    _uow.Save();
            //    return BadRequest(ModelState);
            //}           
            _uow.Save();
            return Ok(studyplan.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanDto studyplanDto)
        {
            if (studyplanDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var studyplan = _mapper.Map<StudyPlan>(studyplanDto);
            var validatecode = _studyPlanRepository.Duplicate(studyplan);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            _studyPlanRepository.Update(studyplan);
            if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");
            return Ok(studyplan.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanRepository.Find(id);

            if (record == null)
                return NotFound();

            _studyPlanRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanRepository.Find(id);
            if (record == null)
                return NotFound();
            var validatecode = _studyPlanRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }
            _studyPlanRepository.Active(record);
            _uow.Save();

            return Ok();
        }


    }
}
