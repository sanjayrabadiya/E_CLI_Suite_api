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
    public class DesignTrialController : BaseController
    {
        private readonly IDesignTrialRepository _designTrialRepository;
        private readonly ITrialTypeRepository _trialTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DesignTrialController(IDesignTrialRepository designTrialRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ITrialTypeRepository trialTypeRepository)
        {
            _designTrialRepository = designTrialRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _trialTypeRepository = trialTypeRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var designTrials = _designTrialRepository.GetDesignTrialList(isDeleted);
            designTrials.ForEach(b =>
            {
                b.TrialType = _trialTypeRepository.Find(b.TrialTypeId);
            });
            return Ok(designTrials);
            //var designTrials = _designTrialRepository.FindByInclude(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            //    , t => t.TrialType).OrderByDescending(x => x.Id).ToList();
            //return Ok(designTrialsDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var designTrial = _designTrialRepository.Find(id);
            var designTrialDto = _mapper.Map<DesignTrialDto>(designTrial);
            return Ok(designTrialDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] DesignTrialDto designTrialDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            designTrialDto.Id = 0;
            var designTrial = _mapper.Map<DesignTrial>(designTrialDto);
            var validate = _designTrialRepository.Duplicate(designTrial);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _designTrialRepository.Add(designTrial);
            if (_uow.Save() <= 0) throw new Exception("Creating Design Trial failed on save.");
            return Ok(designTrial.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DesignTrialDto designTrialDto)
        {
            if (designTrialDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var designTrial = _mapper.Map<DesignTrial>(designTrialDto);
            var validate = _designTrialRepository.Duplicate(designTrial);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _designTrialRepository.AddOrUpdate(designTrial);

            if (_uow.Save() <= 0) throw new Exception("Updating Design Trial failed on save.");
            return Ok(designTrial.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _designTrialRepository.Find(id);

            if (record == null)
                return NotFound();

            _designTrialRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _designTrialRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _designTrialRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _designTrialRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDesignTrialDropDown")]
        public IActionResult GetDesignTrialDropDown()
        {
            return Ok(_designTrialRepository.GetDesignTrialDropDown());
        }

        [HttpGet]
        [Route("GetDesignTrialDropDownByTrialType/{id}")]
        public IActionResult GetDesignTrialDropDownByTrialType(int id)
        {
            return Ok(_designTrialRepository.GetDesignTrialDropDownByTrialType(id));
        }
    }
}