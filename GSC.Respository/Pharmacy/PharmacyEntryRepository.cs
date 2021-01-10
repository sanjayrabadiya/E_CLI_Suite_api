using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyEntryRepository : GenericRespository<PharmacyEntry>, IPharmacyEntryRepository
    {
        //private readonly IPharmacyTemplateRepository _pharmacyTemplateRepository;
        private readonly IPharmacyTemplateValueRepository _pharmacyTemplateValueRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IGSCContext _context;
        public PharmacyEntryRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IVolunteerRepository volunteerRepository,
            IPharmacyTemplateValueRepository pharmacyTemplateValueRepository)
            : base(context)
        {
            _volunteerRepository = volunteerRepository;
            _pharmacyTemplateValueRepository = pharmacyTemplateValueRepository;
            _context = context;
            // _pharmacyTemplateRepository = pharmacyTemplateRepository;
        }

        public PharmacyEntryDto GetDetails(int id)
        {


            var pharmacyEntryDto = _context.PharmacyEntry.Where(t => t.Id == id && t.DeletedDate == null)
                .Select(t => new PharmacyEntryDto
                {
                    Id = t.Id,
                    PharmacyNo = t.PharmacyNo,
                    PharmacyDate = t.PharmacyDate,
                    ProjectId = t.ProjectId,
                    FormId = t.FormId
                }).FirstOrDefault();

            //var workflowlevel = _projectWorkflowRepository.GetProjectWorkLevel(PharmacyEntryDto.ProjectId);

            if (pharmacyEntryDto != null)
            {
                pharmacyEntryDto.PharmacyTemplateValues =
                    _pharmacyTemplateValueRepository.GetPharmacyTemplateTree(pharmacyEntryDto.Id);

                return pharmacyEntryDto;
            }

            return null;
        }

        public void SavePharmacy(PharmacyEntry pharmacyEntry)
        {
            pharmacyEntry.Id = 0;
            pharmacyEntry.PharmacyNo = "RC-" + pharmacyEntry.ProjectId;
            pharmacyEntry.PharmacyDate = DateTime.UtcNow;
            pharmacyEntry.Status = IsFormType.IsReceipt;
            Add(pharmacyEntry);
        }

        public List<PharmacyEntryDto> GetpharmacyList()
        {
            var pharmacyEntry = (from pharmacyentry in _context.PharmacyEntry.Where(t => t.DeletedDate == null)
                                 join project in _context.Project.Where(t => t.DeletedDate == null) on pharmacyentry.ProjectId equals
                                     project.Id
                                 join config in _context.PharmacyConfig.Where(t => t.DeletedDate == null) on pharmacyentry.FormId equals
                                     config.FormId
                                 select new PharmacyEntryDto
                                 {
                                     IsDeleted = pharmacyentry.DeletedDate != null,
                                     Id = pharmacyentry.Id,
                                     ProjectId = pharmacyentry.ProjectId,
                                     PharmacyNo = pharmacyentry.PharmacyNo,
                                     PharmacyDate = pharmacyentry.PharmacyDate,
                                     FormId = config.FormId,
                                     ProjectName = project.ProjectName,
                                     FormName = config.FormName,
                                     ProjectCode = project.ProjectCode
                                 }).ToList();

            return pharmacyEntry;
        }

        public PharmacyTemplateValueListDto GetpharmacyTemplateValueList(int? projectId, int domainId,
            int? productTypeId)
        {
            var receiptTemlId = _context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsReceipt)
                .FirstOrDefault();
            var pharmacyVariable = (from variable in _context.Variable
                                    join variabletemplateDetail in _context.VariableTemplateDetail.Where(
                                        vv => vv.DeletedBy == null && vv.VariableTemplateId == receiptTemlId.VariableTemplateId
                                    ).OrderBy(a => a.SeqNo) on variable.Id equals variabletemplateDetail.VariableId
                                    select new VariableDto
                                    {
                                        IsDeleted = variable.DeletedDate != null,
                                        Id = variable.Id,
                                        VariableName = variable.VariableName,
                                        VariableCode = variable.VariableCode
                                    }).ToList();

            var pharmacyEntry = (from pharmacyentry in _context.PharmacyEntry.Where(t =>
                    t.DeletedDate == null &&
                    (projectId == null || projectId == 0 || projectId > 0 && t.ProjectId == projectId) &&
                    (productTypeId == null || productTypeId == 0 ||
                     productTypeId != null && t.ProductTypeId == productTypeId)).OrderByDescending(x => x.Id)
                                 join productType in _context.ProductType.Where(t => t.DeletedBy == null) on pharmacyentry.ProductTypeId
                                     equals productType.Id
                                 select new PharmacyEntryDto
                                 {
                                     IsDeleted = pharmacyentry.DeletedDate != null,
                                     Id = pharmacyentry.Id,
                                     Status = pharmacyentry.Status,
                                     PharmacyDate = pharmacyentry.PharmacyDate,
                                     PharmacyNo = pharmacyentry.PharmacyNo,
                                     ProductTypeName = productType.ProductTypeName
                                 }).ToList();

            foreach (var item in pharmacyEntry) item.PharmacyTemplateValues = GetpharmacyTemplateListByEntry(item.Id);

            var objtemplatevaluelist = new PharmacyTemplateValueListDto();
            objtemplatevaluelist.PharmacyEntry = pharmacyEntry;
            objtemplatevaluelist.VariableList = pharmacyVariable;
            objtemplatevaluelist.PharmacyTemplateValue = null;

            return objtemplatevaluelist;
        }

        public List<PharmacyTemplateValueDto> GetpharmacyTemplateListByEntry(int entryId)
        {
            var receiptTemlId = _context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsReceipt)
                .FirstOrDefault();
            var pharmacyValue = (from p in _context.VariableTemplateDetail.Where(vv => vv.DeletedBy == null)
                                 from ptv in _context.PharmacyTemplateValue.Where(x =>
                                         x.DeletedBy == null && x.VariableId == p.VariableId && x.PharmacyEntryId == entryId)
                                     .DefaultIfEmpty()
                                 where p.VariableTemplateId == receiptTemlId.VariableTemplateId
                                 select new PharmacyTemplateValueDto
                                 {
                                     TempId = ptv.Id,
                                     VariableId = p.VariableId,
                                     Value = ptv.Value,
                                     ValueId = ptv.Value,
                                     PharmacyEntryId = entryId
                                 }).OrderBy(x => x.VariableId).ToList();

            foreach (var item in pharmacyValue)
            {
                var objVariable = _context.Variable.Where(x => x.Id == item.VariableId).FirstOrDefault();
                if (objVariable != null)
                {
                    if (objVariable.CollectionSource == CollectionSources.ComboBox ||
                        objVariable.CollectionSource == CollectionSources.RadioButton ||
                        objVariable.CollectionSource == CollectionSources.NumericScale)
                    {
                        var varvalue = _context.VariableValue.Where(x => x.Id == Convert.ToInt32(item.Value))
                            .FirstOrDefault();
                        item.ValueId = item.Value;
                        item.Value = varvalue != null ? varvalue.ValueName : item.Value;
                    }
                    else if (objVariable.CollectionSource == CollectionSources.MultiCheckBox)
                    {
                        var varvalue =
                            from ptv in _context.PharmacyTemplateValueChild.Where(x =>
                                x.PharmacyTemplateValueId == item.TempId)
                            from vv in _context.VariableValue.Where(x => x.Id == ptv.VariableValueId)
                            from v in _context.Variable.Where(x => x.Id == vv.VariableId)
                            select new
                            {
                                vv.ValueName
                            };
                        item.ValueId = item.Value;
                        item.Value = string.Join(",", varvalue.Select(x => x.ValueName));
                    }
                }
            }

            return pharmacyValue;
        }

        public VariableTemplate GetTemplate(int id)
        {
            var receiptTemlId = _context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsReceipt)
                .FirstOrDefault();
            var template = _context.VariableTemplate.Where(t => t.Id == receiptTemlId.VariableTemplateId)
                .Include(d => d.VariableTemplateDetails)
                .Include(t => t.Domain)
                .Include(t => t.Notes)
                .FirstOrDefault();

            if (template != null)
            {
                template.VariableTemplateDetails = template.VariableTemplateDetails.Where(t => t.DeletedDate == null)
                    .OrderBy(t => t.SeqNo).ToList();
                template.VariableTemplateDetails.ForEach(detail =>
                {
                    detail.Variable = _context.Variable.Where(t => t.Id == detail.VariableId)
                        .Include(variable => variable.Values)
                        .Include(t => t.Unit)
                        .FirstOrDefault();

                    if (detail.Variable != null)
                    {
                        detail.Variable.Values = detail.Variable.Values.Where(t => t.DeletedDate == null).ToList();
                        detail.VariableCategoryName = detail.Variable.VariableCategoryId == null
                            ? ""
                            : _context.VariableCategory.FirstOrDefault(t => t.Id == detail.Variable.VariableCategoryId)
                                ?.CategoryName;
                    }
                });
                //obj.VariableTemplate = template;
                return template;
            }

            return null;
        }

        public IList<DropDownDto> AutoCompleteSearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return new List<DropDownDto>();
            searchText = searchText.Trim();
            var volunterIds = _volunteerRepository.AutoCompleteSearch(searchText, true);
            if (volunterIds == null || volunterIds.Count == 0) return new List<DropDownDto>();

            var items = new List<DropDownDto>();

            return items;
        }
    }
}