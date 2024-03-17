using System;
using System.Collections.Generic;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Respository.Pharmacy;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Pharmacy
{
    [Route("api/[controller]")]
    public class PharmacyVerificationEntryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPharmacyEntryRepository _pharmacyEntryRepository;
        private readonly IPharmacyVerificationEntryRepository _pharmacyVerificationEntryRepository;
        private readonly IUnitOfWork _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableValueRepository _variableValueRepository;

        public PharmacyVerificationEntryController(
            IPharmacyVerificationEntryRepository pharmacyVerificationEntryRepository,
            IPharmacyEntryRepository pharmacyEntryRepository,
            IUnitOfWork uow, IMapper mapper,
            IVariableRepository variableRepository,
            IVariableValueRepository variableValueRepository
        )
        {
            _pharmacyVerificationEntryRepository = pharmacyVerificationEntryRepository;
            _pharmacyEntryRepository = pharmacyEntryRepository;
            _variableRepository = variableRepository;
            _variableValueRepository = variableValueRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var pharmacyVerificationEntryDto = _pharmacyVerificationEntryRepository.GetDetails(id);
            return Ok(pharmacyVerificationEntryDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PharmacyVerificationEntryDto pharmacyVerificationEntryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pharmacyVerificationEntryDto.Id = 0;
            var pharmacyVerificationEntry = _mapper.Map<PharmacyVerificationEntry>(pharmacyVerificationEntryDto);

            _pharmacyVerificationEntryRepository.SavePharmacyVerificaction(pharmacyVerificationEntry);

            var pharmacyEntry = _pharmacyEntryRepository.Find(pharmacyVerificationEntryDto.PharmacyEntryId);
            pharmacyEntry.Status = IsFormType.IsVerification;
            _pharmacyEntryRepository.Update(pharmacyEntry);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Pharmacy Verification Entry failed on save.");
                return BadRequest(ModelState);
            }
            var pharmacyverificationvaluelist =
                GetpharmacyVerificationTemplateValueList(pharmacyVerificationEntry.ProjectId, 0);

            return Ok(pharmacyverificationvaluelist);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PharmacyVerificationEntryDto pharmacyVerificationEntryDto)
        {
            if (pharmacyVerificationEntryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var pharmacyVerificationEntry = _mapper.Map<PharmacyVerificationEntry>(pharmacyVerificationEntryDto);
            _pharmacyVerificationEntryRepository.Update(pharmacyVerificationEntry);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Pharmacy Verification Entry failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(pharmacyVerificationEntry.Id);
        }


        [HttpGet]
        [Route("GetpharmacyVerificationList")]
        public IActionResult GetpharmacyVerificationList()
        {
            var result = _pharmacyVerificationEntryRepository.GetpharmacyVerificationList();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetpharmacyVerificationTemplateValueList/{ProjectId}/{DomainId}")]
        public IActionResult GetpharmacyVerificationTemplateValueList(int projectId, int domainId)
        {
            var result =
                _pharmacyVerificationEntryRepository.GetpharmacyVerificationTemplateValueList(projectId, domainId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetpharmacyVerificationTemplateListByEntry/{EntryId}")]
        public IActionResult GetpharmacyVerificationTemplateListByEntry(int entryId)
        {
            var result = _pharmacyVerificationEntryRepository.GetpharmacyVerificationTemplateListByEntry(entryId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pharmacyVerificationEntryRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyVerificationEntryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pharmacyVerificationEntryRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyVerificationEntryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("PreviewVerification/{entryId}/{receiptTemplateId}")]
        public IActionResult PreviewVerification(int entryId, int receiptTemplateId)
        {
            var obj = new ProjectDesignTemplateDto();
            var pharmacyverificationtemplate = new PharmacyVerificationEntryDto();
            obj.Variables = new List<ProjectDesignVariableDto>();
            var variableTemplate = _pharmacyVerificationEntryRepository.GetTemplate(receiptTemplateId);
            if (entryId > 0)
            {
                var pharmacyVerificationEntry = _pharmacyVerificationEntryRepository.Find(entryId);

                if (pharmacyVerificationEntry != null)
                {
                    obj.ParentId = pharmacyVerificationEntry.ProjectId;
                    obj.TemplateCode = pharmacyVerificationEntry.PharmacyVerificationNo;
                }

                var pharmacyVerificationTemplateValuelist =
                    _pharmacyVerificationEntryRepository.GetpharmacyVerificationTemplateListByEntry(entryId);
                pharmacyverificationtemplate.PharmacyVerificationTemplateValuesList =
                    pharmacyVerificationTemplateValuelist;
            }

            var variableTemplateDto = _mapper.Map<VariableTemplateDto>(variableTemplate);
            pharmacyverificationtemplate.VariableTemplate = variableTemplate;

            obj.VariableTemplate = variableTemplateDto;

            foreach (var v in obj.VariableTemplate.VariableTemplateDetails)
            {
                var variableDetail = _variableRepository.Find(v.VariableId);
                var variableValueDetail = _variableValueRepository.FindByInclude(x => x.VariableId == v.VariableId);
                var objDto = new ProjectDesignVariableDto();
                objDto.VariableId = v.VariableId;
                objDto.CollectionSource = variableDetail.CollectionSource;
                objDto.VariableName = variableDetail.VariableName;
                objDto.VariableCode = variableDetail.VariableCode;
                objDto.VariableAlias = variableDetail.VariableAlias;
                objDto.DomainId = variableDetail.DomainId;
                objDto.CDISCValue = variableDetail.CDISCValue;
                objDto.CDISCSubValue = variableDetail.CDISCSubValue;
                objDto.CollectionAnnotation = variableDetail.CollectionAnnotation;
                objDto.VariableCategoryId = variableDetail.VariableCategoryId;
                objDto.AnnotationTypeId = variableDetail.AnnotationTypeId;
                objDto.Annotation = variableDetail.Annotation;
                objDto.DataType = variableDetail.DataType;
                objDto.ValidationType = variableDetail.ValidationType;
                objDto.DefaultValue = variableDetail.DefaultValue;
                objDto.LowRangeValue = variableDetail.LowRangeValue;
                objDto.HighRangeValue = variableDetail.HighRangeValue;
                objDto.UnitId = variableDetail.UnitId;
                objDto.UnitAnnotation = variableDetail.UnitAnnotation;
                objDto.PrintType = variableDetail.PrintType;
                objDto.IsDocument = variableDetail.IsDocument;
                var variableValueList = new List<ProjectDesignVariableValueDto>();
                foreach (var item in variableValueDetail)
                {
                    var objValue = new ProjectDesignVariableValueDto();
                    objValue.ValueName = item.ValueName;
                    objValue.Id = item.Id;
                    objValue.ValueCode = item.ValueCode;
                    variableValueList.Add(objValue);
                }

                objDto.Values = variableValueList;
                obj.Variables.Add(objDto);
            }

            return Ok(obj);
        }
    }
}