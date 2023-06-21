using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Entities.LanguageSetup;
using GSC.Respository.LanguageSetup;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.LanguageSetup
{
    [Route("api/[controller]")]
    [ApiController]
    public class VariableLabelLanguageController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IVariabeLabelLanguageRepository _variableLabelLanguageRepository;
        private readonly IUnitOfWork _uow;
        IJwtTokenAccesser _jwtTokenAccesser;

        public VariableLabelLanguageController(
            IUnitOfWork uow, IMapper mapper,
            IVariabeLabelLanguageRepository variableLabelLanguageRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _variableLabelLanguageRepository = variableLabelLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet]
        [Route("GetVariableLabelLanguage/{VariableId}")]
        public IActionResult GetVariableLabelLanguage(int VariableId)
        {
            var variableLabelLanguage = _variableLabelLanguageRepository.GetVariableLabelLanguageList(VariableId);
            return Ok(variableLabelLanguage);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableLabelLanguage = _variableLabelLanguageRepository.Find(id);
            var variableLabelLanguageDto = _mapper.Map<VariableLabelLanguageDto>(variableLabelLanguage);
            return Ok(variableLabelLanguageDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableLabelLanguageDto variableLabelLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            foreach (var item in variableLabelLanguageDto.variableLabelLanguages)
            {
                if (item.Id == 0)
                {
                    VariableLabelLanguage language = new VariableLabelLanguage();
                    language.ProjectDesignVariableId = item.ProjectDesignVariableId;
                    language.LanguageId = item.LanguageId;
                    language.Display = item.Display;
                    _variableLabelLanguageRepository.Add(language);
                }
            }
            if (_uow.Save() <= 0) throw new Exception("Creating variable label language failed on save.");
            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableLabelLanguageRepository.Find(id);
            if (record == null)
                return NotFound();
            _variableLabelLanguageRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}
