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
using GSC.Respository.Configuration;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
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
        private readonly IVisitLanguageRepository _visitLanguageRepository;
        private readonly ITemplateLanguageRepository _templateLanguageRepository;
        private readonly IVariableCategoryLanguageRepository _variabeCategoryLanguageRepository;
        private readonly IVariabeLanguageRepository _variabeLanguageRepository;
        private readonly IVariabeNoteLanguageRepository _variabeNoteLanguageRepository;
        private readonly IVariabeValueLanguageRepository _variabeValueLanguageRepository;
        private readonly ITemplateNoteLanguageRepository _templateNoteLanguageRepository;

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public LanguageController(ILanguageRepository languageRepository,
            IUnitOfWork uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IVisitLanguageRepository visitLanguageRepository,
            ITemplateLanguageRepository templateLanguageRepository,
            IVariableCategoryLanguageRepository variabeCategoryLanguageRepository,
            IVariabeLanguageRepository variabeLanguageRepository,
            IVariabeNoteLanguageRepository variabeNoteLanguageRepository,
            IVariabeValueLanguageRepository variabeValueLanguageRepository,
            ITemplateNoteLanguageRepository templateNoteLanguageRepository,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _visitLanguageRepository = visitLanguageRepository;
            _templateLanguageRepository = templateLanguageRepository;
            _variabeCategoryLanguageRepository = variabeCategoryLanguageRepository;
            _variabeLanguageRepository = variabeLanguageRepository;
            _variabeNoteLanguageRepository = variabeNoteLanguageRepository;
            _variabeValueLanguageRepository = variabeValueLanguageRepository;
            _templateNoteLanguageRepository = templateNoteLanguageRepository;
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
            var languages = _languageRepository.GetLanguageList(isDeleted);
            return Ok(languages);
            //var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            //var languages = _languageRepository.FindBy(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).ToList();

            //return Ok(languagesDto);
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

            /* Added by swati for effective Date on 02-06-2019 */
            _languageRepository.AddOrUpdate(language);

            if (_uow.Save() <= 0) throw new Exception("Updating Language failed on save.");

            return Ok(language.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _languageRepository.Find(id);

            if (record == null)
                return NotFound();


            //check language use in variable category
            var LangExistsInVariableCategory = _variabeCategoryLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInVariableCategory)
            {
                ModelState.AddModelError("Message", "Language use in variable category");
                return BadRequest(ModelState);
            }

            //check language use in visit
            var LangExistsInVisit = _visitLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInVisit)
            {
                ModelState.AddModelError("Message", "Language use in visit");
                return BadRequest(ModelState);
            }

            //check language use in template
            var LangExistsInTemplate = _templateLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInTemplate)
            {
                ModelState.AddModelError("Message", "Language use in template");
                return BadRequest(ModelState);
            }

            //check language use in Variable
            var LangExistsInVariable = _variabeLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInVisit)
            {
                ModelState.AddModelError("Message", "Language use in variable");
                return BadRequest(ModelState);
            }

            //check language use in Variable Note
            var LangExistsInVariableNote = _variabeNoteLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInVariableNote)
            {
                ModelState.AddModelError("Message", "Language use in variable note");
                return BadRequest(ModelState);
            }

            //check language use in Variable value
            var LangExistsInVariableValue = _variabeValueLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInVariableValue)
            {
                ModelState.AddModelError("Message", "Language use in visit");
                return BadRequest(ModelState);
            }

            //check language use in template note
            var LangExistsInTemplateNote = _templateNoteLanguageRepository.IsLanguageExist(id);
            if (!LangExistsInTemplateNote)
            {
                ModelState.AddModelError("Message", "Language use in template note");
                return BadRequest(ModelState);
            }

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