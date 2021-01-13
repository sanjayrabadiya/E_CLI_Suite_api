using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Respository.LanguageSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LanguageSetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariableValueLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVariabeValueLanguageRepository _variableValueLanguageRepository;
        private readonly IUnitOfWork _uow;
        IJwtTokenAccesser _jwtTokenAccesser;

        public VariableValueLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVariabeValueLanguageRepository variableValueLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableValueLanguageRepository = variableValueLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetVariableValueLanguage/{VariableValueId}")]
        public IActionResult GetVariableValueLanguage(int VariableValueId)
        {
            var variableValueLanguage = _variableValueLanguageRepository.GetVariableValueLanguageList(VariableValueId);
            return Ok(variableValueLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableValueLanguage = _variableValueLanguageRepository.Find(id);
            var variableValueLanguageDto = _mapper.Map<VariableValueLanguageDto>(variableValueLanguage);
            return Ok(variableValueLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableValueLanguageDto variableValueLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in variableValueLanguageDto.VariableValueLanguages)
            {
                if (item.Id == 0)
                {
                    VariableValueLanguage language = new VariableValueLanguage();
                    language.ProjectDesignVariableValueId = item.ProjectDesignVariableValueId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    language.LabelName = item.LabelName;
                    _variableValueLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating variable value language failed on save.");
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableValueLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _variableValueLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
