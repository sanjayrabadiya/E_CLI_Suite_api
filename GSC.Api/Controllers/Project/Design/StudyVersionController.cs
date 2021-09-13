﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
        private readonly IStudyVersionStatusRepository _studyVersionVisitStatusRepository;

        public StudyVersionController(IStudyVersionRepository studyVersionRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context, IStudyVersionStatusRepository studyVersionVisitStatusRepository)
        {
            _studyVersionRepository = studyVersionRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _studyVersionVisitStatusRepository = studyVersionVisitStatusRepository;
        }


        [HttpGet]
        [Route("GetDetails/{projectDesignId}")]
        public IActionResult GetDetails(int projectDesignId)
        {
            var manageSite = _studyVersionRepository.GetStudyVersion(projectDesignId);
            return Ok(manageSite);
        }

        [HttpGet]
        [Route("GetOnTrialDetail/{projectDesignId}")]
        public IActionResult GetOnTrialDetail(int projectDesignId)
        {
            var studyVersion = _studyVersionRepository
                    .FindByInclude(x => x.ProjectDesignId == projectDesignId && x.VersionStatus == Helper.VersionStatus.OnTrial, x => x.StudyVerionStatus).FirstOrDefault();
            if (studyVersion == null)
                return BadRequest();

            if (studyVersion != null && studyVersion.StudyVerionStatus != null)
                studyVersion.StudyVerionStatus = studyVersion.StudyVerionStatus.Where(x => x.DeletedDate == null).ToList();

            var studyVersionDto = _mapper.Map<StudyVersionDto>(studyVersion);
            studyVersionDto.IsTestSiteVerified = studyVersionDto.IsTestSiteVerified ?? false;
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

            foreach (var item in studyVersion.StudyVerionStatus)
            {
                item.StudyVerion = studyVersion;
                _studyVersionVisitStatusRepository.Add(item);
            }

            _uow.Save();

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

        [HttpPut("GoLive")]
        public IActionResult GoLive([FromBody] StudyGoLiveDto studyGoLiveDto)
        {
            var studyVersion = _studyVersionRepository.All.AsNoTracking().Where(x => x.DeletedDate == null && x.ProjectDesignId == studyGoLiveDto.ProjectDesignId && x.VersionStatus == Helper.VersionStatus.OnTrial).FirstOrDefault();

            if (_studyVersionRepository.AnyLive(studyGoLiveDto.ProjectDesignId) && studyGoLiveDto.IsOnTrial == false && (studyVersion.IsTestSiteVerified == null || studyVersion.IsTestSiteVerified == false))
            {
                ModelState.AddModelError("Message", "First verify on Test site");
                return BadRequest(ModelState);
            }

             _studyVersionRepository.UpdateGoLive(studyGoLiveDto, studyVersion);
            _uow.Save();
            return Ok();
        }

    }
}
