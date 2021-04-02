using GSC.Common;
using GSC.Common.Base;
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
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.LogReport;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Pharmacy;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Project.Rights;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.ProjectRight;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
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
        DbSet<AuditTrail> AuditTrail { get; set; }
        
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
        DbSet<VariableTemplateRight> VariableTemplateRight { get; set; }
        DbSet<ScreeningTemplateReview> ScreeningTemplateReview { get; set; }
        DbSet<PharmacyConfig> PharmacyConfig { get; set; }
        DbSet<BarcodeType> BarcodeType { get; set; }
        DbSet<BarcodeConfig> BarcodeConfig { get; set; }
        DbSet<PharmacyTemplateValue> PharmacyTemplateValue { get; set; }
        DbSet<PharmacyTemplateValueAudit> PharmacyTemplateValueAudit { get; set; }
        DbSet<PharmacyTemplateValueChild> PharmacyTemplateValueChild { get; set; }

        DbSet<PharmacyEntry> PharmacyEntry { get; set; }

        //DbSet<PharmacyTemplate> PharmacyTemplate { get; set; }
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
        DbSet<EtmfZoneMasterLibrary> EtmfZoneMasterLibrary { get; set; }
        DbSet<EtmfSectionMasterLibrary> EtmfSectionMasterLibrary { get; set; }
        DbSet<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }

        DbSet<ProjectWorkplace> ProjectWorkplace { get; set; }
        DbSet<ProjectWorkplaceArtificate> ProjectWorkplaceArtificate { get; set; }
        DbSet<ProjectWorkplaceDetail> ProjectWorkplaceDetail { get; set; }
        DbSet<ProjectWorkplaceSection> ProjectWorkplaceSection { get; set; }
        DbSet<ProjectWorkPlaceZone> ProjectWorkPlaceZone { get; set; }
        DbSet<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
        DbSet<ProjectWorkplaceSubSection> ProjectWorkplaceSubSection { get; set; }
        DbSet<ProjectWorkplaceSubSectionArtifact> ProjectWorkplaceSubSectionArtifact { get; set; }
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
        DbSet<EconsentSetupPatientStatus> EconsentSetupPatientStatus { get; set; }
        DbSet<EconsentSetup> EconsentSetup { get; set; }
        DbSet<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        DbSet<EconsentSectionReference> EconsentSectionReference { get; set; }
        DbSet<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        DbSet<EconsentChat> EconsentChat { get; set; }
        DbSet<EconsentSetupRoles> EconsentSetupRoles { get; set; }
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
        DbSet<StudyPlanTask> StudyPlanTask { get; set; }
        DbSet<ProjectDesignVariableRelation> ProjectDesignVariableRelation { get; set; }
        DbSet<ProjectDesignVariableEncryptRole> ProjectDesignVariableEncryptRole { get; set; }
        DbSet<TemplatePermission> TemplatePermission { get; set; }
        DbSet<UserSetting> UserSetting { get; set; }
        DbSet<RandomizationNumberSettings> RandomizationNumberSettings { get; set; }
        DbSet<ScreeningNumberSettings> ScreeningNumberSettings { get; set; }
    }
}
