using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceSectionController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IProjectWorkplaceSectionRepository _projectWorkplaceSectionRepository;

        public ProjectWorkplaceSectionController(
            IMapper mapper,
            IProjectWorkplaceSectionRepository projectWorkplaceSectionRepository
            )
        {
            _mapper = mapper;
            _projectWorkplaceSectionRepository = projectWorkplaceSectionRepository;
        }


        [HttpGet]
        [Route("GetProjectWorkPlaceSectionDropDown/{zoneid}")]
        public IActionResult GetProjectWorkPlaceSectionDropDown(int zoneid)
        {
            return Ok(_projectWorkplaceSectionRepository.GetProjectWorkPlaceSectionDropDown(zoneid));
        }

        [HttpGet("Get/{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectWorkplaceSection = _projectWorkplaceSectionRepository.FindByInclude(x => x.Id == id, x => x.EtmfMasterLibrary).First();
            var projectWorkplaceSectionDto = _mapper.Map<EtmfProjectWorkPlaceDto>(projectWorkplaceSection);
            projectWorkplaceSectionDto.SectionName = projectWorkplaceSection.EtmfMasterLibrary.SectionName;
            return Ok(projectWorkplaceSectionDto);
        }
    }
}