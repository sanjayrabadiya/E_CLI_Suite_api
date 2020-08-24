using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.UserMgt
{
    public class ReportFavouriteScreen : BaseEntity
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int UserId { get; set; }
    }
}
