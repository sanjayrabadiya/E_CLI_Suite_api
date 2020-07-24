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
    public class ProjectWorkplaceSubSectionArtifactController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IProjectWorkplaceSubSectionArtifactRepository _projectWorkplaceSubSectionArtifactRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionArtifactController(
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IEtmfZoneMasterLibraryRepository etmfZoneMasterLibraryRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectWorkplaceSubSectionArtifactRepository projectWorkplaceSubSectionArtifactRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfZoneMasterLibraryRepository = etmfZoneMasterLibraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkplaceSubSectionArtifactRepository = projectWorkplaceSubSectionArtifactRepository;
        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfZoneMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectWorkplaceSubSectionArtifactDto projectWorkplaceSubSectionArtifactDto)
        {
            var data = _projectWorkplaceSubSectionArtifactRepository.getSectionDetail(projectWorkplaceSubSectionArtifactDto);

            var projectWorkplaceSubSectionArtifact = _mapper.Map<ProjectWorkplaceSubSectionArtifact>(projectWorkplaceSubSectionArtifactDto);
            var validate = _projectWorkplaceSubSectionArtifactRepository.Duplicate(projectWorkplaceSubSectionArtifact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionArtifactRepository.Add(projectWorkplaceSubSectionArtifact);
            if (_uow.Save() <= 0) throw new Exception("Creating Sub Section failed on save.");
            return Ok(projectWorkplaceSubSectionArtifact.Id);
 
        }


        [HttpPut]
        public IActionResult Put([FromBody] ProjectWorkplaceSubSectionArtifactDto projectWorkplaceSubSectionArtifactDto)
        {
            var data = _projectWorkplaceSubSectionArtifactRepository.UpdateArtifactDetail(projectWorkplaceSubSectionArtifactDto);

            var projectWorkplaceSubSectionArtifact = _mapper.Map<ProjectWorkplaceSubSectionArtifact>(projectWorkplaceSubSectionArtifactDto);
           
            var validate = _projectWorkplaceSubSectionArtifactRepository.Duplicate(projectWorkplaceSubSectionArtifact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionArtifactRepository.Update(projectWorkplaceSubSectionArtifact);
            if (_uow.Save() <= 0) throw new Exception("Creating Sub Section failed on save.");
            return Ok(projectWorkplaceSubSectionArtifact.Id);

        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var subArtifact = _projectWorkplaceSubSectionArtifactRepository.FindByInclude(x => x.Id == id, x => x.ProjectWorkplaceSubSecArtificatedocument)
                .FirstOrDefault();

            if (subArtifact == null)
                return NotFound();
            _projectWorkplaceSubSectionArtifactRepository.Delete(subArtifact);
            _uow.Save();
            var aa = _projectWorkplaceSubSectionArtifactRepository.DeletArtifactDetailFolder(id);
            return Ok();
        }

        [HttpGet]
        [Route("GetDrodDown/{subsectionId}")]
        public IActionResult GetDrodDown(int subsectionId)
        {
            return Ok(_projectWorkplaceSubSectionArtifactRepository.GetDrodDown(subsectionId));
        }
    }
}