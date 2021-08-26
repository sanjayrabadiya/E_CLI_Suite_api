using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class VariableTemplateRepository : GenericRespository<VariableTemplate>,
        IVariableTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IVariableTemplateDetailRepository _variableTemplateDetailRepository;
        private readonly IVariableValueRepository _variableValueRepository;
        public VariableTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVariableValueRepository variableValueRepository,
            IVariableTemplateDetailRepository variableTemplateDetailRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _variableTemplateDetailRepository = variableTemplateDetailRepository;
            _variableValueRepository = variableValueRepository;
        }

        public List<DropDownDto> GetVariableTemplateDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetVariableTemplateNonCRFDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                    && x.ActivityMode == Helper.ActivityMode.Generic)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetVariableTemplateByDomainId(int domainId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null &&
                    x.DomainId == domainId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
                .OrderBy(o => o.Value).ToList();
        }

        public VariableTemplate GetTemplate(int id)
        {
            var template = _context.VariableTemplate.AsNoTracking().Where(t => t.Id == id)
                .Include(d => d.VariableTemplateDetails)
                .Include(t => t.Domain)
                .Include(t => t.Notes)
                .FirstOrDefault();

            template.Notes = template.Notes.Where(t => t.DeletedDate == null && t.IsPreview).ToList();
            template.VariableTemplateDetails = template.VariableTemplateDetails.Where(t => t.DeletedDate == null)
                .OrderBy(t => t.SeqNo).ToList();
            template.VariableTemplateDetails.ForEach(detail =>
            {
                detail.Variable = _context.Variable.Where(t => t.Id == detail.VariableId)
                    .Include(variable => variable.Values)
                    // .Include(r => r.Remarks)
                    .Include(t => t.Unit)
                    .FirstOrDefault();

                detail.Variable.Values = detail.Variable.Values.Where(t => t.DeletedDate == null).OrderBy(x => x.Id).ToList();
                //  detail.Variable.Remarks = detail.Variable.Remarks.Where(t => t.DeletedDate == null).ToList();
                detail.VariableCategoryName = detail.Variable.VariableCategoryId == null
                    ? ""
                    : _context.VariableCategory.Where(t => t.Id == detail.Variable.VariableCategoryId).FirstOrDefault()?.CategoryName;
            });

            return template;
        }

        public string Duplicate(VariableTemplate objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TemplateCode == objSave.TemplateCode.Trim() && x.DeletedDate == null))
                return "Duplicate Template code : " + objSave.TemplateCode;

            //if (!string.IsNullOrEmpty(objSave.Activity.ActivityName))
            //    if (All.Any(x => x.Id != objSave.Id && x.DeletedDate == null && !string.IsNullOrEmpty(x.Activity.ActivityName) && x.Activity.ActivityName == objSave.Activity.ActivityName.Trim()))
            //        return "Duplicate Activity name : " + objSave.Activity.ActivityName;

            if (All.Any(x => x.Id != objSave.Id && x.TemplateName == objSave.TemplateName.Trim() && x.DeletedDate == null))
                return "Duplicate Template name : " + objSave.TemplateName;

            return "";
        }

        public List<Variable> GetVariableNotAddedinTemplate(int variableTemplateId)
        {
            //var variables = All.Where(t => t.DeletedDate == null && t.Id == variableTemplateId)
            //    .Include(x => x.VariableTemplateDetails)
            //    .ThenInclude(x => x.Variable).ToList();



            //template.VariableTemplateDetails.ForEach(detail =>
            //{
            //    detail.Variable = _context.Variable.Where(t => t.Id == detail.VariableId)
            //        .Include(variable => variable.Values)
            //        .Include(r => r.Remarks)
            //        .Include(t => t.Unit)
            //        .FirstOrDefault();

            //    detail.Variable.Values = detail.Variable.Values.Where(t => t.DeletedDate == null).ToList();
            //    detail.Variable.Remarks = detail.Variable.Remarks.Where(t => t.DeletedDate == null).ToList();
            //    detail.VariableCategoryName = detail.Variable.VariableCategoryId == null
            //        ? ""
            //        : _context.VariableCategory.Where(t => t.Id == detail.Variable.VariableCategoryId).FirstOrDefault()?.CategoryName;
            //});

            return new List<Variable>();
        }

        public List<DropDownDto> GetVariableTemplateByCRFByDomainId(bool isNonCRF, int domainId)
        {
            return All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null &&
                x.DomainId == domainId && (isNonCRF ? x.ActivityMode == Helper.ActivityMode.Generic : x.ActivityMode == Helper.ActivityMode.SubjectSpecific))
            .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
            .OrderBy(o => o.Value).ToList();
        }

        public void AddRequieredTemplate(Variable variable)
        {
            var template = All.Where(x => x.DomainId == variable.DomainId).ToList();
            foreach (var item in template)
            {
                var templateDetails = _variableTemplateDetailRepository.FindByInclude(x => x.VariableTemplateId == item.Id).ToList();
                VariableTemplateDetail VariableTemplateDetail = new VariableTemplateDetail();
                VariableTemplateDetail.VariableTemplateId = item.Id;
                VariableTemplateDetail.VariableId = variable.Id;
                VariableTemplateDetail.SeqNo = templateDetails.LastOrDefault().SeqNo + 1;
                _variableTemplateDetailRepository.Add(VariableTemplateDetail);
                _context.Save();
            }
        }

        public DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(int id)
        {
            var result = All.Where(t => t.Id == id)
                .Include(d => d.VariableTemplateDetails).
                Select(r => new DesignVerificationApprovalTemplateDto
                {
                    Id = r.Id,
                    VariableTemplateId = r.Id,
                    VariableTemplateName = r.TemplateName,
                    ActivityName = r.Activity.ActivityName,
                    Notes = r.Notes.Where(c => c.DeletedDate == null).Select(a => a.Note).ToList(),
                    VariableTemplateDetails = r.VariableTemplateDetails
                }
            ).FirstOrDefault();

            result.VariableTemplateDetails = result.VariableTemplateDetails.Where(t => t.DeletedDate == null).OrderBy(t => t.SeqNo).ToList();

            if (result != null)
            {
                List<VerificationApprovalVariableDto> VariablesDto = new List<VerificationApprovalVariableDto>();
                result.VariableTemplateDetails.ForEach(detail =>
                {
                    var variables = _context.Variable.Where(t => t.Id == detail.VariableId && t.DeletedDate == null)
                    .Include(variable => variable.Values)
                    .Select(x => new VerificationApprovalVariableDto
                    {
                        VariableTemplateId = id,
                        VariableId = x.Id,
                        Id = x.Id,
                        VariableName = x.VariableName,
                        VariableCode = x.VariableCode,
                        CollectionSource = x.CollectionSource,
                        ValidationType = x.ValidationType,
                        DataType = x.DataType,
                        Length = x.Length,
                        DefaultValue = string.IsNullOrEmpty(x.DefaultValue) && x.CollectionSource == CollectionSources.HorizontalScale ? "1" : x.DefaultValue,
                        LargeStep = x.LargeStep,
                        LowRangeValue = x.LowRangeValue,
                        HighRangeValue = x.HighRangeValue,
                        PrintType = x.PrintType,
                        UnitName = x.Unit.UnitName,
                        VariableCategoryName = x.VariableCategory.CategoryName ?? "",
                        SystemType = x.SystemType,
                        IsNa = x.IsNa,
                        DateValidate = x.DateValidate,
                        Alignment = x.Alignment ?? Alignment.Right,
                        Note = detail.Note,
                        ValidationMessage = x.ValidationType == ValidationType.Required ? "This field is required" : ""
                    }).FirstOrDefault();

                    variables.Values = _context.VariableValue.Where(c => c.VariableId == detail.VariableId).Select(c => new VerificationApprovalVariableValueDto
                    {
                        Id = c.Id,
                        VariableId = c.VariableId,
                        ValueName = c.ValueName,
                        SeqNo = c.SeqNo,
                        Label = c.Label,
                    }).OrderBy(c => c.SeqNo).ToList();

                    VariablesDto.Add(variables);
                });
                result.Variables = VariablesDto;
            }

            return result;
        }

        public List<DropDownDto> GetVariableTemplateByModuleId(int moduleId)
        {
            return All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null &&
                 (int)x.ModuleId == moduleId)
            .Select(c => new DropDownDto { Id = c.Id, Value = c.TemplateName, Code = c.TemplateCode })
            .OrderBy(o => o.Value).ToList();
        }
    }
}