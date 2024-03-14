using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Respository.Project.Design;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectDesignVisitStatusController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IUnitOfWork _uow;

        public ProjectDesignVisitStatusController(IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _uow = uow;
            _mapper = mapper;
        }

        //added by vipul for get visit status by template id on 23092020
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            return Ok(_projectDesignVisitStatusRepository.GetProjectDesignVisitStatusById(id));
        }

        //added by vipul for add visit status on 23092020
        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignVisitStatusDto projectDesignVisitStatusDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDesignVisitStatusDto.Id = 0;
            var projectDesignVisitStatus = _mapper.Map<ProjectDesignVisitStatus>(projectDesignVisitStatusDto);

            var validate = _projectDesignVisitStatusRepository.Duplicate(projectDesignVisitStatusDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDesignVisitStatusRepository.Add(projectDesignVisitStatus);
            _uow.Save();
            return Ok(projectDesignVisitStatus.Id);
        }

        //added by vipul for DELETE previos visit status if exists on same visit on 23092020
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignVisitStatusRepository.Find(id);
            if (record == null)
                return NotFound();
            _projectDesignVisitStatusRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        //added by vipul for get visit status list by visit on 09112020
        [HttpGet]
        [Route("GetVisits/{visitId}")]
        public IActionResult GetVisits(int visitId)
        {
            return Ok(_projectDesignVisitStatusRepository.GetVisits(visitId));
        }
    }
}
