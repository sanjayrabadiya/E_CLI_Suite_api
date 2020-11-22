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
using GSC.Shared;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Pharmacy
{
    public class PharmacyVerificationEntryRepository : GenericRespository<PharmacyVerificationEntry, GscContext>,
        IPharmacyVerificationEntryRepository
    {
        //private readonly IAttendanceRepository _attendanceRepository;
        //private readonly IProjectWorkflowRepository _projectWorkflowRepository;        
        private readonly IPharmacyVerificationTemplateValueRepository _pharmacyVerificationTemplateValueRepository;

        //private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        //private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IVolunteerRepository _volunteerRepository;

        public PharmacyVerificationEntryRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            //IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IVolunteerRepository volunteerRepository,
            IPharmacyVerificationTemplateValueRepository pharmacyVerificationTemplateValueRepository
        // IProjectDesignVariableRepository projectDesignVariableRepository
        )
            : base(uow, jwtTokenAccesser)
        {
            //_projectDesignVariableRepository = projectDesignVariableRepository;
            //_projectDesignTemplateRepository = projectDesignTemplateRepository;
            _volunteerRepository = volunteerRepository;
            _pharmacyVerificationTemplateValueRepository = pharmacyVerificationTemplateValueRepository;
        }

        public PharmacyVerificationEntryDto GetDetails(int id)
        {
            //var PharmacyTemplateValue = _pharmacyTemplateValueRepository.
            //    FindBy(x => x.DeletedDate == null && x.PharmacyEntryId == id).ToList();

            var pharmacyVerificationEntryDto = Context.PharmacyVerificationEntry
                .Where(t => t.Id == id && t.DeletedDate == null)
                .Select(t => new PharmacyVerificationEntryDto
                {
                    Id = t.Id,
                    PharmacyEntryId = t.PharmacyEntryId,
                    PharmacyVerificationNo = t.PharmacyVerificationNo,
                    PharmacyVerificationDate = t.PharmacyVerificationDate,
                    ProjectId = t.ProjectId,
                    FormId = t.FormId
                }).FirstOrDefault();

            if (pharmacyVerificationEntryDto != null)
            {
                pharmacyVerificationEntryDto.PharmacyVerificationTemplateValues =
                    _pharmacyVerificationTemplateValueRepository.GetPharmacyVerificationTemplateTree(
                        pharmacyVerificationEntryDto.Id);

                return pharmacyVerificationEntryDto;
            }

            return null;
        }

        public void SavePharmacyVerificaction(PharmacyVerificationEntry pharmacyVerificationEntry)
        {
            pharmacyVerificationEntry.Id = 0;
            pharmacyVerificationEntry.PharmacyVerificationNo = "RC-" + pharmacyVerificationEntry.ProjectId;
            pharmacyVerificationEntry.PharmacyVerificationDate = DateTime.UtcNow;
            Add(pharmacyVerificationEntry);
        }

        public List<PharmacyVerificationEntryDto> GetpharmacyVerificationList()
        {
            var pharmacyVerificationEntry =
                (from pharmacyverificationentry in Context.PharmacyVerificationEntry.Where(t => t.DeletedDate == null)
                 join project in Context.Project.Where(t => t.DeletedDate == null) on pharmacyverificationentry
                     .ProjectId equals project.Id
                 join config in Context.PharmacyConfig.Where(t => t.DeletedDate == null) on pharmacyverificationentry
                     .FormId equals config.FormId
                 select new PharmacyVerificationEntryDto
                 {
                     IsDeleted = pharmacyverificationentry.DeletedDate != null,
                     Id = pharmacyverificationentry.Id,
                     ProjectId = pharmacyverificationentry.ProjectId,
                     PharmacyVerificationNo = pharmacyverificationentry.PharmacyVerificationNo,
                     PharmacyEntryId = pharmacyverificationentry.PharmacyEntryId,
                     PharmacyVerificationDate = pharmacyverificationentry.PharmacyVerificationDate,
                     FormId = config.FormId,
                     ProjectName = project.ProjectName,
                     FormName = config.FormName,
                     ProjectCode = project.ProjectCode
                 }).ToList();

            return pharmacyVerificationEntry;
        }

        public PharmacyVerificationTemplateValueListDto GetpharmacyVerificationTemplateValueList(int projectId,
            int domainId)
        {
            var receiptTemlId = Context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsVerification)
                .FirstOrDefault();
            var pharmacyVerificationVariable = (from variable in Context.Variable
                                                join variabletemplateDetail in Context.VariableTemplateDetail.Where(
                                                    vv => vv.DeletedBy == null && vv.VariableTemplateId == receiptTemlId.VariableTemplateId
                                                ).OrderBy(a => a.SeqNo) on variable.Id equals variabletemplateDetail.VariableId
                                                select new VariableDto
                                                {
                                                    IsDeleted = variable.DeletedDate != null,
                                                    Id = variable.Id,
                                                    VariableName = variable.VariableName,
                                                    VariableCode = variable.VariableCode
                                                }).ToList();

            var pharmacyVerificationEntry = (from pharmacyverificationentry in Context.PharmacyVerificationEntry
                    .Where(t => t.DeletedDate == null && t.ProjectId == projectId).OrderByDescending(x => x.Id)
                                                 //join productType in Context.ProductType.Where(t=>t.DeletedBy == null) on pharmacyverificationentry.ProductTypeId equals productType.Id
                                             select new PharmacyVerificationEntryDto
                                             {
                                                 IsDeleted = pharmacyverificationentry.DeletedDate != null,
                                                 Id = pharmacyverificationentry.Id,
                                                 PharmacyEntryId = pharmacyverificationentry.PharmacyEntryId,
                                                 PharmacyVerificationDate = pharmacyverificationentry.PharmacyVerificationDate,
                                                 PharmacyVerificationNo = pharmacyverificationentry.PharmacyVerificationNo
                                                 //ProductTypeName = productType.ProductTypeName,
                                             }).ToList();

            foreach (var item in pharmacyVerificationEntry)
                item.PharmacyVerificationTemplateValues = GetpharmacyVerificationTemplateListByEntry(item.Id);

            var objtemplatevaluelist = new PharmacyVerificationTemplateValueListDto();
            objtemplatevaluelist.PharmacyVerificationEntry = pharmacyVerificationEntry;
            objtemplatevaluelist.VariableList = pharmacyVerificationVariable;
            objtemplatevaluelist.PharmacyVerificationTemplateValue = null;

            return objtemplatevaluelist;
        }

        public List<PharmacyVerificationTemplateValueDto> GetpharmacyVerificationTemplateListByEntry(int entryId)
        {
            var receiptTemlId = Context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsVerification)
                .FirstOrDefault();
            var pharmacyVerificationValue = (from p in Context.VariableTemplateDetail.Where(vv => vv.DeletedBy == null)
                                             from ptv in Context.PharmacyVerificationTemplateValue.Where(x =>
                                                     x.DeletedBy == null && x.VariableId == p.VariableId && x.PharmacyVerificationEntryId == entryId)
                                                 .DefaultIfEmpty()
                                             where p.VariableTemplateId == receiptTemlId.VariableTemplateId
                                             select new PharmacyVerificationTemplateValueDto
                                             {
                                                 TempId = ptv.Id,
                                                 VariableId = p.VariableId,
                                                 Value = ptv.Value,
                                                 ValueId = ptv.Value,
                                                 PharmacyVerificationEntryId = entryId
                                             }).OrderBy(x => x.VariableId).ToList();


            foreach (var item in pharmacyVerificationValue)
            {
                var objVariable = Context.Variable.Where(x => x.Id == item.VariableId).FirstOrDefault();
                if (objVariable != null)
                {
                    if (objVariable.CollectionSource == CollectionSources.ComboBox ||
                        objVariable.CollectionSource == CollectionSources.RadioButton)
                    {
                        var varvalue = Context.VariableValue.Where(x => x.Id == Convert.ToInt32(item.Value))
                            .FirstOrDefault();
                        item.ValueId = item.Value;
                        item.Value = varvalue != null ? varvalue.ValueName : item.Value;
                    }
                    else if (objVariable.CollectionSource == CollectionSources.MultiCheckBox)
                    {
                        var varvalue =
                            from ptv in Context.PharmacyVerificationTemplateValueChild.Where(x =>
                                x.PharmacyVerificationTemplateValueId == item.TempId)
                            from vv in Context.VariableValue.Where(x => x.Id == ptv.VariableValueId)
                            from v in Context.Variable.Where(x => x.Id == vv.VariableId)
                            select new
                            {
                                vv.ValueName
                            };
                        item.ValueId = item.Value;
                        item.Value = string.Join(",", varvalue.Select(x => x.ValueName));
                    }
                }
            }

            return pharmacyVerificationValue;
        }

        public VariableTemplate GetTemplate(int id)
        {
            var receiptTemlId = Context.PharmacyConfig.Where(x => x.FormId == (int)IsFormType.IsVerification)
                .FirstOrDefault();
            var template = Context.VariableTemplate.Where(t => t.Id == receiptTemlId.VariableTemplateId)
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
                    detail.Variable = Context.Variable.Where(t => t.Id == detail.VariableId)
                        .Include(variable => variable.Values)
                        .Include(t => t.Unit)
                        .FirstOrDefault();

                    if (detail.Variable != null)
                    {
                        detail.Variable.Values = detail.Variable.Values.Where(t => t.DeletedDate == null).ToList();
                        detail.VariableCategoryName = detail.Variable.VariableCategoryId == null
                            ? ""
                            : Context.VariableCategory.FirstOrDefault(t => t.Id == detail.Variable.VariableCategoryId)
                                ?.CategoryName;
                    }
                });

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
            //var query = All.Where(t => t.DeletedDate == null)
            //    .Include(t => t.Attendance).AsQueryable();

            //query = query.Where(x => volunterIds.Any(a => a.Id == x.Attendance.VolunteerId));

            //var items = query.Select(t => new DropDownDto
            //{
            //    Id = t.Attendance.VolunteerId,
            //    Value = t.Attendance.Volunteer.VolunteerNo + " " + t.Attendance.Volunteer.FullName
            //}).ToList();

            return items;
        }
    }
}