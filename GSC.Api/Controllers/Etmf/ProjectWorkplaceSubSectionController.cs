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


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceSubSectionController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IEtmfZoneMasterLibraryRepository _etmfZoneMasterLibraryRepository;
        private readonly IProjectWorkplaceSubSectionRepository _projectWorkplaceSubSectionRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public ProjectWorkplaceSubSectionController(
            IUnitOfWork<GscContext> uow,
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
    }
}