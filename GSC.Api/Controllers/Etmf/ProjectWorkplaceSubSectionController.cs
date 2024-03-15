using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Shared.Generic;
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
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        private readonly IProjectWorkplaceSubSectionRepository _projectWorkplaceSubSectionRepository;
        public ProjectWorkplaceSubSectionController(
            IUnitOfWork uow,
            IMapper mapper,
            IEtmfMasterLbraryRepository etmfMasterLibraryRepository,
            IProjectWorkplaceSubSectionRepository projectWorkplaceSubSectionRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
            _projectWorkplaceSubSectionRepository = projectWorkplaceSubSectionRepository;
        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [HttpGet("Get/{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectWorkplaceSection = _projectWorkplaceSubSectionRepository.FindByInclude(x => x.Id == id).First();
            var projectWorkplaceSectionDto = _mapper.Map<EtmfProjectWorkPlaceDto>(projectWorkplaceSection);
            projectWorkplaceSectionDto.SubSectionName = projectWorkplaceSection.SubSectionName;
            return Ok(projectWorkplaceSectionDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = _projectWorkplaceSubSectionRepository.getSectionDetail(projectWorkplaceSubSectionDto);
            var projectWorkplaceSubSection = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceSubSectionDto);
            projectWorkplaceSubSection.EtmfProjectWorkPlaceId = data.ProjectWorkplaceSectionId;
            projectWorkplaceSubSection.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSubSection;
            projectWorkplaceSubSection.ProjectId = data.ProjectId;
            var validate = _projectWorkplaceSubSectionRepository.Duplicate(projectWorkplaceSubSection);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionRepository.Add(projectWorkplaceSubSection);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Sub Section failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(projectWorkplaceSubSection.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EtmfProjectWorkPlaceDto projectWorkplaceSubSectionDto)
        {
            var data = _projectWorkplaceSubSectionRepository.updateSectionDetailFolder(projectWorkplaceSubSectionDto);

            var projectWorkplaceSubSection = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceSubSectionDto);
            projectWorkplaceSubSection.EtmfProjectWorkPlaceId = data.ProjectWorkplaceSectionId;
            projectWorkplaceSubSection.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSubSection;
            projectWorkplaceSubSection.ProjectId = data.ProjectId;
            var validate = _projectWorkplaceSubSectionRepository.Duplicate(projectWorkplaceSubSection);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionRepository.Update(projectWorkplaceSubSection);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Sub Section failed on save.");
                return BadRequest(ModelState);
            }
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
            var subArtifact = _projectWorkplaceSubSectionRepository.FindByInclude(x => x.Id == id, x => x.ProjectWorkPlace)
                .FirstOrDefault();

            if (subArtifact == null)
                return NotFound();
            _projectWorkplaceSubSectionRepository.Delete(subArtifact);
            _uow.Save();
            return Ok();
        }
    }
}