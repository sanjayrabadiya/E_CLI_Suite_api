﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Respository.LabManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.LabManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabManagementVariableMappingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ILabManagementVariableMappingRepository _labManagementVariableMappingRepository;
        private readonly ILabManagementUploadDataRepository _labManagementUploadDataRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILabManagementConfigurationRepository _labManagementConfigurationRepository;

        public LabManagementVariableMappingController(
            ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
            ILabManagementUploadDataRepository labManagementUploadDataRepository,
            ILabManagementConfigurationRepository labManagementConfigurationRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            _labManagementUploadDataRepository = labManagementUploadDataRepository;
            _labManagementConfigurationRepository = labManagementConfigurationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{LabManagementConfigurationId}")]
        public IActionResult Get(int LabManagementConfigurationId)
        {
            return Ok(_labManagementVariableMappingRepository.GetLabManagementVariableMappingList(LabManagementConfigurationId));
        }

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{
        //    if (id <= 0) return BadRequest();
        //    var mapping = _labManagementVariableMappingRepository.Find(id);
        //    var mappingDto = _mapper.Map<LabManagementVariableMappingDto>(mapping);
        //    return Ok(mappingDto);
        //}


        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] LabManagementVariableMappingDto mappingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            mappingDto.Id = 0;

            foreach (var item in mappingDto.LabManagementVariableMappingDetail)
            {
                LabManagementVariableMapping mapping = new LabManagementVariableMapping();
                mapping.LabManagementConfigurationId = mappingDto.LabManagementConfigurationId;
                mapping.ProjectDesignVariableId = item.ProjectDesignVariableId;
                if (item.ProjectDesignVariableId == 0)
                {
                    // Get Project design variable id by lab management configuration Id
                    mapping.ProjectDesignVariableId = _labManagementConfigurationRepository.getProjectDesignVariableId(mappingDto.LabManagementConfigurationId, item.TargetVariable);
                    if (mapping.ProjectDesignVariableId == 0)
                    {
                        ModelState.AddModelError("Message", item.TargetVariable + " Variable not found in template, please add first");
                        return BadRequest(ModelState);
                    }
                }
                mapping.TargetVariable = item.TargetVariable;

                _labManagementVariableMappingRepository.Add(mapping);
                if (_uow.Save() <= 0) throw new Exception("Creating Mapping failed on save.");
            }

            return Ok();
        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] LabManagementVariableMappingDto mappingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Exists = _labManagementUploadDataRepository.CheckDataIsUploadForRemapping(mappingDto.LabManagementConfigurationId);
            if (!string.IsNullOrEmpty(Exists))
            {
                ModelState.AddModelError("Message", Exists);
                return BadRequest(ModelState);
            }

            _labManagementVariableMappingRepository.DeleteMapping(mappingDto);
            return Ok();
        }
    }
}
