using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitRepository : IGenericRepository<ScreeningVisit>
    {
        void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId, int projectDesignVisitId, DateTime visitDate, double? StudyVersion);
        void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto);
        void OpenVisit(ScreeningVisitDto screeningVisitDto);
        void VisitRepeat(ScreeningVisitDto screeningVisitDto);
        void PatientStatus(int screeningEntryId);
        ScreeningVisitStatus? AutomaticStatusUpdate(int screeningTemplateId);
        bool IsPatientScreeningFailure(int screeningVisitId);
        List<ScreeningVisitTree> GetVisitTree(int screeningEntryId);
        void FindOpenVisitVarible(int projectDesignVisitId, int screeningVisitId, DateTime visitDate, int screeningEntryId);

        string CheckScheduleDate(ScreeningVisitHistoryDto screeningVisitDto);
        void ScheduleVisitUpdate(int screeningEntryId);
        IList<DropDownDto> GetVisitByLockedDropDown(LockUnlockDDDto lockUnlockDDDto);

        bool ValidateRepeatVisit(int id);

        // Dashboard chart for Visit Status
        List<DashboardQueryStatusDto> GetVisitStatus(int projectId);
    }
}
