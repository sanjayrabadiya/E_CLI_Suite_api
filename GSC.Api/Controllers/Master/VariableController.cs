using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Custom;
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
    public class VariableController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableValueRepository _variableValueRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IVariableTemplateDetailRepository _variableTemplateDetailRepository;
        private readonly IGSCContext _context;
        private readonly IDomainRepository _domainRepository;

        public VariableController(IVariableRepository variableRepository,
            IVariableValueRepository variableValueRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context,
            IVariableTemplateRepository variableTemplateRepository,
            IDomainRepository domainRepository,
            IVariableTemplateDetailRepository variableTemplateDetailRepository)
        {
            _variableTemplateRepository = variableTemplateRepository;
            _variableRepository = variableRepository;
            _variableValueRepository = variableValueRepository;
            _uow = uow;
            _mapper = mapper;
            _variableTemplateDetailRepository = variableTemplateDetailRepository;
            _context = context;
            _domainRepository = domainRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var variables = _variableRepository.GetVariableList(isDeleted);
            return Ok(variables);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variable = _variableRepository.FindByInclude(t => t.Id == id, t => t.Values.Where(x => x.DeletedDate == null)).FirstOrDefault();

            var variableDto = _mapper.Map<VariableDto>(variable);
            if (variableDto != null && variableDto.Values != null)
            {
                variableDto.Values.ToList().ForEach(x =>
                {
                    if (variableDto.CollectionSource == CollectionSources.Table)
                        x.TableCollectionSourceName = x.TableCollectionSource.GetDescription();
                });
            }

            return Ok(variableDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] VariableDto variableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            variableDto.Id = 0;
            var variable = _mapper.Map<Variable>(variableDto);
            var validate = _variableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _variableRepository.Add(variable);
            foreach (var item in variable.Values)
            {
                _variableValueRepository.Add(item);
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Variable failed on save.");
                return BadRequest(ModelState);
            }

            if (variableDto.CoreVariableType == Helper.CoreVariableType.Required)
            {
                _variableTemplateRepository.AddRequieredTemplate(variable);
            }
            return Ok(variable.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableDto variableDto)
        {
            if (variableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //For Static Vaiable
            var checkStatic = _variableRepository.NonChangeVariableCode(variableDto);
            if (!string.IsNullOrEmpty(checkStatic))
            {
                ModelState.AddModelError("Message", checkStatic);
                return BadRequest(ModelState);
            }

            var lastVariable = _variableRepository.Find(variableDto.Id);
            var DomainCode = _domainRepository.Find((int)lastVariable.DomainId).DomainCode;
            if (DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }

            var filteredValues = variableDto.Values
                 .Where(s => s.VariableId == 0)
                 .Select(s =>
                 {
                     s.VariableId = variableDto.Id;
                     s.Id = 0;
                     return s;
                 }).ToList();

            var variable = _mapper.Map<Variable>(filteredValues);
            var validate = _variableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            UpdateVariableValues(variable, variableDto.CollectionValueDisable);
            _variableRepository.Update(variable);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Variable failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(variable.Id);
        }

        private void UpdateVariableValues(Variable variable, bool CollectionValueDisable)
        {
            if (CollectionValueDisable)
            {
                var deletedisableValues = variable.Values.ToList();

                foreach (var item in deletedisableValues)
                {
                    _variableValueRepository.Delete(item);
                }
            }
            else
            {
                var data = _variableValueRepository.FindBy(x => x.VariableId == variable.Id).ToList();
                var deleteValues = data.Where(t => variable.Values.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
                var addvariables = variable.Values.Where(x => x.Id == 0).ToList();
                var updatevariables = variable.Values.Where(x => x.Id > 0).ToList();
                foreach (var value in deleteValues)
                    _variableValueRepository.Remove(value);

                foreach (var value in updatevariables)
                    _variableValueRepository.Update(value);
                foreach (var item in addvariables)
                {
                    _variableValueRepository.Add(item);
                }
            }
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableRepository.Find(id);

            if (record == null)
                return NotFound();

            record.Domain = _domainRepository.Find((int)record.DomainId);
            if (record.Domain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            if (record.SystemType != null)
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            _variableRepository.Delete(record);

            var variableTemplateDetails = _variableTemplateDetailRepository.FindByInclude(x => x.VariableId == id).ToList();

            variableTemplateDetails.ForEach(temp =>
            {
                _variableTemplateDetailRepository.Delete(temp);
            });
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _variableRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _variableRepository.Duplicate(record);
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

            _variableRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVariableDropDown")]
        public IActionResult GetVariableDropDown()
        {
            return Ok(_variableRepository.GetVariableDropDown());
        }

        [HttpGet]
        [Route("GetVariableListByDomainId/{domainId}")]
        public IActionResult GetVariableListByDomainId(int domainId)
        {
            return Ok(_variableRepository.GetVariableListByDomainId(domainId));
        }


    }
}