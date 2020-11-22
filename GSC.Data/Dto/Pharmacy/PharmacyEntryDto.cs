using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared;

namespace GSC.Data.Dto.Pharmacy
{
    public class PharmacyEntryDto : BaseDto
    {
        public string PharmacyNo { get; set; }
        private DateTime _pharmacyDate { get; set; }

        public DateTime PharmacyDate
        {
            get => _pharmacyDate.UtcDate();
            set => _pharmacyDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "FormName is required.")]
        public int FormId { get; set; }

        public ICollection<PharmacyTemplateValueDto> PharmacyTemplateValues { get; set; }
        public string ProjectName { get; set; }
        public string FormName { get; set; }
        public string ProjectCode { get; set; }
        public string ProductTypeName { get; set; }
        public int? ProductTypeId { get; set; }
        public IsFormType? Status { get; set; }
    }

    public class PharmacyEntryVariableEnumDto : BaseDto
    {
        public int VariableId { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
        public object ExtraData { get; set; }
    }

    public class PharmacyAuditDto
    {
        private DateTime? _createdDate;
        public string Visit { get; set; }
        public string Template { get; set; }
        public string Variable { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Reason { get; set; }
        public string Note { get; set; }
        public string User { get; set; }
        public string Role { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }

        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}