using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class LanguageController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public LanguageController(ILanguageRepository languageRepository,
            IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("list")]
        public IActionResult GetList()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var existing = _languageRepository.All.Select(x => x.Culture.ToLower());
            var languages = cultures.Where(x => !existing.Contains(x.Name.ToLower()))
                .Select(x => new LanguageDto
                {
                    LanguageName = x.DisplayName,
                    Culture = x.Name
                });
            return Ok(languages);
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
           
            var languages = _languageRepository.FindBy(x => x.IsDeleted == isDeleted).OrderByDescending(x => x.Id).ToList();

            var languagesDto = _mapper.Map<IEnumerable<LanguageDto>>(languages);
            languagesDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(languagesDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var language = _languageRepository.Find(id);

            return Ok(_mapper.Map<LanguageDto>(language));
        }

        [HttpPost]
        public IActionResult Post([FromBody] LanguageDto dto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            dto.Id = 0;
            var language = _mapper.Map<Language>(dto);
            var validate = _languageRepository.Duplicate(language);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _languageRepository.Add(language);
            if (_uow.Save() <= 0) throw new Exception("Creating Marital Status failed on save.");
            return Ok(language.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] LanguageDto dto)
        {
            var language = _mapper.Map<Language>(dto);
            var validate = _languageRepository.Duplicate(language);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(language.Id);
            language.Id = 0;
            _languageRepository.Add(language);

            if (_uow.Save() <= 0) throw new Exception("Updating Language failed on save.");

            return Ok(language.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _languageRepository.Find(id);

            if (record == null)
                return NotFound();

            _languageRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _languageRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _languageRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _languageRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetLanguageDropDown")]
        public IActionResult GetLanguageDropDown()
        {
            return Ok(_languageRepository.GetLanguageDropDown());
        }
    }
}