using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using GSC.Api.Controllers.Common;
using GSC.Respository.Medra;
using GSC.Respository.Configuration;
using GSC.Data.Dto.Medra;
using GSC.Shared.DocumentService;
using GSC.Data.Entities.Medra;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.JWTAuth;

namespace GSC.Api.Controllers.Medra
{
    [Route("api/[controller]")]
    public class StudyScopingController : BaseController
    {
        private readonly IStudyScopingRepository _studyScopingRepository;
        private readonly IMeddraCodingRepository _meddraCodingRepository;
        private readonly IMedraConfigRepository _medraConfigRepository;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IDocumentTypeRepository _documentTypeRepository;

        public StudyScopingController(IMedraConfigRepository medraConfigRepository,
            IMeddraCodingRepository meddraCodingRepository,
           IStudyScopingRepository studyScopingRepository,
           IDictionaryRepository dictionaryRepository,
           IUserRepository userRepository,
        IUnitOfWork uow,
          IMapper mapper,
          IJwtTokenAccesser jwtTokenAccesser,
          IDocumentTypeRepository documentTypeRepository
          )
        {
            _studyScopingRepository = studyScopingRepository;
            _meddraCodingRepository = meddraCodingRepository;
            _medraConfigRepository = medraConfigRepository;
            _dictionaryRepository = dictionaryRepository;
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _documentTypeRepository = documentTypeRepository;
        }

        [HttpGet]
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var studyscoping = _studyScopingRepository.FindByInclude(x => (x.CompanyId == null
                                                           || x.CompanyId == _jwtTokenAccesser.CompanyId) && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();

            var studyscopingDto = _mapper.Map<IEnumerable<StudyScoping>>(studyscoping).ToList();
            return Ok(studyscopingDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var studyscoping = _studyScopingRepository.Find(id);
            var studyscopingDto = _mapper.Map<StudyScoping>(studyscoping);
            return Ok(studyscopingDto);
        }

        [HttpGet("GetStudyScopingList/{projectId}")]
        public IActionResult GetStudyScopingList(int projectId)
        {
            return Ok(_studyScopingRepository.GetStudyScopingList(projectId));
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyScopingDto studyScopingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            studyScopingDto.Id = 0;
            if (studyScopingDto.ScopingBy == 2) studyScopingDto.IsByAnnotation = true;

            foreach (var variable in studyScopingDto.ProjectDesignVariableIds)
            {
                studyScopingDto.ProjectDesignVariableId = variable;
                var studyScoping = _mapper.Map<StudyScoping>(studyScopingDto);
                var validate = _studyScopingRepository.Duplicate(studyScoping);
                if (string.IsNullOrEmpty(validate))
                {
                    _studyScopingRepository.Add(studyScoping);
                    if (_uow.Save() <= 0) throw new Exception($"Creating study Scoping failed on save.");
                }

            }
            return Ok(studyScopingDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyScopingDto studyScopingDto)
        {
            if (studyScopingDto.Id <= 0) return BadRequest();
            if (studyScopingDto.ScopingBy == 2) studyScopingDto.IsByAnnotation = true;

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var result = _studyScopingRepository.All.AsNoTracking().Where(x => x.Id == studyScopingDto.Id).FirstOrDefault();
            var check = _studyScopingRepository.checkForScopingEdit(result.ProjectDesignVariableId);
            if (check)
            {
                if (studyScopingDto.MedraConfigId != result.MedraConfigId)
                {
                    result.MedraConfigId = studyScopingDto.MedraConfigId;
                    _meddraCodingRepository.UpdateScopingVersion(result);
                }
            }

            var studyScoping = _mapper.Map<StudyScoping>(studyScopingDto);
            var validate = _studyScopingRepository.Duplicate(studyScoping);
            if (string.IsNullOrEmpty(validate))
            {
                _studyScopingRepository.Update(studyScoping);
                // if (_uow.Save() <= 0) throw new Exception($"Updating Study Scoping failed on save.");
            }
            _uow.Save();
            return Ok(studyScoping.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyScopingRepository.Find(id);

            if (record == null)
                return NotFound();

            _studyScopingRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyScopingRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _studyScopingRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyScopingRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
