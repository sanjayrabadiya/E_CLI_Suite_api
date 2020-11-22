using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Shared;

namespace GSC.Data.Entities.Pharmacy
{
    public class PharmacyVerificationEntry : BaseEntity
    {
        public int PharmacyEntryId { get; set; }
        public string PharmacyVerificationNo { get; set; }
        private DateTime _pharmacyVerificationDate { get; set; }

        public DateTime PharmacyVerificationDate
        {
            get => _pharmacyVerificationDate.UtcDate();
            set => _pharmacyVerificationDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public int ProjectId { get; set; }
        public int FormId { get; set; }
        public int? CompanyId { get; set; }
        public ICollection<PharmacyVerificationTemplateValue> PharmacyVerificationTemplateValues { get; set; }
    }
}