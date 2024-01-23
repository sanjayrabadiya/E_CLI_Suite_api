using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IPatientCostRepository : IGenericRepository<StudyPlan>
    {
        List<ProcedureVisitdadaDto> getBudgetPlaner(bool isDeleted, int studyId);
    }
}
