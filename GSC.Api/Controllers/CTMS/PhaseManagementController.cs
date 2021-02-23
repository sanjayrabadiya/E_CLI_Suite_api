using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class PhaseManagementController : BaseController
    {
        
        private readonly IPhaseManagementRepository _phasemanagementRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PhaseManagementController(IPhaseManagementRepository phasemanagementRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _phasemanagementRepository = phasemanagementRepository;
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

            var phasemanagement = _phasemanagementRepository.GetPhaseManagementList(isDeleted);
            return Ok(phasemanagement);
            //var  phasemanagement  = _phasemanagementRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var phasemanagement = _phasemanagementRepository.Find(id);
            var phasemanagementDto = _mapper.Map<PhaseManagementDto>(phasemanagement);
            return Ok(phasemanagementDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PhaseManagementDto phasemanagementDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            phasemanagementDto.Id = 0;
            var PhaseManagement = _mapper.Map<PhaseManagement>(phasemanagementDto);
            var validate = _phasemanagementRepository.Duplicate(PhaseManagement);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _phasemanagementRepository.Add(PhaseManagement);
            if (_uow.Save() <= 0) throw new Exception("Creating PhaseManagement failed on save.");
            return Ok(PhaseManagement.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PhaseManagementDto phasemanagementDto)
        {
            if (phasemanagementDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var phasemanagement = _mapper.Map<PhaseManagement>(phasemanagementDto);
            var validate = _phasemanagementRepository.Duplicate(phasemanagement);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _phasemanagementRepository.AddOrUpdate(phasemanagement);

            if (_uow.Save() <= 0) throw new Exception("Updating PhaseManagement failed on save.");
            return Ok(phasemanagement.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _phasemanagementRepository.Find(id);

            if (record == null)
                return NotFound();

            _phasemanagementRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _phasemanagementRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _phasemanagementRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _phasemanagementRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPhaseManagementDropDown")]
        public IActionResult GetPhaseManagementDropDown()
        {
            return Ok(_phasemanagementRepository.GetPhaseManagementDropDown());
        }
    }
}

    

