using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class RegulatoryTypeController : BaseController
    {
        private readonly IRegulatoryTypeRepository _regulatoryTypeRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public RegulatoryTypeController(IRegulatoryTypeRepository regulatoryTypeRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _regulatoryTypeRepository = regulatoryTypeRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var regulatoryType = _regulatoryTypeRepository.GetRegulatoryTypeList(isDeleted);
            return Ok(regulatoryType);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var regulatoryType = _regulatoryTypeRepository.Find(id);
            var regulatoryTypeDto = _mapper.Map<RegulatoryTypeDto>(regulatoryType);
            return Ok(regulatoryTypeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] RegulatoryTypeDto regulatoryTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            regulatoryTypeDto.Id = 0;
            var regulatoryType = _mapper.Map<RegulatoryType>(regulatoryTypeDto);
            var validate = _regulatoryTypeRepository.Duplicate(regulatoryType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _regulatoryTypeRepository.Add(regulatoryType);
            if (_uow.Save() <= 0) throw new Exception("Creating Regulatory failed on save.");
            return Ok(regulatoryType.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] RegulatoryTypeDto regulatoryTypeDto)
        {
            if (regulatoryTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var regulatoryType = _mapper.Map<RegulatoryType>(regulatoryTypeDto);
            var validate = _regulatoryTypeRepository.Duplicate(regulatoryType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by darshil for effective Date on 09-09-2020 */
            _regulatoryTypeRepository.AddOrUpdate(regulatoryType);

            if (_uow.Save() <= 0) throw new Exception("Updating Regulatory failed on save.");
            return Ok(regulatoryType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _regulatoryTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _regulatoryTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _regulatoryTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _regulatoryTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _regulatoryTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetRegulatoryTypeDropDown")]
        public IActionResult GetRegulatoryTypeDropDown()
        {
            return Ok(_regulatoryTypeRepository.GetRegulatoryTypeDropDown());
        }
    }
}
