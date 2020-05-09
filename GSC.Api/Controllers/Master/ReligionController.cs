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
    public class ReligionController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IReligionRepository _religionRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public ReligionController(IReligionRepository religionRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _religionRepository = religionRepository;
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
            var religions = _religionRepository.All.Where(x =>x.IsDeleted == isDeleted
            ).OrderByDescending(x => x.Id).ToList();
            var religionsDto = _mapper.Map<IEnumerable<ReligionDto>>(religions);
            religionsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(religionsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var religion = _religionRepository.Find(id);
            var religionDto = _mapper.Map<ReligionDto>(religion);
            return Ok(religionDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ReligionDto religionDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            religionDto.Id = 0;
            var religion = _mapper.Map<Religion>(religionDto);
            var validate = _religionRepository.Duplicate(religion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _religionRepository.Add(religion);
            if (_uow.Save() <= 0) throw new Exception("Creating Religion failed on save.");
            return Ok(religion.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ReligionDto religionDto)
        {
            if (religionDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var religion = _mapper.Map<Religion>(religionDto);
            religion.Id = religionDto.Id;
            var validate = _religionRepository.Duplicate(religion);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(religion.Id);
            religion.Id = 0;
            _religionRepository.Add(religion);

            if (_uow.Save() <= 0) throw new Exception("Updating Religion failed on save.");
            return Ok(religion.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _religionRepository.Find(id);

            if (record == null)
                return NotFound();

            _religionRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _religionRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _religionRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _religionRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetReligionDropDown")]
        public IActionResult GetReligionDropDown()
        {
            return Ok(_religionRepository.GetReligionDropDown());
        }
    }
}