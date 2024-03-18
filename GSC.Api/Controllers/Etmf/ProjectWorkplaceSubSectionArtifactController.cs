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
    public class ProjectWorkplaceSubSectionArtifactController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IEtmfMasterLbraryRepository _etmfMasterLibraryRepository;
        private readonly IProjectWorkplaceSubSectionArtifactRepository _projectWorkplaceSubSectionArtifactRepository;
        public ProjectWorkplaceSubSectionArtifactController(
            IUnitOfWork uow,
            IMapper mapper,
            IEtmfMasterLbraryRepository etmfMasterLibraryRepository,
            IProjectWorkplaceSubSectionArtifactRepository projectWorkplaceSubSectionArtifactRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _etmfMasterLibraryRepository = etmfMasterLibraryRepository;
            _projectWorkplaceSubSectionArtifactRepository = projectWorkplaceSubSectionArtifactRepository;
        }


        [Route("Get")]
        [HttpGet]
        public ActionResult Get()
        {
            var result = _etmfMasterLibraryRepository.FindByInclude(x => x.DeletedBy == null, x => x.EtmfSectionMasterLibrary);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EtmfProjectWorkPlaceDto projectWorkplaceSubSectionArtifactDto)
        {
            var data = _projectWorkplaceSubSectionArtifactRepository.getSectionDetail(projectWorkplaceSubSectionArtifactDto);

            var projectWorkplaceSubSectionArtifact = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceSubSectionArtifactDto);
            projectWorkplaceSubSectionArtifact.EtmfProjectWorkPlaceId = projectWorkplaceSubSectionArtifactDto.ProjectWorkplaceSubSectionId;
            projectWorkplaceSubSectionArtifact.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact;
            projectWorkplaceSubSectionArtifact.ProjectId = data.ProjectId;
            var validate = _projectWorkplaceSubSectionArtifactRepository.Duplicate(projectWorkplaceSubSectionArtifact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionArtifactRepository.Add(projectWorkplaceSubSectionArtifact);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Sub Section failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(projectWorkplaceSubSectionArtifact.Id);

        }


        [HttpPut]
        public IActionResult Put([FromBody] EtmfProjectWorkPlaceDto projectWorkplaceSubSectionArtifactDto)
        {
            var data = _projectWorkplaceSubSectionArtifactRepository.UpdateArtifactDetail(projectWorkplaceSubSectionArtifactDto);

            var projectWorkplaceSubSectionArtifact = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceSubSectionArtifactDto);
            projectWorkplaceSubSectionArtifact.EtmfProjectWorkPlaceId = projectWorkplaceSubSectionArtifactDto.ProjectWorkplaceSubSectionId;
            projectWorkplaceSubSectionArtifact.TableTag = (int)EtmfTableNameTag.ProjectWorkPlaceSubSectionArtifact;
            projectWorkplaceSubSectionArtifact.ProjectId = data.ProjectId;
            var validate = _projectWorkplaceSubSectionArtifactRepository.Duplicate(projectWorkplaceSubSectionArtifact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectWorkplaceSubSectionArtifactRepository.Update(projectWorkplaceSubSectionArtifact);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Sub Section failed on save.");
                return BadRequest(ModelState);
            }
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
            return Ok();
        }

        [HttpGet]
        [Route("GetDrodDown/{subsectionId}")]
        public IActionResult GetDrodDown(int subsectionId)
        {
            return Ok(_projectWorkplaceSubSectionArtifactRepository.GetDrodDown(subsectionId));
        }

        [HttpPut]
        [Route("UpdateNotRequired/{id}")]
        public IActionResult UpdateNotRequired(int id)
        {
            var projectWorkplaceSubSecArtificatDto = _projectWorkplaceSubSectionArtifactRepository.Find(id);
            projectWorkplaceSubSecArtificatDto.IsNotRequired = !projectWorkplaceSubSecArtificatDto.IsNotRequired;
            var projectWorkplaceSubSecArtificat = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceSubSecArtificatDto);
            _projectWorkplaceSubSectionArtifactRepository.Update(projectWorkplaceSubSecArtificat);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Artificate failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(projectWorkplaceSubSecArtificat.Id);
        }
    }
}