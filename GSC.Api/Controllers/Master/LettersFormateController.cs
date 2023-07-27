using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class LettersFormateController : BaseController
    {
        private readonly ILettersFormateRepository _lettersFormateRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public LettersFormateController(ILettersFormateRepository lettersFormateRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _lettersFormateRepository = lettersFormateRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var lettersFormate = _lettersFormateRepository.GetlettersFormateList(isDeleted);
            return Ok(lettersFormate);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var lettersFormate = _lettersFormateRepository.Find(id);
            var lettersFormateDto = _mapper.Map<LettersFormateDto>(lettersFormate);
            return Ok(lettersFormateDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] LettersFormateDto lettersFormateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            lettersFormateDto.Id = 0;
            var lettersFormate = _mapper.Map<LettersFormate>(lettersFormateDto);

            var validate = _lettersFormateRepository.Duplicate(lettersFormate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _lettersFormateRepository.Add(lettersFormate);
            if (_uow.Save() <= 0) throw new Exception("letters Formate failed on save.");
            return Ok(lettersFormate.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LettersFormateDto lettersFormateDto)
        {
            if (lettersFormateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lettersFormate = _mapper.Map<LettersFormate>(lettersFormateDto);
            var validate = _lettersFormateRepository.Duplicate(lettersFormate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _lettersFormateRepository.Update(lettersFormate);
            if (_uow.Save() <= 0) throw new Exception("Updating letters Formatefailed on save.");
            return Ok(lettersFormate.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _lettersFormateRepository.Find(id);

            if (record == null)
                return NotFound();

            _lettersFormateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _lettersFormateRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _lettersFormateRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _lettersFormateRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}