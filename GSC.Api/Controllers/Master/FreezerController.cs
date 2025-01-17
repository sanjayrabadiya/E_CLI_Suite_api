using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class FreezerController : BaseController
    {
        private readonly IFreezerRepository _freezerRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public FreezerController(IFreezerRepository freezerRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _freezerRepository = freezerRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var freezer = _freezerRepository.GetFreezerList(isDeleted);
            freezer.ForEach(t => t.FreezerTypeName = t.FreezerType.GetDescription());
            return Ok(freezer);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var freezer = _freezerRepository.Find(id);
            var freezerDto = _mapper.Map<FreezerDto>(freezer);
            return Ok(freezerDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] FreezerDto freezerDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            freezerDto.Id = 0;
            var freezer = _mapper.Map<Freezer>(freezerDto);
            var validate = _freezerRepository.Duplicate(freezer);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _freezerRepository.Add(freezer);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Freezer failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(freezer.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] FreezerDto freezerDto)
        {
            if (freezerDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var freezer = _mapper.Map<Freezer>(freezerDto);
            freezer.Id = freezerDto.Id;
            var validate = _freezerRepository.Duplicate(freezer);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _freezerRepository.AddOrUpdate(freezer);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Freezer failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(freezer.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _freezerRepository.Find(id);

            if (record == null)
                return NotFound();

            _freezerRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _freezerRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _freezerRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _freezerRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetFreezerDropDowns")]
        public IActionResult GetFreezerDropDown()
        {
            return Ok(_freezerRepository.GetFreezerDropDown());
        }
    }
}