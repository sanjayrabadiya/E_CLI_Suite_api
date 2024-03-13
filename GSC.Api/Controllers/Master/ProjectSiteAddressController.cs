using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProjectSiteAddressController : BaseController
    {
        private readonly IProjectSiteAddressRepository _projectSiteAddressRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ProjectSiteAddressController(IProjectSiteAddressRepository projectSiteAddressRepository, IProjectRepository projectRepository, IManageSiteAddressRepository manageSiteAddressRepository, IManageSiteRepository manageSiteRepository, IMapper mapper, IUnitOfWork uow, IGSCContext context)
        {
            _projectSiteAddressRepository = projectSiteAddressRepository;
            _mapper = mapper;
            _uow = uow;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var ProjectSiteAddress = _projectSiteAddressRepository.GetProjectSiteAddressList(isDeleted);
            return Ok(ProjectSiteAddress);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ProjectSiteAddress = _projectSiteAddressRepository.Find(id);
            var ProjectSiteAddressDto = _mapper.Map<ProjectSiteAddressDto>(ProjectSiteAddress);
            return Ok(ProjectSiteAddressDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProjectSiteAddressDto ProjectSiteAddressDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ProjectSiteAddressDto.Id = 0;
            var ProjectSiteAddress = _mapper.Map<ProjectSiteAddress>(ProjectSiteAddressDto);
            var validate = _projectSiteAddressRepository.Duplicate(ProjectSiteAddress);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _projectSiteAddressRepository.Add(ProjectSiteAddress);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Project Site Address failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(ProjectSiteAddress.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectSiteAddressDto ProjectSiteAddressDto)
        {
            if (ProjectSiteAddressDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ProjectSiteAddress = _mapper.Map<ProjectSiteAddress>(ProjectSiteAddressDto);
            var validate = _projectSiteAddressRepository.Duplicate(ProjectSiteAddress);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _projectSiteAddressRepository.AddOrUpdate(ProjectSiteAddress);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Project Site Address failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(ProjectSiteAddress.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectSiteAddressRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectSiteAddressRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectSiteAddressRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _projectSiteAddressRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectSiteAddressRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetProjectSiteAddressByProject/{isDeleted}/{projectId}/{manageSiteId}")]
        public IActionResult GetProjectSiteAddressByProject(bool isDeleted, int projectId, int manageSiteId)
        {
            var ProjectSiteAddress = _projectSiteAddressRepository.GetProjectSiteAddressByProject(isDeleted, projectId, manageSiteId);
            return Ok(ProjectSiteAddress);
        }
    }
}
