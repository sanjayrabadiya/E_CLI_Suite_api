using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]   
    public class SyncConfigurationController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ISyncConfigurationRepository _syncConfigurationRepository;
        private readonly IProjectRepository _projectRepository;
        public SyncConfigurationController(IUnitOfWork uow, IMapper mapper, ISyncConfigurationRepository syncConfigurationRepository, IProjectRepository projectRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _syncConfigurationRepository = syncConfigurationRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("{isDeleted:bool?}/{projectId}")]
        public IActionResult Get(bool isDeleted,int projectId)
        {
            var syncConfigurationlist = _syncConfigurationRepository.GetsyncConfigurationList(isDeleted,projectId);
            return Ok(syncConfigurationlist);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var syncConfiguration = _syncConfigurationRepository.Find(id);
            var syncConfigurationDetails = _mapper.Map<SyncConfigurationDto>(syncConfiguration);
            return Ok(syncConfigurationDetails);
        }


        [HttpGet]
        [Route("GetProjectWorkPlaceDetails/{projectId}/{workPlaceFolderId}")]
        public IActionResult GetProjectWorkPlaceDetails(int projectId, int workPlaceFolderId)
        {
            var syncConfigurationlist = _syncConfigurationRepository.GetProjectWorkPlaceDetails(projectId, Convert.ToInt16(workPlaceFolderId));
            return Ok(syncConfigurationlist);
        }


        [HttpGet]
        [Route("GetProjectdDropDownETMF")]
        public IActionResult GetProjectdDropDownETMF()
        {
            var projectDetails = _syncConfigurationRepository.GetProjectDropDownEtmf();
            return Ok(projectDetails);
        }


        [HttpPost]
        public IActionResult Post([FromBody] SyncConfigurationDto details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            details.Id = 0;
            var syncconfigDetails = _mapper.Map<SyncConfiguration>(details);

            _syncConfigurationRepository.Add(syncconfigDetails);
            if (_uow.Save() <= 0) throw new Exception("Creating Sync Configuration failed on save.");
            return Ok(syncconfigDetails.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SyncConfigurationDto details)
        {
            if (details.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var syncconfigDetails = _mapper.Map<SyncConfiguration>(details);

            _syncConfigurationRepository.Update(syncconfigDetails);

            if (_uow.Save() <= 0) throw new Exception("Updating Sync Configuration failed on save.");
            return Ok(syncconfigDetails.Id);
        }




    }
}
