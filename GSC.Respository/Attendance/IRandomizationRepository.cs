using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;

namespace GSC.Respository.Attendance
{
    public interface IRandomizationRepository : IGenericRepository<Randomization>
    {
        string Duplicate(RandomizationDto objSave, int projectId);
        List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted);
        void SaveRandomization(Randomization randomization, RandomizationDto randomizationDto);
        void SendEmailOfStartEconsent(Randomization randomization);
        void ChangeStatustoConsentInProgress();
        void ChangeStatustoConsentCompleted(int id);
        void ChangeStatustoReConsentInProgress(int id);
        void PatientStatus(ScreeningPatientStatus patientStatus, int screeningEntryId);
        void ChangeStatustoWithdrawal(FileModel fileModel);
        DashboardPatientDto GetDashboardPatientDetail();
        //List<ProjectDesignVisitMobileDto> GetPatientVisits();
        List<ProjectDesignVisitMobileDto> GetPatientVisits();
        List<ProjectDesignTemplateMobileDto> GetPatientTemplates(int screeningVisitId);
        RandomizationNumberDto GetRandomizationAndScreeningNumber(int id);
    }
}