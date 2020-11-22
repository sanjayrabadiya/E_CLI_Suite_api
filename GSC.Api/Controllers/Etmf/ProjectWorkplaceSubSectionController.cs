using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceSubSectionController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IProjectWorkplaceSubSectionRepository _projectWorkplaceSubSectionRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionController(
            IUnitOfWork uow,
            IMapper mapper,
            IEtmfZoneMasterLibraryRepository etmfZoneMasterLibraryRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectWorkplaceSubSectionRepository projectWorkplaceSubSectionRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfZoneMasterLibraryRepository = etmfZoneMasterLibraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkplaceSubSectionRepository = projectWorkplaceSubSectionRepository;
        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfZoneMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [HttpGet("Get/{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectWorkplaceSection = _projectWorkplaceSubSectionRepository.FindByInclude(x => x.Id == id).FirstOrDefault();
            var projectWorkplaceSectionDto = _mapper.Map<ProjectWorkplaceSubSectionDto>(projectWorkplaceSection);
            projectWorkplaceSectionDto.SubSectionName = projectWorkplaceSection.SubSectionName;
            return Ok(projectWorkplaceSectionDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectWorkplaceSubSectionDto projectWorkplaceSubSectionDto)
        {
            var data = _projectWorkplaceSubSectionRepository.getSectionDetail(projectWorkplaceSubSectionDto);

            var projectWorkplaceSubSection = _mapper.Map<ProjectWorkplaceSubSection>(projectWorkplaceSubSectionDto);
            var validate = _projectWorkplaceSubSectionRepository.Duplicate(projectWorkplaceSubSection);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionRepository.Add(projectWorkplaceSubSection);
            if (_uow.Save() <= 0) throw new Exception("Creating Sub Section failed on save.");
            return Ok(projectWorkplaceSubSection.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectWorkplaceSubSectionDto projectWorkplaceSubSectionDto)
        {
            var data = _projectWorkplaceSubSectionRepository.updateSectionDetailFolder(projectWorkplaceSubSectionDto);

            var projectWorkplaceSubSection = _mapper.Map<ProjectWorkplaceSubSection>(projectWorkplaceSubSectionDto);
            var validate = _projectWorkplaceSubSectionRepository.Duplicate(projectWorkplaceSubSection);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionRepository.Update(projectWorkplaceSubSection);
            if (_uow.Save() <= 0) throw new Exception("Creating Sub Section failed on save.");
            return Ok(projectWorkplaceSubSection.Id);
        }

        [HttpGet]
        [Route("GetDrodDown/{sectionId}")]
        public IActionResult GetDrodDown(int sectionId)
        {
            return Ok(_projectWorkplaceSubSectionRepository.GetDrodDown(sectionId));
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var subArtifact = _projectWorkplaceSubSectionRepository.FindByInclude(x => x.Id == id, x => x.ProjectWorkplaceSubSectionArtifact)
                .FirstOrDefault();
 
            if (subArtifact == null)
                return NotFound();
            _projectWorkplaceSubSectionRepository.Delete(subArtifact);
            _uow.Save();
            var aa = _projectWorkplaceSubSectionRepository.DeletSectionDetailFolder(id);
            return Ok();
        }
    }
}