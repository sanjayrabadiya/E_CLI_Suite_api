using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class StudyPlanResourceController : BaseController
    {

        private readonly IStudyPlanResourceRepository _studyPlanTaskResourceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public StudyPlanResourceController(IStudyPlanResourceRepository studyPlanTaskResourceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IGSCContext context)
        {
            _studyPlanTaskResourceRepository = studyPlanTaskResourceRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}/{PlanTaskId}")]
        public IActionResult Get(bool isDeleted, int PlanTaskId)
        {
            var taskResource = _studyPlanTaskResourceRepository.GetTaskResourceList(isDeleted, PlanTaskId);
            return Ok(taskResource);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var taskResource = _studyPlanTaskResourceRepository.ResourceById(id);
            return Ok(taskResource);
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyPlanResourceDto studyPlanResourceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyPlanResourceDto.Id = 0;

            var taskResource = _mapper.Map<StudyPlanResource>(studyPlanResourceDto);
                var validate = _studyPlanTaskResourceRepository.Duplicate(taskResource);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
                _studyPlanTaskResourceRepository.Add(taskResource);        

            if (_uow.Save() <= 0) throw new Exception("Creating Resource failed on save.");

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanResourceDto studyPlanResourceDto)
        {
            if (studyPlanResourceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskResource = _mapper.Map<StudyPlanResource>(studyPlanResourceDto);
            var validate = _studyPlanTaskResourceRepository.Duplicate(taskResource);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _studyPlanTaskResourceRepository.Update(taskResource);

            if (_uow.Save() <= 0) throw new Exception("Updating Resource failed on save.");
            return Ok(taskResource.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanTaskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            _studyPlanTaskResourceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanTaskResourceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _studyPlanTaskResourceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyPlanTaskResourceRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}