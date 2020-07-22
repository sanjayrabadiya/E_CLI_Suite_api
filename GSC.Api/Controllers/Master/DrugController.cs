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
    public class DrugController : BaseController
    {

        private readonly IDrugRepository _drugRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DrugController(IDrugRepository drugRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _drugRepository = drugRepository;
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
            var drugs = _drugRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();

            var drugsDto = _mapper.Map<IEnumerable<DrugDto>>(drugs).ToList();

            drugsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                   b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(drugsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var drug = _drugRepository.Find(id);
            var drugDto = _mapper.Map<DrugDto>(drug);
            return Ok(drugDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] DrugDto drugDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            drugDto.Id = 0;
            var drug = _mapper.Map<Drug>(drugDto);
            var validate = _drugRepository.Duplicate(drug);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _drugRepository.Add(drug);
            if (_uow.Save() <= 0) throw new Exception("Creating Drug failed on save.");
            return Ok(drug.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DrugDto drugDto)
        {
            if (drugDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var drug = _mapper.Map<Drug>(drugDto);
            var validate = _drugRepository.Duplicate(drug);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _drugRepository.AddOrUpdate(drug);

            if (_uow.Save() <= 0) throw new Exception("Updating Drug failed on save.");
            return Ok(drug.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _drugRepository.Find(id);

            if (record == null)
                return NotFound();

            _drugRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _drugRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _drugRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _drugRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDrugDropDown")]
        public IActionResult GetDrugDropDown()
        {
            return Ok(_drugRepository.GetDrugDropDown());
        }
    }
}