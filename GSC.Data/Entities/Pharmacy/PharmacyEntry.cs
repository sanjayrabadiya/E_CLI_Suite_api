using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Pharmacy
{
    public class PharmacyEntry : BaseEntity
    {
        public string PharmacyNo { get; set; }
        private DateTime _pharmacyDate { get; set; }

        public DateTime PharmacyDate
        {
            get => _pharmacyDate.UtcDate();
            set => _pharmacyDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public int ProjectId { get; set; }
        public IsFormType? Status { get; set; }

        public int? ProductTypeId { get; set; }

        public int FormId { get; set; }
        public int? CompanyId { get; set; }
        public ICollection<PharmacyTemplateValue> PharmacyTemplateValues { get; set; }
    }
}