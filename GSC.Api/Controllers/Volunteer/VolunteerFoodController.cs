using System;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerFoodController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerFoodRepository _volunteerFoodRepository;

        public VolunteerFoodController(IVolunteerFoodRepository volunteerFoodRepository,
            IUnitOfWork uow)
        {
            _volunteerFoodRepository = volunteerFoodRepository;
            _uow = uow;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            return Ok(_volunteerFoodRepository.GetFoods(id));
        }


        [HttpPost]
        public IActionResult Post([FromBody] VolunteerFoodDto volunteerFoodDto)
        {
            _volunteerFoodRepository.SaveFoods(volunteerFoodDto);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating volunteer food failed on save."));
            return NoContent();
        }
    }
}