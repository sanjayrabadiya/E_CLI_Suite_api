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
using GSC.Respository.Master;
using GSC.Respository.Pharmacy;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Pharmacy
{
    [Route("api/[controller]")]
    public class PharmacyEntryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPharmacyEntryRepository _pharmacyEntryRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableValueRepository _variableValueRepository;

        public PharmacyEntryController(IPharmacyEntryRepository pharmacyEntryRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IVariableRepository variableRepository,
            IVariableValueRepository variableValueRepository)
        {
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
            var pharmacyEntryDto = _pharmacyEntryRepository.GetDetails(id);
            return Ok(pharmacyEntryDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PharmacyEntryDto pharmacyEntryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pharmacyEntryDto.Id = 0;
            var pharmacyEntry = _mapper.Map<PharmacyEntry>(pharmacyEntryDto);

            _pharmacyEntryRepository.SavePharmacy(pharmacyEntry);

            if (_uow.Save() <= 0) throw new Exception("Creating Pharmacy Entry failed on save.");

            var pharmacyvaluelist =
                GetpharmacyTemplateValueList(pharmacyEntry.ProjectId, 0, pharmacyEntryDto.ProductTypeId);

            return Ok(pharmacyvaluelist);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PharmacyEntryDto pharmacyEntryDto)
        {
            if (pharmacyEntryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pharmacyEntry = _mapper.Map<PharmacyEntry>(pharmacyEntryDto);

            _pharmacyEntryRepository.Update(pharmacyEntry);

            if (_uow.Save() <= 0) throw new Exception("Updating Pharmacy Entry failed on save.");

            return Ok(pharmacyEntry.Id);
        }


        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _pharmacyEntryRepository.AutoCompleteSearch(searchText);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetpharmacyList")]
        public IActionResult GetpharmacyList()
        {
            var result = _pharmacyEntryRepository.GetpharmacyList();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetpharmacyTemplateValueList/{ProjectId}/{DomainId}/{ProductTypeId}")]
        public IActionResult GetpharmacyTemplateValueList(int? projectId, int domainId, int? productTypeId)
        {
            var result = _pharmacyEntryRepository.GetpharmacyTemplateValueList(projectId, domainId, productTypeId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetpharmacyTemplateListByEntry/{EntryId}")]
        public IActionResult GetpharmacyTemplateListByEntry(int entryId)
        {
            var result = _pharmacyEntryRepository.GetpharmacyTemplateListByEntry(entryId);
            return Ok(result);
        }

        //[HttpGet]
        //[Route("GetAuditHistory/{id}")]   
        //public IActionResult GetAuditHistory(int id)
        //{
        //    var auditHistory = _pharmacyEntryRepository.GetAuditHistory(id);

        //    return Ok(auditHistory);
        //}

        //[HttpGet("Summary/{id}")]
        //public IActionResult Summary(int id)
        //{
        //    if (id <= 0)
        //    {
        //        return BadRequest();
        //    }

        //    var pharmacySummaryDto = _pharmacyEntryRepository.GetSummary(id);

        //    return Ok(pharmacySummaryDto);
        //}

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pharmacyEntryRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyEntryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pharmacyEntryRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyEntryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("Preview/{entryId}/{receiptTemplateId}")]
        public IActionResult Preview(int entryId, int receiptTemplateId)
        {
            var obj = new ProjectDesignTemplateDto();
            var pharmacytemplate = new PharmacyTemplateDto();
            obj.Variables = new List<ProjectDesignVariableDto>();
            var variableTemplate = _pharmacyEntryRepository.GetTemplate(receiptTemplateId);
            var pharmacyEntry = _pharmacyEntryRepository.Find(entryId);
            if (pharmacyEntry != null)
            {
                obj.ParentId = pharmacyEntry.ProjectId;
                obj.TemplateCode = pharmacyEntry.PharmacyNo;
                obj.ProductTypeId = pharmacyEntry.ProductTypeId;
            }

            var pharmacyTemplateValuelist = _pharmacyEntryRepository.GetpharmacyTemplateListByEntry(entryId);
            var variableTemplateDto = _mapper.Map<VariableTemplateDto>(variableTemplate);
            pharmacytemplate.VariableTemplate = variableTemplate;
            pharmacytemplate.PharmacyTemplateValue = pharmacyTemplateValuelist;

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
                var detailList = pharmacytemplate.PharmacyTemplateValue.Find(x =>
                    x.VariableId == v.VariableId && x.PharmacyEntryId == entryId);
                if (detailList != null)
                {
                    objDto.ScreeningValue = detailList.ValueId;
                    objDto.Id = detailList.TempId == null ? 0 : (int) detailList.TempId;
                }

                obj.Variables.Add(objDto);
            }

            return Ok(obj);
        }
    }
}