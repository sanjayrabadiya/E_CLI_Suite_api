using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class FoodTypeController : BaseController
    {
        private readonly IFoodTypeRepository _foodTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public FoodTypeController(IFoodTypeRepository foodTypeRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _foodTypeRepository = foodTypeRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var foodTypes = _foodTypeRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var foodTypesDto = _mapper.Map<IEnumerable<FoodTypeDto>>(foodTypes);

            //foodTypesDto.ForEach(b =>
            //{
            //    b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
            //    if (b.ModifiedBy != null)
            //        b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
            //    if (b.DeletedBy != null)
            //        b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
            //    if (b.CompanyId != null)
            //        b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            //});
            return Ok(foodTypesDto);
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
            if (_uow.Save() <= 0) throw new Exception("Creating Food Type failed on save.");
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

            if (_uow.Save() <= 0) throw new Exception("Updating Food Type failed on save.");
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