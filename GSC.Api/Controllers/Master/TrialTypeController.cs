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
    public class TrialTypeController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ITrialTypeRepository _trialTypeRepository;
        private readonly IUnitOfWork<GscContext> _uow;

        public TrialTypeController(ITrialTypeRepository trialTypeRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _trialTypeRepository = trialTypeRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var trialTypes = _trialTypeRepository.All.Where(x => x.IsDeleted == isDeleted
            ).OrderByDescending(x => x.Id).ToList();
            var trialTypesDto = _mapper.Map<IEnumerable<TrialTypeDto>>(trialTypes);
            trialTypesDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(trialTypesDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var trialType = _trialTypeRepository.Find(id);
            var trialTypeDto = _mapper.Map<TrialTypeDto>(trialType);
            return Ok(trialTypeDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TrialTypeDto trialTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            trialTypeDto.Id = 0;
            var trialType = _mapper.Map<TrialType>(trialTypeDto);
            var validate = _trialTypeRepository.Duplicate(trialType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _trialTypeRepository.Add(trialType);
            if (_uow.Save() <= 0) throw new Exception("Creating Trial Type failed on save.");
            return Ok(trialType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TrialTypeDto trialTypeDto)
        {
            if (trialTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var trialType = _mapper.Map<TrialType>(trialTypeDto);
            var validate = _trialTypeRepository.Duplicate(trialType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(trialType.Id);
            trialType.Id = 0;
            _trialTypeRepository.Add(trialType);

            if (_uow.Save() <= 0) throw new Exception("Updating Trail Type failed on save.");
            return Ok(trialType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _trialTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _trialTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _trialTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _trialTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _trialTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTrialTypeDropDown")]
        public IActionResult GetTrialTypeDropDown()
        {
            return Ok(_trialTypeRepository.GetTrialTypeDropDown());
        }
    }
}