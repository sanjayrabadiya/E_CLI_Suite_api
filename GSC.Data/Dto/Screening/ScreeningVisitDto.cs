using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningVisitDto
    {
        public int ScreeningVisitId { get; set; }
        private DateTime _visitOpenDate { get; set; }

        public DateTime VisitOpenDate
        {
            get => _visitOpenDate.UtcDate();
            set => _visitOpenDate = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public string ScreeningVisitName { get; set; }
    }
}
