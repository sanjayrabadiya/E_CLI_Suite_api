using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralDepotController : BaseController{
        
        private readonly IMapper _mapper;
        private readonly ICentralDepotRepository _centralDepotRepository;
        private readonly IUnitOfWork _uow;

        public CentralDepotController(ICentralDepotRepository centralDepotRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _centralDepotRepository = centralDepotRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var productTypes = _centralDepotRepository.GetCentralDepotList(isDeleted);
            return Ok(productTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _centralDepotRepository.Find(id);
            var centralDepoDto = _mapper.Map<CentralDepotDto>(centralDepo);
            return Ok(centralDepoDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CentralDepotDto centralDepotDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            centralDepotDto.Id = 0;
            var centralDepot = _mapper.Map<CentralDepot>(centralDepotDto);
            var validate = _centralDepotRepository.Duplicate(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var receipt = _centralDepotRepository.StudyUseInReceipt(centralDepot);
            if (!string.IsNullOrEmpty(receipt))
            {
                ModelState.AddModelError("Message", receipt);
                return BadRequest(ModelState);
            }

            _centralDepotRepository.Add(centralDepot);
            if (_uow.Save() <= 0) throw new Exception("Creating central depot failed on save.");
            return Ok(centralDepot.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] CentralDepotDto centralDepotDto)
        {
            if (centralDepotDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var centralDepot = _mapper.Map<CentralDepot>(centralDepotDto);
            var validate = _centralDepotRepository.Duplicate(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var exists = _centralDepotRepository.ExistsInReceipt(centralDepot.Id);
            if (!string.IsNullOrEmpty(exists))
            {
                ModelState.AddModelError("Message", exists);
                return BadRequest(ModelState);
            }

            var receipt = _centralDepotRepository.StudyUseInReceipt(centralDepot);
            if (!string.IsNullOrEmpty(receipt))
            {
                ModelState.AddModelError("Message", receipt);
                return BadRequest(ModelState);
            }

            _centralDepotRepository.AddOrUpdate(centralDepot);

            if (_uow.Save() <= 0) throw new Exception("Updating central depot failed on save.");
            return Ok(centralDepot.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _centralDepotRepository.Find(id);

            if (record == null)
                return NotFound();

            var exists = _centralDepotRepository.ExistsInReceipt(id);
            if (!string.IsNullOrEmpty(exists))
            {
                ModelState.AddModelError("Message", exists);
                return BadRequest(ModelState);
            }

            _centralDepotRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _centralDepotRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _centralDepotRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _centralDepotRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetStorageAreaByDepoDropDown")]
        public IActionResult GetStorageAreaByDepoDropDown()
        {
            return Ok(_centralDepotRepository.GetStorageAreaByDepoDropDown());
        }

        [HttpGet]
        [Route("GetStorageAreaByProjectDropDown/{ProjectId}")]
        public IActionResult GetStorageAreaByProjectDropDown(int ProjectId)
        {
            return Ok(_centralDepotRepository.GetStorageAreaByProjectDropDown(ProjectId));
        }

        [HttpGet]
        [Route("GetStorageAreaByIdDropDown/{Id}")]
        public IActionResult GetStorageAreaByIdDropDown(int Id)
        {
            return Ok(_centralDepotRepository.GetStorageAreaByIdDropDown(Id));
        }   
        [HttpGet]
        [Route("getStorageAreaByProjectandCountryDropDown/{ProjectId?}/{CountryId?}")]
        public IActionResult getStorageAreaByProjectandCountryDropDown(int? ProjectId, int? CountryId)
        {
            return Ok(_centralDepotRepository.GetStorageAreaByProjectDropDown(ProjectId, CountryId));
        }
    }
}
