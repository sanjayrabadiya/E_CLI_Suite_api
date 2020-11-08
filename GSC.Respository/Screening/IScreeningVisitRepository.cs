﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using System;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public interface IScreeningVisitRepository : IGenericRepository<ScreeningVisit>
    {
        void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId,int projectDesignVisitId, DateTime visitDate);
        void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto);
        void OpenVisit(int screeningVisitId, DateTime visitDate);
        void VisitRepeat(int screeningVisitId);
        void PatientStatus(int screeningEntryId);
        void AutomaticStatusUpdate(int screeningTemplateId);
        bool IsPatientScreeningFailure(int screeningVisitId);
        List<ScreeningVisitTree> GetVisitTree(int screeningEntryId);
    }
}
