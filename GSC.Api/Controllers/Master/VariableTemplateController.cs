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
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly IVariableTemplateNoteRepository _variableTemplateNoteRepository;
        private readonly IGSCContext _context;
        private readonly IDomainRepository _domainRepository;

        public VariableTemplateController(IVariableTemplateRepository variableTemplateRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IVariableTemplateDetailRepository variableTemplateDetailRepository,
            IVariableTemplateNoteRepository variableTemplateNoteRepository,
            IDomainRepository domainRepository,
            IGSCContext context)
        {
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _variableTemplateDetailRepository = variableTemplateDetailRepository;
            _variableTemplateNoteRepository = variableTemplateNoteRepository;
            _domainRepository = domainRepository;
            _context = context;
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
                .FindByInclude(x => x.VariableTemplateId == id && x.DeletedBy == null && x.Variable.DeletedDate == null, t => t.Variable)
                .Select(c => new VariableTemplateDetailDto
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
            variableTemplate.VariableTemplateDetails.ForEach(item =>
            {
                _variableTemplateDetailRepository.Add(item);
            });
            variableTemplate.Notes.ToList().ForEach(item =>
            {
                _variableTemplateNoteRepository.Add(item);
            });
            if (_uow.Save() <= 0) throw new Exception("Creating Variable Template failed on save.");
            return Ok(variableTemplate.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableTemplateDto variableTemplateDto)
        {
            if (variableTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lastForm = _variableTemplateRepository.Find(variableTemplateDto.Id);
            var DomainCode = _domainRepository.Find(lastForm.DomainId).DomainCode;
            if (DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }

            var variableTemplate = _mapper.Map<VariableTemplate>(variableTemplateDto);
            var validate = _variableTemplateRepository.Duplicate(variableTemplate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            UpdateTemplateDetail(variableTemplate);

            _variableTemplateRepository.Update(variableTemplate);

            for (var i = 0; i < variableTemplate.VariableTemplateDetails.Count; i++)
            {
                variableTemplate.VariableTemplateDetails[i].SeqNo = i + 1;
                if (variableTemplate.VariableTemplateDetails[i].Id > 0)
                    _variableTemplateDetailRepository.Update(variableTemplate.VariableTemplateDetails[i]);
                else
                    _variableTemplateDetailRepository.Add(variableTemplate.VariableTemplateDetails[i]);
            }

            var notes = _variableTemplateNoteRepository.All.Where(x => x.VariableTemplateId == variableTemplateDto.Id).ToList();
            if (notes.Any())
            {
                foreach (var item in notes)
                {
                    _variableTemplateNoteRepository.Remove(item);
                    _uow.Save();
                    _context.Entry(item).State = EntityState.Detached;
                }
            }
           
            for (var i = 0; i < variableTemplate.Notes.Count; i++)
            {
                _context.Entry(variableTemplate.Notes[i]).State = EntityState.Detached;
                variableTemplate.Notes[i].Id = 0;
                //if (variableTemplate.Notes[i].Id > 0)
                //    _variableTemplateNoteRepository.Update(variableTemplate.Notes[i]);
                //else
                    _variableTemplateNoteRepository.Add(variableTemplate.Notes[i]);
            }

            if (_uow.Save() <= 0) throw new Exception("Updating Variable Template failed on save.");
            return Ok(variableTemplate.Id);
        }

        private void UpdateTemplateDetail(VariableTemplate variableTemplate)
        {
            var details = _variableTemplateDetailRepository.FindBy(x => x.VariableTemplateId == variableTemplate.Id);
            if (details != null)
                foreach (var varItem in details)
                {
                    varItem.DeletedBy = _jwtTokenAccesser.UserId;
                    varItem.DeletedDate = _jwtTokenAccesser.GetClientDate();
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
                    variableTemplate.VariableTemplateDetails[i].SeqNo = i + 1;
                    _variableTemplateDetailRepository.Update(result);
                }
                else
                {
                    variableTemplate.VariableTemplateDetails[i].SeqNo = i + 1;
                    _variableTemplateDetailRepository.Add(variableTemplate.VariableTemplateDetails[i]);
                }

            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            record.Domain = _domainRepository.Find((int)record.DomainId);
            if (record.Domain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

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

            var domain = _context.Domain.Where(x => x.Id == record.DomainId).FirstOrDefault();
            if (domain?.DeletedDate != null)
            {
                var message = "Domain Is Deacitvated.";
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }

            var variableTemplateDetails = _variableTemplateDetailRepository.FindByInclude(x => x.VariableTemplateId == record.Id).ToList();

            foreach (var item in variableTemplateDetails)
            {
                var variable = _context.Variable.Where(a => a.Id == item.VariableId).FirstOrDefault();
                if (variable?.DeletedDate != null)
                {
                    var messagevar = "Variable Is Deacitvated.";
                    ModelState.AddModelError("Message", messagevar);
                    return BadRequest(ModelState);
                }
            }

            _variableTemplateRepository.Active(record);
            variableTemplateDetails.ForEach(z =>
            {
                _variableTemplateDetailRepository.Active(z);
            });

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
        [Route("GetVariableTemplateNonCRFDropDown")]
        public IActionResult GetVariableTemplateNonCRFDropDown()
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateNonCRFDropDown());
        }

        [HttpGet]
        [Route("GetVariableTemplateByDomainId/{domainId}")]
        public IActionResult GetVariableTemplateByDomainId(int domainId)
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateByDomainId(domainId));
        }

        [HttpGet]
        [Route("GetVariableTemplateByCRFByDomainId/{isNonCRF}/{domainId}")]
        public IActionResult GetVariableTemplateByCRFByDomainId(bool isNonCRF, int domainId)
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateByCRFByDomainId(isNonCRF, domainId));
        }

        [HttpGet]
        [Route("GetVariableNotAddedinTemplate/{variableTemplateId}")]
        public IActionResult GetVariableNotAddedinTemplate(int variableTemplateId)
        {
            return Ok(_variableTemplateRepository.GetVariableNotAddedinTemplate(variableTemplateId));
        }

        [HttpGet]
        [Route("GetVariableTemplateByModuleId/{moduleId}")]
        public IActionResult GetVariableTemplateByModuleId(int moduleId)
        {
            return Ok(_variableTemplateRepository.GetVariableTemplateByModuleId(moduleId));
        }
    }
}