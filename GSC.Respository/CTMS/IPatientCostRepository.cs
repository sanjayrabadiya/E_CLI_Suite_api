using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IPatientCostRepository : IGenericRepository<PatientCost>
    {
        bool CheckVisitData(bool isDeleted, int studyId);
        List<ProcedureVisitdadaDto> GetPullPatientCost(bool isDeleted, int studyId, int? procedureId, bool isPull);
        List<PatientCostGridData> GetPatientCostGrid(bool isDeleted, int studyId);
        List<PatientCostGridData> GetPatientCostCurrencyGrid(bool isDeleted, int studyId);
        string Duplicate(List<ProcedureVisitdadaDto> procedureVisitDataDto);
        void AddPatientCost(List<ProcedureVisitdadaDto> procedureVisitDataDto);
        void DeletePatientCost(int projectId, int procedureId);
        bool AddPatientCount(int studyId, int currencyId, int patientCount);
    }
}
