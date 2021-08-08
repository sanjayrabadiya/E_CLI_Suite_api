using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyVersionController : BaseController
    {
        private readonly IStudyVersionRepository _studyVersionRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IStudyVersionVisitStatusRepository _studyVersionVisitStatusRepository;

        public StudyVersionController(IStudyVersionRepository studyVersionRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context, IStudyVersionVisitStatusRepository studyVersionVisitStatusRepository)
        {
            _studyVersionRepository = studyVersionRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _studyVersionVisitStatusRepository = studyVersionVisitStatusRepository;
        }


        //added by vipul for get versions list by projectdesign on 01062021
        [HttpGet]
        [Route("GetDetails/{projectDesignId}")]
        public IActionResult GetDetails(int projectDesignId)
        {
            var manageSite = _studyVersionRepository.GetStudyVersion(projectDesignId);
            return Ok(manageSite);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var studyVersion = _studyVersionRepository
                    .FindByInclude(x => x.Id == id, x => x.StudyVersionVisitStatus).SingleOrDefault();
            if (studyVersion == null)
                return BadRequest();

            if (studyVersion != null && studyVersion.StudyVersionVisitStatus != null)
                studyVersion.StudyVersionVisitStatus = studyVersion.StudyVersionVisitStatus.Where(x => x.DeletedDate == null).ToList();

            var studyVersionDto = _mapper.Map<StudyVersion>(studyVersion);
            return Ok(studyVersionDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyVersionDto studyVersionDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyVersionDto.Id = 0;
            studyVersionDto.VersionStatus = Helper.VersionStatus.OnTrial;
            var studyVersion = _mapper.Map<StudyVersion>(studyVersionDto);
            var validate = _studyVersionRepository.Duplicate(studyVersion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _studyVersionRepository.Add(studyVersion);

            foreach (var item in studyVersion.StudyVersionVisitStatus)
            {
                item.StudyVerion = studyVersion;
                _studyVersionVisitStatusRepository.Add(item);
            }

            _uow.Save();

            return Ok(studyVersion.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] StudyVersionDto studyVersionDto)
        {
            if (studyVersionDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var studyVersion = _mapper.Map<StudyVersion>(studyVersionDto);

            var validate = _studyVersionRepository.Duplicate(studyVersion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyVersionRepository.UpdateVisitStatus(studyVersion);
            /* Added by Darshil for effective Date on 24-07-2020 */
            _studyVersionRepository.Update(studyVersion);

            if (_uow.Save() <= 0) throw new Exception("Updating study version failed on save.");
            return Ok(studyVersion.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _studyVersionRepository.Find(id);

            if (record == null)
                return NotFound();

            _studyVersionRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _studyVersionRepository.Find(id);

            if (record == null)
                return NotFound();
            _studyVersionRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetVersionNumber/{projectId}/{isMonir}")]
        public IActionResult GetVersionNumber(int projectId, bool isMonir)
        {
            return Ok(_studyVersionRepository.GetVersionNumber(projectId, isMonir));
        }

       
        [HttpGet]
        [Route("GetVersionDropDown/{projectId}")]
        public IActionResult GetVersionDropDown(int projectId)
        {
            return Ok(_studyVersionRepository.GetVersionDropDown(projectId));
        }

        [HttpGet]
        [Route("GetStudyVersionForLive/{projectId}")]
        public IActionResult GetStudyVersionForLive(int projectId)
        {
            return Ok(_studyVersionRepository.GetStudyVersionForLive(projectId));
        }

    }
}
