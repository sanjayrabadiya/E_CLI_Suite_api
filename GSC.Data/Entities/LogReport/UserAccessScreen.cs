using System;
using GSC.Common.Base;
using GSC.Shared.Extension;

namespace GSC.Data.Entities.LogReport
{
    public class UserAccessScreen : BaseEntity
    {
        private DateTime _ActivityDate;
        public int UserId { get; set; }

        public int? AppScreenId { get; set; }

        public string ScreenName { get; set; }

        public DateTime ActivityDate
        {
            get => _ActivityDate.UtcDate();
            set => _ActivityDate = value.UtcDate();
        }
    }
}