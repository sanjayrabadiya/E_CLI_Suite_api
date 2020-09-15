using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    public class VariableTemplateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableTemplateDetailRepository _variableTemplateDetailRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;

        public VariableTemplateController(IVariableTemplateRepository variableTemplateRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IVariableTemplateDetailRepository variableTemplateDetailRepository)
        {
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _variableTemplateDetailRepository = variableTemplateDetailRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var variableTemplates = _variableTemplateRepository.All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<VariableTemplateGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return Ok(variableTemplates);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variableTemplate = _variableTemplateRepository
                .FindByInclude(t => t.Id == id, t => t.Notes, d => d.VariableTemplateDetails).FirstOrDefault();
            variableTemplate.Notes = variableTemplate.Notes.Where(t => t.DeletedDate == null).ToList();

            var variableTemplateDto = _mapper.Map<VariableTemplateDto>(variableTemplate);

            variableTemplateDto.VariableTemplateDetails = _variableTemplateDetailRepository
                .FindByInclude(x => x.VariableTemplateId == id && x.DeletedBy == null, t => t.Variable).Select(
                    c => new VariableTemplateDetailDto
                    {
                        VariableId = c.VariableId,
                        SeqNo = c.SeqNo,
                        Note = c.Note,
                        Name = c.Variable?.VariableName,
                        Type = c.Variable == null ? CoreVariableType.Expected : c.Variable.CoreVariableType,
                        CollectionSourcesName = c.Variable.CollectionSource.ToString(),
                        DataTypeName = c.Variable.DataType.ToString()
                    }).OrderBy(c => c.SeqNo).ToList();
            return Ok(variableTemplateDto);
        }

        [HttpGet("Preview/{id}")]
        public IActionResult Preview(int id)
        {
            if (id <= 0) return BadRequest();
            var variableTemplate = _variableTemplateRepository.GetTemplate(id);
            return Ok(variableTemplate);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableTemplateDto variableTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            variableTemplateDto.Id = 0;
            var variableTemplate = _mapper.Map<VariableTemplate>(variableTemplateDto);
            var validate = _variableTemplateRepository.Duplicate(variableTemplate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableTemplateRepository.Add(variableTemplate);
            if (_uow.Save() <= 0) throw new Exception("Creating Variable Template failed on save.");
            return Ok(variableTemplate.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableTemplateDto variableTemplateDto)
        {
            if (variableTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var variableTemplate = _mapper.Map<VariableTemplate>(variableTemplateDto);
            var validate = _variableTemplateRepository.Duplicate(variableTemplate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            UpdateTemplateDetail(variableTemplate);

            for (var i = 0; i < variableTemplate.VariableTemplateDetails.Count; i++)
                variableTemplate.VariableTemplateDetails[i].SeqNo = i + 1;

            _variableTemplateRepository.Update(variableTemplate);

            if (_uow.Save() <= 0) throw new Exception("Updating Variable Template failed on save.");
            return Ok(variableTemplate.Id);
        }

        private void UpdateTemplateDetail(VariableTemplate variableTemplate)
        {
            var details = _variableTemplateDetailRepository.FindBy(x => x.VariableTemplateId == variableTemplate.Id);
            if (details != null)
                foreach (var varItem in details)
                {
                    varItem.DeletedDate = DateTime.Now;
                    _variableTemplateDetailRepository.Update(varItem);
                }

            for (var i = 0; i < variableTemplate.VariableTemplateDetails.Count; i++)
            {
                var result = details.FirstOrDefault(x =>
                    x.VariableId == variableTemplate.VariableTemplateDetails[i].VariableId);

                if (result != null)
                {
                    result.DeletedDate = null;
                    result.Note = variableTemplate.VariableTemplateDetails[i].Note;
                    result.DeletedBy = null;
                    variableTemplate.VariableTemplateDetails[i] = result;
                }

                variableTemplate.VariableTemplateDetails[i].SeqNo = i + 1;
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            _variableTemplateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _variableTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _variableTemplateRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableTemplateRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVariableTemplateDropDown")]
        public IActionResult GetVariableTemplateDropDown()
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateDropDown());
        }

        [HttpGet]
        [Route("GetVariableTemplateByDomainId/{domainId}")]
        public IActionResult GetVariableTemplateByDomainId(int domainId)
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateByDomainId(domainId));
        }
    }
}