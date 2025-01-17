﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Shared.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Respository.Attendance
{
    public interface IRandomizationRepository : IGenericRepository<Randomization>
    {
        string Duplicate(RandomizationDto objSave, int projectId);
        List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted);
        void SaveRandomizationNumber(Randomization randomization, RandomizationDto randomizationDto);
        void SaveScreeningNumber(Randomization randomization, RandomizationDto randomizationDto);
        void SendEmailOfStartEconsent(Randomization randomization);
        Task SendEmailOfScreenedtoPatient(Randomization randomization, int sendtype);
        void ChangeStatustoConsentCompleted(int id);
        void ChangeStatustoReConsentInProgress(int id);
        Task PatientStatus(ScreeningPatientStatus patientStatus, int screeningEntryId);
        void ChangeStatustoWithdrawal();
        DashboardPatientDto GetDashboardPatientDetail();
        List<ProjectDesignVisitMobileDto> GetPatientVisits();
        List<ProjectDesignTemplateMobileDto> GetPatientTemplates(int screeningVisitId);
        RandomizationNumberDto GetRandomizationNumber(int id);
        RandomizationNumberDto GetScreeningNumber(int id);
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

        void UpdateRandomizationIdForIWRS(RandomizationDto obj);
        Task SendEmailOfScreenedtoPatientLAR(Randomization randomization, int sendtype);
        void SendEmailOfStartEconsentLAR(Randomization randomization);
        void AddRandomizationUser(UserDto userDto, CommonResponceView userdetails);
        void AddRandomizationUserLAR(UserDto userLarDto, CommonResponceView userLardetails);

        bool ValidateRandomizationIdForIWRS(RandomizationDto obj);

        RandomizationDto GetRandomizationNumberIWRS(RandomizationDto randomizationDto);

        RandomizationDto SetKitNumber(RandomizationDto obj);
        void UpdateRandmizationKitNotAssigned(RandomizationDto obj);

        void SendRandomizationIWRSEMail(RandomizationDto obj);

        void SendRandomizationThresholdEMail(RandomizationDto obj);

        bool CheckDUplicateRandomizationNumber(RandomizationDto obj);

        void SetFactorMappingData(Randomization randomizationDto);

        void RevertKitData(RandomizationDto obj);

        RandomizationDto CheckDuplicateRandomizationNumberIWRS(RandomizationDto obj, RandomizationNumberSettings numerformate);

        List<ScreeningVisitForSubject> GetPatientVisitsForMobile();

        Task SendRandomizationThresholdEmailSchedule();

        List<RandomizationGridDto> GetRandomizationById(int id, int projectId);
    }
}