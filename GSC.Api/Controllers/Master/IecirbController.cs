using System;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class IecirbController : BaseController
    {
        private readonly IIecirbRepository _iecirbRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public IecirbController(IIecirbRepository iecirbRepository,
        IUnitOfWork uow, IMapper mapper)
        {
            _iecirbRepository = iecirbRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();

            var iecirb = _iecirbRepository.All.Where(x => x.ManageSiteId == id && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                    ProjectTo<IecirbGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return Ok(iecirb);
        }

        [HttpGet]
        [Route("GetIecirbChange/{id}")]
        public IActionResult GetIecirbChange(int id)
        {
            var iecirb = _iecirbRepository.FindByInclude(x => x.Id == id).SingleOrDefault();
            if (iecirb == null)
                return BadRequest();

            var iecirbDto = _mapper.Map<IecirbDto>(iecirb);

            return Ok(iecirbDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] IecirbDto iecirbDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            iecirbDto.Id = 0;
            var iecirb = _mapper.Map<Iecirb>(iecirbDto);

            var validate = _iecirbRepository.Duplicate(iecirb);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _iecirbRepository.Add(iecirb);


            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating IEC/IRB failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(iecirb.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] IecirbDto iecirbDto)
        {
            if (iecirbDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var iecirb = _mapper.Map<Iecirb>(iecirbDto);

            var validate = _iecirbRepository.Duplicate(iecirb);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            iecirb.Id = iecirbDto.Id;

            /* Added by Darshil for effective Date on 24-07-2020 */
            _iecirbRepository.AddOrUpdate(iecirb);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating IEC/IRB failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(iecirb.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _iecirbRepository.Find(id);

            if (record == null)
                return NotFound();

            _iecirbRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _iecirbRepository.Find(id);

            if (record == null)
                return NotFound();
            _iecirbRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetIecirbDropDown")]
        public IActionResult GetIecirbDropDown()
        {
            return Ok(_iecirbRepository.GetIecirbDropDown());
        }
    }
}
