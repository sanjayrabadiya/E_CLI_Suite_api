using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Shared.DocumentService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Respository.Attendance
{
    public interface IRandomizationRepository : IGenericRepository<Randomization>
    {
        string Duplicate(RandomizationDto objSave, int projectId);
        List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted);
        //void SaveRandomization(Randomization randomization, RandomizationDto randomizationDto);
        void SaveRandomizationNumber(Randomization randomization, RandomizationDto randomizationDto);
        void SaveScreeningNumber(Randomization randomization, RandomizationDto randomizationDto);
        void SendEmailOfStartEconsent(Randomization randomization);
        Task SendEmailOfScreenedtoPatient(Randomization randomization, int sendtype); 
        //void ChangeStatustoConsentInProgress();
        void ChangeStatustoConsentCompleted(int id);
        void ChangeStatustoReConsentInProgress(int id);
        Task PatientStatus(ScreeningPatientStatus patientStatus, int screeningEntryId);
        void ChangeStatustoWithdrawal();
        DashboardPatientDto GetDashboardPatientDetail();
        //List<ProjectDesignVisitMobileDto> GetPatientVisits();
        List<ProjectDesignVisitMobileDto> GetPatientVisits();
        List<ProjectDesignTemplateMobileDto> GetPatientTemplates(int screeningVisitId);
        //RandomizationNumberDto GetRandomizationAndScreeningNumber(int id);
        RandomizationNumberDto GetRandomizationNumber(int id);
        RandomizationNumberDto GetScreeningNumber(int id);
        //string ValidateRandomizationAndScreeningNumber(RandomizationDto randomization);
        string ValidateScreeningNumber(RandomizationDto randomization);
        string ValidateRandomizationNumber(RandomizationDto randomization);
        List<DropDownDto> GetRandomizationDropdown(int projectid);
        bool IsScreeningFormatSetInStudy(int id);
        bool IsRandomFormatSetInStudy(int id);

        // Dashboard chart for Subject Status
        public List<DashboardQueryStatusDto> GetSubjectStatus(int projectId);
        List<DropDownDto> GetAttendanceForMeddraCodingDropDown(MeddraCodingSearchDto filters);
        List<DashboardPatientStatusDto> GetDashboardPatientStatus(int projectId);
        List<DashboardRecruitmentStatusDisplayDto> GetDashboardRecruitmentStatus(int projectId);
        DashboardRecruitmentRateDto GetDashboardRecruitmentRate(int projectId);
    }
}