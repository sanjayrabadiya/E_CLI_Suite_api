using GSC.Data.Dto.Screening;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Screening
{
    public interface IScreeningProgress
    {
        ScreeningProgressDto GetScreeningProgress(int screeningEntryId, int screeningTemplateId);
        int SetTemplateCount(int id);
    }
}
