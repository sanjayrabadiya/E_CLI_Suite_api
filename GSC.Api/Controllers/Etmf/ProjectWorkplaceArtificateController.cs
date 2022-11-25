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
    public class ProjectWorkplaceArtificateController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
      
        public ProjectWorkplaceArtificateController(IProjectRepository projectRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository
            )
        {
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
        }


        [HttpGet]
        [Route("GetProjectWorkPlaceArtificateDropDown/{sectionId}")]
        public IActionResult GetProjectWorkPlaceSectionDropDown(int sectionId)
        {
            return Ok(_projectWorkplaceArtificateRepository.GetProjectWorkPlaceArtificateDropDown(sectionId));
        }

        [Route("GetWorkPlaceFolder/{EtmfArtificateMasterLbraryId}/{ProjectWorkplaceArtificateId}")]
        [HttpGet]
        public IActionResult GetWorkPlaceFolder(int EtmfArtificateMasterLbraryId, int ProjectWorkplaceArtificateId)
        {
            var result = _projectWorkplaceArtificateRepository.GetWorkPlaceFolder(EtmfArtificateMasterLbraryId, ProjectWorkplaceArtificateId);
            return Ok(result);
        }

        [Route("GetDocChart")]
        [HttpGet]
        public IActionResult GetDocChart([FromQuery] WorkplaceChartFilterDto filters)
        {
            var result = _projectWorkplaceArtificateRepository.GetDocChart(filters);
            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateNotRequired/{id}")]
        public IActionResult UpdateNotRequired(int id)
        {
            var projectWorkplaceArtificateDto = _projectWorkplaceArtificateRepository.Find(id);
            if (projectWorkplaceArtificateDto.IsNotRequired)
            {
                projectWorkplaceArtificateDto.IsNotRequired = !projectWorkplaceArtificateDto.IsNotRequired;
            }
            else
            {
                projectWorkplaceArtificateDto.IsNotRequired = !projectWorkplaceArtificateDto.IsNotRequired;
            }
            var projectWorkplaceArtificate = _mapper.Map<EtmfProjectWorkPlace>(projectWorkplaceArtificateDto);
            _projectWorkplaceArtificateRepository.Update(projectWorkplaceArtificate);

            if (_uow.Save() <= 0) throw new Exception("Updating Artificate failed on save.");
            return Ok(projectWorkplaceArtificate.Id);
        }
    }
}