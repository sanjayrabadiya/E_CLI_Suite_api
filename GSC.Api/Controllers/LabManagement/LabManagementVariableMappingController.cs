using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public LabManagementVariableMappingController(
            ILabManagementVariableMappingRepository labManagementVariableMappingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _labManagementVariableMappingRepository = labManagementVariableMappingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var mapping = _labManagementVariableMappingRepository.Find(id);
            var mappingDto = _mapper.Map<LabManagementVariableMappingDto>(mapping);
            return Ok(mappingDto);
        }


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
                mapping.TargetVariable = item.TargetVariable;

                _labManagementVariableMappingRepository.Add(mapping);
                if (_uow.Save() <= 0) throw new Exception("Creating Mapping failed on save.");
            }
            
            return Ok();
        }

    }
}
