﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.SupplyManagement
{
    public class PharmacyStudyProductType : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int ProductTypeId { get; set; }
        public ProductUnitType ProductUnitType { get; set; }
        public Entities.Master.Project Project { get; set; }
        public ProductType ProductType { get; set; }
       
    }
}