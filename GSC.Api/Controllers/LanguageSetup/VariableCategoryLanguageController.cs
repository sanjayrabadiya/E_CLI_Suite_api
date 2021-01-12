using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Respository.LanguageSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LanguageSetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariableCategoryLanguageController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IVariableCategoryLanguageRepository _variableCategoryLanguageRepository;
        private readonly IUnitOfWork _uow;
        IJwtTokenAccesser _jwtTokenAccesser;

        public VariableCategoryLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVariableCategoryLanguageRepository variableCategoryLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableCategoryLanguageRepository = variableCategoryLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetVariableCategoryLanguage/{VariableId}")]
        public IActionResult GetVariableCategoryLanguage(int VariableId)
        {
            var variableLanguage = _variableCategoryLanguageRepository.GetVariableCategoryLanguageList(VariableId);
            return Ok(variableLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableCategoryLanguage = _variableCategoryLanguageRepository.Find(id);
            var variableCategoryLanguageDto = _mapper.Map<VariableCategoryLanguageDto>(variableCategoryLanguage);
            return Ok(variableCategoryLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableCategoryLanguageDto variableCategoryLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in variableCategoryLanguageDto.variableCategoryLanguages)
            {
                if (item.Id == 0)
                {
                    VariableCategoryLanguage language = new VariableCategoryLanguage();
                    language.VariableCategoryId = item.VariableCategoryId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _variableCategoryLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating variable category language failed on save.");
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableCategoryLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _variableCategoryLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

    }
}
