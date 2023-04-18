using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageSiteAddressController : BaseController
    {
        private readonly IManageSiteAddressRepository _manageSiteAddressRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public ManageSiteAddressController(IManageSiteAddressRepository manageSiteAddressRepository,
            IUserRepository userRepository,
            ICityRepository cityRepository,
            ICompanyRepository companyRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context)
        {
            _manageSiteAddressRepository = manageSiteAddressRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }


        // GET: api/<controller>
        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            var manageSiteAddress = _manageSiteAddressRepository.GetManageSiteAddress(id, isDeleted);
            return Ok(manageSiteAddress);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var mangeSite = _manageSiteAddressRepository.FindByInclude(q => q.Id == id, x => x.City, x => x.City.State).FirstOrDefault();
            var manageSiteDto = _mapper.Map<ManageSiteAddressDto>(mangeSite);
            manageSiteDto.StateId = mangeSite.City.StateId;
            manageSiteDto.CountryId = mangeSite.City.State.CountryId;
            manageSiteDto.Facilities = mangeSite.Facilities.Split(',').ToList();
            return Ok(manageSiteDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ManageSiteAddressDto manageSiteAddressDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageSiteAddressDto.Id = 0;
            var manageSiteAddress = _mapper.Map<ManageSiteAddress>(manageSiteAddressDto);
            manageSiteAddress.Facilities = manageSiteAddressDto.Facilities?.Aggregate((a, b) => a + "," + b);
            var validate = _manageSiteAddressRepository.Duplicate(manageSiteAddress);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _manageSiteAddressRepository.Add(manageSiteAddress);
            if (_uow.Save() <= 0) throw new Exception("Creating Site failed on save.");
            return Ok(manageSiteAddress.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] ManageSiteAddressDto manageSiteAddressDto)
        {
            if (manageSiteAddressDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageSiteAddress = _mapper.Map<ManageSiteAddress>(manageSiteAddressDto);
            manageSiteAddress.Facilities = manageSiteAddressDto.Facilities?.Aggregate((a, b) => a + "," + b);
            var validate = _manageSiteAddressRepository.Duplicate(manageSiteAddress);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _manageSiteAddressRepository.Update(manageSiteAddress);

            if (_uow.Save() <= 0) throw new Exception("Updating Site failed on save.");
            return Ok(manageSiteAddress.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _manageSiteAddressRepository.Find(id);

            if (record == null)
                return NotFound();

            _manageSiteAddressRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _manageSiteAddressRepository.Find(id);

            if (record == null)
                return NotFound();
            _manageSiteAddressRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetSiteAddressDropdown/{id}")]
        public IActionResult GetSiteAddressDropdown(int id)
        {
            return Ok(_manageSiteAddressRepository.GetSiteAddressDropdown(id));
        }

        [HttpGet("GetSiteAddressDropdownForMangeStudy/{projectId}/{manageSiteId}")]
        public IActionResult GetSiteAddressDropdownForMangeStudy(int projectId, int manageSiteId)
        {
            return Ok(_manageSiteAddressRepository.GetSiteAddressDropdownForMangeStudy(projectId, manageSiteId));
        }

    }
}
