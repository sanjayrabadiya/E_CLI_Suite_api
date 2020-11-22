using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Medra;
using GSC.Respository.UserMgt;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class MedraLanguageController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMedraLanguageRepository _medraLanguageRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;

        public MedraLanguageController(IMedraLanguageRepository medraLanguageRepository,
                        IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _medraLanguageRepository = medraLanguageRepository;
            _uow = uow;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public IActionResult GetList()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var existing = _medraLanguageRepository.All.Select(x => x.Culture.ToLower());
            var languages = cultures.Where(x => !existing.Contains(x.Name.ToLower()))
                .Select(x => new MedraLanguageDto
                {
                    LanguageName = x.DisplayName,
                    Culture = x.Name
                });
            return Ok(languages);
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var languages = _medraLanguageRepository.GetMedraLanguageList(isDeleted);
            return Ok(languages);
            //var language = _medraLanguageRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();
            //return Ok(languages);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var language = _medraLanguageRepository.Find(id);

            return Ok(_mapper.Map<MedraLanguageDto>(language));
        }

        [HttpPost]
        public IActionResult Post([FromBody] MedraLanguageDto dto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            dto.Id = 0;
            var language = _mapper.Map<MedraLanguage>(dto);
            var validate = _medraLanguageRepository.Duplicate(language);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _medraLanguageRepository.Add(language);
            if (_uow.Save() <= 0) throw new Exception("Creating Marital Status failed on save.");
            return Ok(language.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] MedraLanguageDto dto)
        {
            var language = _mapper.Map<MedraLanguage>(dto);
            var validate = _medraLanguageRepository.Duplicate(language);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            //Delete(language.Id);
            //language.Id = 0;
            //_medraLanguageRepository.Add(language);
            /* Added by vipul for new update method */
            _medraLanguageRepository.AddOrUpdate(language);

            if (_uow.Save() <= 0) throw new Exception("Updating Language failed on save.");

            return Ok(language.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _medraLanguageRepository.Find(id);

            if (record == null)
                return NotFound();

            _medraLanguageRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _medraLanguageRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _medraLanguageRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _medraLanguageRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetLanguageDropDown")]
        public IActionResult GetLanguageDropDown()
        {
            return Ok(_medraLanguageRepository.GetLanguageDropDown());
        }
    }
}