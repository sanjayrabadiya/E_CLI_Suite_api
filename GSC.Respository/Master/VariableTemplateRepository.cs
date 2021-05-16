﻿using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
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
        public VariableTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVariableTemplateDetailRepository variableTemplateDetailRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _variableTemplateDetailRepository = variableTemplateDetailRepository;
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
            var template = _context.VariableTemplate.Where(t => t.Id == id)
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

            if (!string.IsNullOrEmpty(objSave.ActivityName))
                if (All.Any(x => x.Id != objSave.Id && x.DeletedDate == null && !string.IsNullOrEmpty(x.ActivityName) && x.ActivityName == objSave.ActivityName.Trim()))
                    return "Duplicate Activity name : " + objSave.ActivityName;

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
    }
}