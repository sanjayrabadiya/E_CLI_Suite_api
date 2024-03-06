using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Mvc;
using System;


namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementConfigurationController : BaseController
    {
        
        private readonly ISupplyManagementConfigurationRepository _supplyManagementConfigurationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public SupplyManagementConfigurationController(
            ISupplyManagementConfigurationRepository supplyManagementConfigurationRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _supplyManagementConfigurationRepository = supplyManagementConfigurationRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_supplyManagementConfigurationRepository.GetSupplyManagementTemplateList(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var configuration = _supplyManagementConfigurationRepository.Find(id);
            var configurationDto = _mapper.Map<SupplyManagementConfigurationDto>(configuration);
            return Ok(configurationDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementConfigurationDto configurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            configurationDto.Id = 0;
            var configuration = _mapper.Map<SupplyManagementConfiguration>(configurationDto);
            var validate = _supplyManagementConfigurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _supplyManagementConfigurationRepository.Add(configuration);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Supply Management Configuration failed on save."));
            return Ok(configuration.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementConfigurationDto configurationDto)
        {
            if (configurationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var configuration = _mapper.Map<SupplyManagementConfiguration>(configurationDto);
            var validate = _supplyManagementConfigurationRepository.Duplicate(configuration);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _supplyManagementConfigurationRepository.AddOrUpdate(configuration);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating Supply Management Configuration failed on save."));
            return Ok(configuration.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementConfigurationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _supplyManagementConfigurationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _supplyManagementConfigurationRepository.Active(record);
            _uow.Save();

            return Ok();
        }

    }
}
