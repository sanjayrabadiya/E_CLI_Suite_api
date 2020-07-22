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
    public class OccupationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IOccupationRepository _occupationRepository;
        private readonly IUnitOfWork _uow;

        public OccupationController(IOccupationRepository occupationRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _occupationRepository = occupationRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var occupations = _occupationRepository.All.Where(x =>
                 isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var occupationsDto = _mapper.Map<IEnumerable<OccupationDto>>(occupations);
            occupationsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(occupationsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var occupation = _occupationRepository.Find(id);
            var occupationDto = _mapper.Map<OccupationDto>(occupation);
            return Ok(occupationDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] OccupationDto occupationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            occupationDto.Id = 0;
            var occupation = _mapper.Map<Occupation>(occupationDto);
            var validate = _occupationRepository.Duplicate(occupation);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _occupationRepository.Add(occupation);
            if (_uow.Save() <= 0) throw new Exception("Creating Occupation failed on save.");
            return Ok(occupation.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] OccupationDto occupationDto)
        {
            if (occupationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var occupation = _mapper.Map<Occupation>(occupationDto);
            var validate = _occupationRepository.Duplicate(occupation);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _occupationRepository.AddOrUpdate(occupation);

            if (_uow.Save() <= 0) throw new Exception("Updating Occupation failed on save.");
            return Ok(occupation.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _occupationRepository.Find(id);

            if (record == null)
                return NotFound();

            _occupationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _occupationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _occupationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _occupationRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetOccupationDropDown")]
        public IActionResult GetOccupationDropDown()
        {
            return Ok(_occupationRepository.GetOccupationDropDown());
        }
    }
}