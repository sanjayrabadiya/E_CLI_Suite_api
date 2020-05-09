using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Screening
{
    public class ScreeningHistoryRepository : GenericRespository<ScreeningHistory, GscContext>,
        IScreeningHistoryRepository
    {
        public ScreeningHistoryRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public List<ScreeningHistoryDto> GetScreeningHistoryByVolunteerId(int volunteerId, int lastDay)
        {
            var result = All.Where(x => x.ScreeningEntry.Attendance.VolunteerId == volunteerId
                                        && x.ScreeningEntry.EntryType == AttendanceType.Screening
                                        && x.DeletedDate == null
            ).AsQueryable();

            if (lastDay > 0)
                result = result.Where(x =>
                    x.ScreeningEntry.IsFitnessFit == true &&
                    x.ScreeningEntry.ScreeningDate.Date >= DateTime.Now.AddDays(-lastDay).Date);

            var finalResult = result.Select(x => new ScreeningHistoryDto
            {
                Id = x.Id,
                ScreeningEntryId = x.ScreeningEntryId,
                XrayDate = x.XrayDate,
                NextXrayDueDate = x.NextXrayDueDate,
                LastPkSampleDate = x.LastPkSampleDate,
                NextEligibleDate = x.NextEligibleDate,
                Enrolled = x.Enrolled,
                IsCompleted = x.IsCompleted,
                ProjectNumber = x.ProjectNumber,
                Reason = x.Reason,
                ScreeningDate = x.ScreeningEntry.ScreeningDate,
                VolunteerNumber = x.ScreeningEntry.Attendance.Volunteer.VolunteerNo,
                VolunteerName = x.ScreeningEntry.Attendance.Volunteer.FullName,
                ScreeningStatus = x.ScreeningEntry.Status.GetDescription()
            }).OrderByDescending(x => x.Id).ToList();

            return finalResult;
        }
    }
}