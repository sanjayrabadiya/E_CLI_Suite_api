using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using GSC.Data.Entities.CTMS;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class ResourceTypeController : BaseController
    {
        private readonly IResourceTypeRepository _resourcetypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ResourceTypeController(IResourceTypeRepository resourcetypeRepository,

            IUnitOfWork uow, IMapper mapper)
        {
            _resourcetypeRepository = resourcetypeRepository;
            _uow = uow;
            _mapper = mapper;
        }
        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var resourcetype = _resourcetypeRepository.GetResourceTypeList(isDeleted);
            resourcetype.ForEach(x => x.User = x.ResourceType == "Material" ? null : x.User);
            return Ok(resourcetype);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var resourcetype = _resourcetypeRepository.Find(id);
            var resourcetypeDto = _mapper.Map<ResourceTypeDto>(resourcetype);
            return Ok(resourcetypeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ResourceTypeDto resourcetypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            resourcetypeDto.Id = 0;
            var ResourceType = _mapper.Map<ResourceType>(resourcetypeDto);
            var validate = _resourcetypeRepository.Duplicate(ResourceType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _resourcetypeRepository.Add(ResourceType);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating ResourceType failed on save."));
            return Ok(ResourceType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ResourceTypeDto resourcetypeDto)
        {
            if (resourcetypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var resourcetype = _mapper.Map<ResourceType>(resourcetypeDto);
            var validate = _resourcetypeRepository.Duplicate(resourcetype);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _resourcetypeRepository.AddOrUpdate(resourcetype);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating ResourceType failed on save."));
            return Ok(resourcetype.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _resourcetypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _resourcetypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _resourcetypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _resourcetypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _resourcetypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetUnitTypeDropDown")]
        public IActionResult GetUnitTypeDropDown()
        {
            return Ok(_resourcetypeRepository.GetUnitTypeDropDown());
        }

        [HttpGet]
        [Route("GetDesignationDropDown")]
        public IActionResult GetDesignationDropDown()
        {
            return Ok(_resourcetypeRepository.GetDesignationDropDown());
        }
        [HttpGet]
        [Route("GetDesignationDropDown/{resourceTypeID}/{resourceSubTypeID}/{projectId}")]
        public IActionResult getSelectDateDrop(int resourceTypeID, int resourceSubTypeID, int projectId)
        {
            return Ok(_resourcetypeRepository.GetDesignationDropDown(resourceTypeID, resourceSubTypeID, projectId));
        }

        [HttpGet]
        [Route("GetNameOfMaterialDropDown/{resourceTypeID}/{resourceSubTypeID}")]
        public IActionResult getNameOfMaterialDropDown(int resourceTypeID, int resourceSubTypeID)
        {
            return Ok(_resourcetypeRepository.GetNameOfMaterialDropDown(resourceTypeID, resourceSubTypeID));

        }
        [HttpGet]
        [Route("GetRollUserDropDown/{designationID}/{projectId}")]
        public IActionResult getSelectDateDrop(int designationID, int projectId)
        {
            return Ok(_resourcetypeRepository.GetRollUserDropDown(designationID, projectId));
        }
        [HttpGet]
        [Route("GetCurrencyDropDown")]
        public IActionResult GetCurrencyDropDown()
        {
            return Ok(_resourcetypeRepository.GetCurrencyDropDown());
        }
    }
}
