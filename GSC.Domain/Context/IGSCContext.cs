﻿using GSC.Common;
using GSC.Common.Base;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.AdverseEvent;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.IDVerificationSystem;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.LabReportManagement;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Entities.License;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.LogReport;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Pharmacy;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Project.Rights;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.ProjectRight;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.SupplyManagement;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Entities.Volunteer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Domain.Context
{
    public interface IGSCContext : IContext
    {
        DbSet<InvestigatorContact> InvestigatorContact { get; set; }
        DbSet<Activity> Activity { get; set; }
        DbSet<AppScreen> AppScreen { get; set; }
        DbSet<Attendance> Attendance { get; set; }
        DbSet<City> City { get; set; }
        DbSet<Client> Client { get; set; }
        DbSet<ClientAddress> ClientAddress { get; set; }
        DbSet<ClientContact> ClientContact { get; set; }
        DbSet<ClientHistory> ClientHistory { get; set; }
        DbSet<ContactType> ContactType { get; set; }
        DbSet<Country> Country { get; set; }
        DbSet<Department> Department { get; set; }
        DbSet<DesignTrial> DesignTrial { get; set; }
        DbSet<DocumentType> DocumentType { get; set; }
        DbSet<DocumentName> DocumentName { get; set; }
        DbSet<Data.Entities.Master.Domain> Domain { get; set; }
        DbSet<DomainClass> DomainClass { get; set; }
        DbSet<Drug> Drug { get; set; }
        DbSet<FoodType> FoodType { get; set; }
        DbSet<Freezer> Freezer { get; set; }
        DbSet<Language> Language { get; set; }
        DbSet<ClientType> ClientType { get; set; }
        DbSet<Location> Location { get; set; }
        DbSet<MaritalStatus> MaritalStatus { get; set; }
        DbSet<Occupation> Occupation { get; set; }
        DbSet<PopulationType> PopulationType { get; set; }
        DbSet<ProductType> ProductType { get; set; }
        DbSet<Product> Product { get; set; }
        DbSet<Project> Project { get; set; }
        DbSet<Race> Race { get; set; }
        DbSet<Religion> Religion { get; set; }
        DbSet<RolePermission> RolePermission { get; set; }
        DbSet<ScopeName> ScopeName { get; set; }
        DbSet<SecurityRole> SecurityRole { get; set; }
        DbSet<State> State { get; set; }
        DbSet<TrialType> TrialType { get; set; }
        DbSet<UserAccessScreen> UserAccessScreen { get; set; }
        DbSet<UserLoginReport> UserLoginReport { get; set; }
        DbSet<UserPassword> UserPassword { get; set; }
        DbSet<RefreshToken> RefreshToken { get; set; }
        DbSet<UserRole> UserRole { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<VariableGroup> VariableGroup { get; set; }
        DbSet<Volunteer> Volunteer { get; set; }
        DbSet<VolunteerAddress> VolunteerAddress { get; set; }
        DbSet<VolunteerBiometric> VolunteerBiometric { get; set; }
        DbSet<VolunteerContact> VolunteerContact { get; set; }
        DbSet<VolunteerDocument> VolunteerDocument { get; set; }
        DbSet<VolunteerFood> VolunteerFood { get; set; }
        DbSet<VolunteerHistory> VolunteerHistory { get; set; }
        DbSet<VolunteerLanguage> VolunteerLanguage { get; set; }
        DbSet<LoginPreference> LoginPreference { get; set; }
        DbSet<EmailTemplate> EmailTemplate { get; set; }
        DbSet<VolunteerImage> VolunteerImage { get; set; }
        DbSet<Variable> Variable { get; set; }
        DbSet<UserImage> UserImage { get; set; }
        DbSet<Company> Company { get; set; }
        DbSet<UploadSetting> UploadSetting { get; set; }
        DbSet<EmailSetting> EmailSetting { get; set; }
        DbSet<VariableTemplate> VariableTemplate { get; set; }
        DbSet<VariableTemplateDetail> VariableTemplateDetail { get; set; }
        DbSet<UserOtp> UserOtp { get; set; }
        DbSet<VariableCategory> VariableCategory { get; set; }
        DbSet<VariableValue> VariableValue { get; set; }
        DbSet<VariableRemarks> VariableRemarks { get; set; }
        DbSet<CityArea> CityArea { get; set; }
        DbSet<AnnotationType> AnnotationType { get; set; }
        DbSet<Unit> Unit { get; set; }
        DbSet<MProductForm> MProductForm { get; set; }
        DbSet<NumberFormat> NumberFormat { get; set; }
        DbSet<Test> Test { get; set; }
        DbSet<TestGroup> TestGroup { get; set; }
        DbSet<AuditReason> AuditReason { get; set; }
        DbSet<VolunteerAuditTrail> VolunteerAuditTrail { get; set; }

        DbSet<AppSetting> AppSetting { get; set; }
        DbSet<ProjectDesign> ProjectDesign { get; set; }
        DbSet<ProjectDesignPeriod> ProjectDesignPeriod { get; set; }
        DbSet<ProjectDesignVisit> ProjectDesignVisit { get; set; }
        DbSet<ProjectDesignTemplate> ProjectDesignTemplate { get; set; }
        DbSet<ProjectDesignVariable> ProjectDesignVariable { get; set; }
        DbSet<ProjectDesignVariableValue> ProjectDesignVariableValue { get; set; }
        DbSet<ProjectDesignVariableRemarks> ProjectDesignVariableRemarks { get; set; }
        DbSet<ProjectRight> ProjectRight { get; set; }
        DbSet<ProjectDocument> ProjectDocument { get; set; }
        DbSet<ProjectDocumentReview> ProjectDocumentReview { get; set; }
        DbSet<ProjectWorkflow> ProjectWorkflow { get; set; }
        DbSet<ProjectWorkflowIndependent> ProjectWorkflowIndependent { get; set; }
        DbSet<ProjectWorkflowLevel> ProjectWorkflowLevel { get; set; }
        DbSet<ProjectSchedule> ProjectSchedule { get; set; }
        DbSet<ProjectScheduleTemplate> ProjectScheduleTemplate { get; set; }

        DbSet<VariableTemplateNote> VariableTemplateNote { get; set; }
        DbSet<UserFavoriteScreen> UserFavoriteScreen { get; set; }
        DbSet<ScreeningEntry> ScreeningEntry { get; set; }
        DbSet<ScreeningTemplate> ScreeningTemplate { get; set; }
        DbSet<ScreeningVisit> ScreeningVisit { get; set; }
        DbSet<ScreeningVisitHistory> ScreeningVisitHistory { get; set; }
        DbSet<ScreeningTemplateValue> ScreeningTemplateValue { get; set; }
        DbSet<ScreeningTemplateValueAudit> ScreeningTemplateValueAudit { get; set; }
        DbSet<ScreeningTemplateValueComment> ScreeningTemplateValueComment { get; set; }
        DbSet<ScreeningTemplateValueChild> ScreeningTemplateValueChild { get; set; }
        DbSet<ScreeningTemplateRemarksChild> ScreeningTemplateRemarksChild { get; set; }
        DbSet<ScreeningTemplateValueQuery> ScreeningTemplateValueQuery { get; set; }

        DbSet<VolunteerBlockHistory> VolunteerBlockHistory { get; set; }
        DbSet<TemplateRights> TemplateRights { get; set; }
        DbSet<TemplateRightsRoleList> TemplateRightsRoleList { get; set; }
        DbSet<UserRecentItem> UserRecentItem { get; set; }
        DbSet<BlockCategory> BlockCategory { get; set; }
        DbSet<ScreeningHistory> ScreeningHistory { get; set; }
        DbSet<UserGridSetting> UserGridSetting { get; set; }
        DbSet<ScreeningTemplateReview> ScreeningTemplateReview { get; set; }
        DbSet<PharmacyConfig> PharmacyConfig { get; set; }
        DbSet<BarcodeType> BarcodeType { get; set; }
        DbSet<BarcodeConfig> BarcodeConfig { get; set; }
        DbSet<PharmacyTemplateValue> PharmacyTemplateValue { get; set; }
        DbSet<PharmacyTemplateValueAudit> PharmacyTemplateValueAudit { get; set; }
        DbSet<PharmacyTemplateValueChild> PharmacyTemplateValueChild { get; set; }

        DbSet<PharmacyEntry> PharmacyEntry { get; set; }

        DbSet<CustomTable> CustomTable { get; set; }
        DbSet<CompanyData> CompanyData { get; set; }
        DbSet<CntTable> CntTable { get; set; }
        DbSet<ProjectSubject> ProjectSubject { get; set; }
        DbSet<EditCheck> EditCheck { get; set; }
        DbSet<EditCheckDetail> EditCheckDetail { get; set; }


        DbSet<Randomization> Randomization { get; set; }
        DbSet<ReportSetting> ReportSetting { get; set; }


        DbSet<BarcodeGenerate> BarcodeGenerate { get; set; }
        DbSet<BarcodeSubjectDetail> BarcodeSubjectDetail { get; set; }

        DbSet<PharmacyVerificationTemplateValue> PharmacyVerificationTemplateValue { get; set; }
        DbSet<PharmacyVerificationTemplateValueAudit> PharmacyVerificationTemplateValueAudit { get; set; }
        DbSet<PharmacyVerificationTemplateValueChild> PharmacyVerificationTemplateValueChild { get; set; }
        DbSet<PharmacyVerificationEntry> PharmacyVerificationEntry { get; set; }
        DbSet<AttendanceHistory> AttendanceHistory { get; set; }

        DbSet<MedraConfig> MedraConfig { get; set; }
        DbSet<MedraVersion> MedraVersion { get; set; }
        DbSet<ScreeningTemplateLockUnlockAudit> ScreeningTemplateLockUnlockAudit { get; set; }

        DbSet<Dictionary> Dictionary { get; set; }
        DbSet<ProjectDesignReportSetting> ProjectDesignReportSetting { get; set; }
        DbSet<StudyScoping> StudyScoping { get; set; }
        DbSet<MedraLanguage> MedraLanguage { get; set; }
        DbSet<MeddraHlgtHltComp> MeddraHlgtHltComp { get; set; }
        DbSet<MeddraHlgtPrefTerm> MeddraHlgtPrefTerm { get; set; }
        DbSet<MeddraHltPrefComp> MeddraHltPrefComp { get; set; }
        DbSet<MeddraHltPrefTerm> MeddraHltPrefTerm { get; set; }
        DbSet<MeddraLowLevelTerm> MeddraLowLevelTerm { get; set; }
        DbSet<MeddraMdHierarchy> MeddraMdHierarchy { get; set; }
        DbSet<MeddraPrefTerm> MeddraPrefTerm { get; set; }
        DbSet<MeddraSmqContent> MeddraSmqContent { get; set; }
        DbSet<MeddraSmqList> MeddraSmqList { get; set; }
        DbSet<MeddraSocHlgtComp> MeddraSocHlgtComp { get; set; }
        DbSet<MeddraSocIntlOrder> MeddraSocIntlOrder { get; set; }
        DbSet<MeddraSocTerm> MeddraSocTerm { get; set; }
        DbSet<MeddraCoding> MeddraCoding { get; set; }
        DbSet<MeddraCodingComment> MeddraCodingComment { get; set; }
        DbSet<MeddraCodingAudit> MeddraCodingAudit { get; set; }
        DbSet<ElectronicSignature> ElectronicSignature { get; set; }
        DbSet<EtmfMasterLibrary> EtmfMasterLibrary { get; set; }
        DbSet<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }

        DbSet<EtmfProjectWorkPlace> EtmfProjectWorkPlace { get; set; }
        DbSet<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
        DbSet<ProjectWorkplaceSubSecArtificatedocument> ProjectWorkplaceSubSecArtificatedocument { get; set; }
        DbSet<InvestigatorContactDetail> InvestigatorContactDetail { get; set; }
        DbSet<ProjectDesignVisitStatus> ProjectDesignVisitStatus { get; set; }
        DbSet<Holiday> Holiday { get; set; }
        DbSet<ManageSite> ManageSite { get; set; }
        DbSet<Iecirb> Iecirb { get; set; }
        DbSet<JobMonitoring> JobMonitoring { get; set; }
        DbSet<PatientStatus> PatientStatus { get; set; }
        DbSet<VisitStatus> VisitStatus { get; set; }
        DbSet<ReportScreen> ReportScreen { get; set; }
        DbSet<ReportFavouriteScreen> ReportFavouriteScreen { get; set; }
        DbSet<ProjectArtificateDocumentReview> ProjectArtificateDocumentReview { get; set; }
        DbSet<ProjectArtificateDocumentComment> ProjectArtificateDocumentComment { get; set; }

        DbSet<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }
        DbSet<EconsentSetup> EconsentSetup { get; set; }
        DbSet<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        DbSet<EconsentSectionReference> EconsentSectionReference { get; set; }
        DbSet<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        DbSet<EconsentChat> EconsentChat { get; set; }
        DbSet<RegulatoryType> RegulatoryType { get; set; }
        DbSet<ProjectArtificateDocumentApprover> ProjectArtificateDocumentApprover { get; set; }
        DbSet<Site> Site { get; set; }
        DbSet<ManageSiteRole> ManageSiteRole { get; set; }
        DbSet<ProjectSubSecArtificateDocumentApprover> ProjectSubSecArtificateDocumentApprover { get; set; }
        DbSet<ProjectSubSecArtificateDocumentComment> ProjectSubSecArtificateDocumentComment { get; set; }
        DbSet<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
        DbSet<ProjectSubSecArtificateDocumentReview> ProjectSubSecArtificateDocumentReview { get; set; }

        DbSet<ProjectModuleRights> ProjectModuleRights { get; set; }
        DbSet<VisitDeviationReport> VisitDeviationReport { get; set; }
        DbSet<SiteTeam> SiteTeam { get; set; }
        DbSet<AppScreenPatient> AppScreenPatient { get; set; }
        DbSet<AppScreenPatientRights> AppScreenPatientRights { get; set; }
        DbSet<SMSSetting> SMSSetting { get; set; }
        DbSet<VariableCategoryLanguage> VariableCategoryLanguage { get; set; }
        DbSet<ScreeningTemplateEditCheckValue> ScreeningTemplateEditCheckValue { get; set; }
        DbSet<AEReporting> AEReporting { get; set; }
        DbSet<AdverseEventSettings> AdverseEventSettings { get; set; }
        DbSet<EtmfUserPermission> EtmfUserPermission { get; set; }
        DbSet<AEReportingValue> AEReportingValue { get; set; }
        DbSet<AdverseEventSettingsLanguage> AdverseEventSettingsLanguage { get; set; }
        DbSet<TaskTemplate> TaskTemplate { get; set; }
        DbSet<PhaseManagement> PhaseManagement { get; set; }
        DbSet<ResourceType> ResourceType { get; set; }
        DbSet<TaskMaster> TaskMaster { get; set; }
        DbSet<StudyPlan> StudyPlan { get; set; }
        DbSet<CurrencyRate> CurrencyRate { get; set; }
        DbSet<StudyPlanTask> StudyPlanTask { get; set; }
        DbSet<ProjectDesignVariableEncryptRole> ProjectDesignVariableEncryptRole { get; set; }
        DbSet<ProjectDesingTemplateRestriction> ProjectDesingTemplateRestriction { get; set; }
        DbSet<UserSetting> UserSetting { get; set; }
        DbSet<RandomizationNumberSettings> RandomizationNumberSettings { get; set; }
        DbSet<ScreeningNumberSettings> ScreeningNumberSettings { get; set; }
        DbSet<VisitLanguage> VisitLanguage { get; set; }
        DbSet<TemplateLanguage> TemplateLanguage { get; set; }
        DbSet<TemplateNoteLanguage> TemplateNoteLanguage { get; set; }
        DbSet<VariableLanguage> VariableLanguage { get; set; }
        DbSet<VariableNoteLanguage> VariableNoteLanguage { get; set; }
        DbSet<VariableValueLanguage> VariableValueLanguage { get; set; }
        DbSet<EConsentVideo> EConsentVideo { get; set; }
        DbSet<DependentTaskDto> DependentTaskDto { get; set; }
        DbSet<HolidayMaster> HolidayMaster { get; set; }
        DbSet<WeekEndMaster> WeekEndMaster { get; set; }
        DbSet<SupplyLocation> SupplyLocation { get; set; }
        DbSet<CentralDepot> CentralDepot { get; set; }
        DbSet<StudyVersion> StudyVersion { get; set; }
        DbSet<StudyVersionStatus> StudyVersionStatus { get; set; }
        DbSet<ProjectDesignTemplateNote> ProjectDesignTemplateNote { get; set; }
        DbSet<VolunteerQuery> VolunteerQuery { get; set; }
        DbSet<PharmacyStudyProductType> PharmacyStudyProductType { get; set; }
        DbSet<ProductReceipt> ProductReceipt { get; set; }
        DbSet<StudyPlanTaskResource> StudyPlanTaskResource { get; set; }
        DbSet<ProductVerification> ProductVerification { get; set; }
        DbSet<ProductVerificationDetail> ProductVerificationDetail { get; set; }
        DbSet<LanguageConfiguration> LanguageConfiguration { get; set; }
        DbSet<LanguageConfigurationDetails> LanguageConfigurationDetails { get; set; }
        DbSet<VerificationApprovalTemplate> VerificationApprovalTemplate { get; set; }
        DbSet<VerificationApprovalTemplateValue> VerificationApprovalTemplateValue { get; set; }
        DbSet<VerificationApprovalTemplateValueChild> VerificationApprovalTemplateValueChild { get; set; }
        DbSet<TableFieldName> TableFieldName { get; set; }
        DbSet<BarcodeCombination> BarcodeCombination { get; set; }
        DbSet<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; }
        DbSet<UploadLimit> UploadLimit { get; set; }
        DbSet<VerificationApprovalTemplateHistory> VerificationApprovalTemplateHistory { get; set; }
        DbSet<VerificationApprovalTemplateValueAudit> VerificationApprovalTemplateValueAudit { get; set; }
        DbSet<SupplyManagementConfiguration> SupplyManagementConfiguration { get; set; }
        DbSet<AttendanceBarcodeGenerate> AttendanceBarcodeGenerate { get; set; }
        DbSet<BarcodeAudit> BarcodeAudit { get; set; }
        DbSet<LabManagementConfiguration> LabManagementConfiguration { get; set; }
        DbSet<LabManagementVariableMapping> LabManagementVariableMapping { get; set; }
        DbSet<LabManagementUploadData> LabManagementUploadData { get; set; }
        DbSet<LabManagementUploadExcelData> LabManagementUploadExcelData { get; set; }
        DbSet<EconsentReviewDetailsAudit> EconsentReviewDetailsAudit { get; set; }
        DbSet<SyncConfigurationMaster> SyncConfigurationMaster { get; set; }
        DbSet<SyncConfigurationMasterDetails> SyncConfigurationMasterDetails { get; set; }
        DbSet<SyncConfigurationMasterDetailsAudit> SyncConfigurationMasterDetailsAudit { get; set; }
        DbSet<FileSizeConfiguration> FileSizeConfiguration { get; set; }
        DbSet<SupplyManagementUploadFile> SupplyManagementUploadFile { get; set; }
        DbSet<LabManagementSendEmailUser> LabManagementSendEmailUser { get; set; }
        DbSet<CtmsActivity> CtmsActivity { get; set; }
        DbSet<ProjectSettings> ProjectSettings { get; set; }
        DbSet<StudyLevelForm> StudyLevelForm { get; set; }
        DbSet<StudyLevelFormVariable> StudyLevelFormVariable { get; set; }
        DbSet<StudyLevelFormVariableValue> StudyLevelFormVariableValue { get; set; }
        DbSet<StudyLevelFormVariableRemarks> StudyLevelFormVariableRemarks { get; set; }

        DbSet<AdverseEventSettingsDetails> AdverseEventSettingsDetails { get; set; }
        DbSet<CtmsMonitoring> CtmsMonitoring { get; set; }
        DbSet<CtmsMonitoringReport> CtmsMonitoringReport { get; set; }
        DbSet<CtmsMonitoringReportReview> CtmsMonitoringReportReview { get; set; }
        DbSet<CtmsMonitoringReportVariableValue> CtmsMonitoringReportVariableValue { get; set; }
        DbSet<CtmsMonitoringReportVariableValueQuery> CtmsMonitoringReportVariableValueQuery { get; set; }
        DbSet<CtmsMonitoringReportVariableValueAudit> CtmsMonitoringReportVariableValueAudit { get; set; }
        DbSet<CtmsMonitoringReportVariableValueChild> CtmsMonitoringReportVariableValueChild { get; set; }
        DbSet<CtmsActionPoint> CtmsActionPoint { get; set; }
        DbSet<CtmsMonitoringStatus> CtmsMonitoringStatus { get; set; }

        DbSet<SupplyManagementRequest> SupplyManagementRequest { get; set; }
        DbSet<SupplyManagementShipment> SupplyManagementShipment { get; set; }
        DbSet<SupplyManagementReceipt> SupplyManagementReceipt { get; set; }
        DbSet<SupplyManagementUploadFileVisit> SupplyManagementUploadFileVisit { get; set; }
        DbSet<SupplyManagementUploadFileDetail> SupplyManagementUploadFileDetail { get; set; }

        DbSet<PageConfiguration> PageConfiguration { get; set; }
        DbSet<PageConfigurationFields> PageConfigurationFields { get; set; }
        DbSet<SendEmailOnVariableChangeSetting> SendEmailOnVariableChangeSetting { get; set; }
        DbSet<SendEmailOnVariableValue> SendEmailOnVariableValue { get; set; }
        DbSet<ScheduleTerminateDetail> ScheduleTerminateDetail { get; set; }
        DbSet<TemplateVariableSequenceNoSetting> TemplateVariableSequenceNoSetting { get; set; }

        DbSet<SupplyManagementAllocation> SupplyManagementAllocation { get; set; }

        DbSet<SupplyManagementKIT> SupplyManagementKIT { get; set; }
        DbSet<SupplyManagementKITDetail> SupplyManagementKITDetail { get; set; }

        DbSet<ScreeningEntryStudyHistory> ScreeningEntryStudyHistory { get; set; }
        DbSet<EconsentGlossary> EconsentGlossary { get; set; }
        DbSet<ScreeningSetting> ScreeningSetting { get; set; }
        DbSet<VolunteerFinger> VolunteerFinger { get; set; }
        DbSet<ProjectStatus> ProjectStatus { get; set; }
        DbSet<SupplyManagementFector> SupplyManagementFector { get; set; }

        DbSet<SupplyManagementFectorDetail> SupplyManagementFectorDetail { get; set; }

        DbSet<SupplyManagementKitAllocationSettings> SupplyManagementKitAllocationSettings { get; set; }

        DbSet<SupplyManagementKitNumberSettings> SupplyManagementKitNumberSettings { get; set; }

        DbSet<SupplyManagementVisitKITDetail> SupplyManagementVisitKITDetail { get; set; }

        DbSet<SupplyManagementKITDetailHistory> SupplyManagementKITDetailHistory { get; set; }
        DbSet<SupplyManagementKITReturn> SupplyManagementKITReturn { get; set; }

        DbSet<SupplyManagementKITDiscard> SupplyManagementKITDiscard { get; set; }

        DbSet<SupplyManagementEmailConfiguration> SupplyManagementEmailConfiguration { get; set; }

        DbSet<SupplyManagementEmailConfigurationDetail> SupplyManagementEmailConfigurationDetail { get; set; }

        DbSet<SupplyManagementEmailConfigurationDetailHistory> SupplyManagementEmailConfigurationDetailHistory { get; set; }

        DbSet<SupplyManagementKITSeries> SupplyManagementKITSeries { get; set; }

        DbSet<SupplyManagementKITSeriesDetail> SupplyManagementKITSeriesDetail { get; set; }

        DbSet<SupplyManagementKITSeriesDetailHistory> SupplyManagementKITSeriesDetailHistory { get; set; }

        DbSet<SupplyManagementVisitKITSequenceDetail> SupplyManagementVisitKITSequenceDetail { get; set; }

        DbSet<SupplyManagementKITReturnVerification> SupplyManagementKITReturnVerification { get; set; }

        DbSet<SupplyManagementKITReturnSeries> SupplyManagementKITReturnSeries { get; set; }

        DbSet<SupplyManagementKITReturnVerificationSeries> SupplyManagementKITReturnVerificationSeries { get; set; }

        DbSet<SupplyManagementUnblindTreatment> SupplyManagementUnblindTreatment { get; set; }

        DbSet<SupplyManagementFactorMapping> SupplyManagementFactorMapping { get; set; }

        DbSet<PKBarcode> PKBarcode { get; set; }
        DbSet<DossingBarcode> DossingBarcode { get; set; }

        DbSet<SampleBarcode> SampleBarcode { get; set; }
        DbSet<ManageSiteAddress> ManageSiteAddress { get; set; }
        DbSet<ProjectSiteAddress> ProjectSiteAddress { get; set; }

        DbSet<Centrifugation> Centrifugation { get; set; }
        DbSet<CentrifugationDetails> CentrifugationDetails { get; set; }
        DbSet<SampleSeparation> SampleSeparation { get; set; }
        DbSet<PkBarcodeGenerate> PkBarcodeGenerate { get; set; }
        DbSet<SampleBarcodeGenerate> SampleBarcodeGenerate { get; set; }
        DbSet<DossingBarcodeGenerate> DossingBarcodeGenerate { get; set; }

        DbSet<SupplyManagementApproval> SupplyManagementApproval { get; set; }

        DbSet<SupplyManagementApprovalDetails> SupplyManagementApprovalDetails { get; set; }

        DbSet<SupplyManagementShipmentApproval> SupplyManagementShipmentApproval { get; set; }
        DbSet<VendorManagement> VendorManagement { get; set; }
        DbSet<PlanMetrics> PlanMetrics { get; set; }
        DbSet<OverTimeMetrics> OverTimeMetrics { get; set; }

        DbSet<EmailConfigurationEditCheck> EmailConfigurationEditCheck { get; set; }
        DbSet<EmailConfigurationEditCheckDetail> EmailConfigurationEditCheckDetail { get; set; }
        DbSet<EmailConfigurationEditCheckRole> EmailConfigurationEditCheckRole { get; set; }

        DbSet<EmailConfigurationEditCheckSendMailHistory> EmailConfigurationEditCheckSendMailHistory { get; set; }
        DbSet<VariableLabelLanguage> VariableLabelLanguage { get; set; }
        DbSet<RefrenceTypes> RefrenceTypes { get; set; }
        DbSet<SupplyManagementKitDosePriority> SupplyManagementKitDosePriority { get; set; }
        DbSet<WorkflowVisit> WorkflowVisit { get; set; }
        DbSet<WorkflowTemplate> WorkflowTemplate { get; set; }
        DbSet<ProjectDesignVisitRestriction> ProjectDesignVisitRestriction { get; set; }
        DbSet<VisitEmailConfiguration> VisitEmailConfiguration { get; set; }
        DbSet<VisitEmailConfigurationRoles> VisitEmailConfigurationRoles { get; set; }
        DbSet<LettersFormate> LettersFormate { get; set; }
        DbSet<LettersActivity> LettersActivity { get; set; }
        DbSet<LabReport> LabReport { get; set; }
        DbSet<WorkingDay> WorkingDay { get; set; }
        DbSet<SiteTypes> SiteTypes { get; set; }
        DbSet<IDVerification> IDVerification { get; set; }
        DbSet<IDVerificationFile> IDVerificationFile { get; set; }
        DbSet<Designation> Designation { get; set; }
        DbSet<StudyPlanResource> StudyPlanResource { get; set; }
        DbSet<Currency> Currency { get; set; }
        DbSet<UserAccess> UserAccess { get; set; }
        DbSet<Procedure> Procedure { get; set; }
        DbSet<PatientCost> PatientCost { get; set; }
        DbSet<PassThroughCostActivity> PassThroughCostActivity { get; set; }
        DbSet<PassThroughCost> PassThroughCost { get; set; }
        DbSet<SupplyManagementKitNumberSettingsRole> SupplyManagementKitNumberSettingsRole { get; set; }

        DbSet<PharmacyBarcodeConfig> PharmacyBarcodeConfig { get; set; }
        DbSet<PharmacyBarcodeDisplayInfo> PharmacyBarcodeDisplayInfo { get; set; }
        DbSet<SupplyManagementEmailScheduleLog> SupplyManagementEmailScheduleLog { get; set; }

        DbSet<SupplyManagementThresholdHistory> SupplyManagementThresholdHistory { get; set; }
        DbSet<ProjectDesignTemplateSiteAccess> ProjectDesignTemplateSiteAccess { get; set; }
        DbSet<ResourceMilestone> ResourceMilestone { get; set; }
        DbSet<PatientMilestone> PatientMilestone { get; set; }
        DbSet<PassthroughMilestone> PassthroughMilestone { get; set; }
        DbSet<BudgetPaymentFinalCost> BudgetPaymentFinalCost { get; set; }
        DbSet<CtmsApprovalRoles> CtmsApprovalRoles { get; set; }
        DbSet<CtmsApprovalUsers> CtmsApprovalUsers { get; set; }
        DbSet<CtmsWorkflowApproval> CtmsWorkflowApproval { get; set; }
        DbSet<CtmsStudyPlanTaskComment> CtmsStudyPlanTaskComment { get; set; }
        DbSet<PaymentTerms> PaymentTerms { get; set; }
        DbSet<PassthroughMilestoneInvoice> PassthroughMilestoneInvoice { get; set; }
        DbSet<PatientMilestoneInvoice> PatientMilestoneInvoice { get; set; }
        DbSet<ResourceMilestoneInvoice> ResourceMilestoneInvoice { get; set; }
        DbSet<UserUUID> UserUUID { get; set; }
        DbSet<LiecenceObj> LiecenceObj { get; set; }
        DbSet<SitePayment> SitePayment { get; set; }
        DbSet<SiteContract> SiteContract { get; set; }
        DbSet<PatientSiteContract> PatientSiteContract { get; set; }
        DbSet<PassthroughSiteContract> PassthroughSiteContract { get; set; }
        DbSet<ContractTemplateFormat> ContractTemplateFormat { get; set; }
        DbSet<CtmsSiteContractWorkflowApproval> CtmsSiteContractWorkflowApproval { get; set; }
    }
}