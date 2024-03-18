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
    public class VariableLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVariabeLanguageRepository _variableLanguageRepository;
        private readonly IUnitOfWork _uow;

        public VariableLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVariabeLanguageRepository variableLanguageRepository)
        {
            _variableLanguageRepository = variableLanguageRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetVariableLanguage/{VariableId}")]
        public IActionResult GetVariableLanguage(int VariableId)
        {
            var variableLanguage = _variableLanguageRepository.GetVariableLanguageList(VariableId);
            return Ok(variableLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableLanguage = _variableLanguageRepository.Find(id);
            var variableLanguageDto = _mapper.Map<VariableLanguageDto>(variableLanguage);
            return Ok(variableLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableLanguageDto variableLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in variableLanguageDto.variableLanguages)
            {
                if (item.Id == 0)
                {
                    VariableLanguage language = new VariableLanguage();
                    language.ProjectDesignVariableId = item.ProjectDesignVariableId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _variableLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating variable language failed on save.");
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _variableLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
