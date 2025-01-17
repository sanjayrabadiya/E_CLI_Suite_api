﻿using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class LoginPreference : BaseEntity, ICommonAduit
    {
        public short MinPasswordLength { get; set; }
        public bool RequiredSpecialChar { get; set; }
        public bool RequiredAlphaNumber { get; set; }
        public bool RequiredCapital { get; set; }
        public int ExpiredDay { get; set; }
        public int MaxLoginAttempt { get; set; }
        public int CompanyId { get; set; }
    }
}