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
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class FoodTypeController : BaseController
    {
        private readonly IFoodTypeRepository _foodTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public FoodTypeController(IFoodTypeRepository foodTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _foodTypeRepository = foodTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var foodtype = _foodTypeRepository.GetFoodTypeList(isDeleted);
            return Ok(foodtype);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var foodType = _foodTypeRepository.Find(id);
            var foodTypeDto = _mapper.Map<FoodTypeDto>(foodType);
            return Ok(foodTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] FoodTypeDto foodTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foodTypeDto.Id = 0;
            var foodType = _mapper.Map<FoodType>(foodTypeDto);
            var validate = _foodTypeRepository.Duplicate(foodType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _foodTypeRepository.Add(foodType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Food Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(foodType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] FoodTypeDto foodTypeDto)
        {
            if (foodTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var foodType = _mapper.Map<FoodType>(foodTypeDto);
            foodType.Id = foodTypeDto.Id;
            var validate = _foodTypeRepository.Duplicate(foodType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _foodTypeRepository.AddOrUpdate(foodType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Food Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(foodType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _foodTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _foodTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _foodTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _foodTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _foodTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetFoodTypeDropDown")]
        public IActionResult GetFoodTypeDropDown()
        {
            return Ok(_foodTypeRepository.GetFoodTypeDropDown());
        }
    }
}