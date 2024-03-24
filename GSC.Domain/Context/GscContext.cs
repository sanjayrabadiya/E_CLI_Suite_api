using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GSC.Common;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Dto.Configuration;
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
using GSC.Shared.Configuration;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GSC.Domain.Context
{
    public class GscContext : GSCBaseContext<GscContext>, IGSCContext, IGSCContextExtension
    {
        public GscContext(DbContextOptions<GscContext> options, ICommonSharedService commonSharedService) : base(options, commonSharedService)
        {

        }


        public void ConfigureServices(string connectionString)
        {
            base.OnConfiguring(new DbContextOptionsBuilder(GetOptions(connectionString)));

        }



        public static DbContextOptions GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (ConfigurationMapping.EnvironmentSetting != null && !ConfigurationMapping.EnvironmentSetting.IsPremise && _commonSharedService.JwtTokenAccesser.CompanyId > 0)
            {
                string companyCode = $"CompanyId{_commonSharedService.JwtTokenAccesser.CompanyId}";
                object connectionStrig;
                _commonSharedService.GSCCaching.TryGetValue(companyCode, out connectionStrig);
                if (connectionStrig != null)
                {
                    optionsBuilder.UseSqlServer(connectionStrig.ToString());
                    base.OnConfiguring(optionsBuilder);
                }
            }
        }

        public DbSet<InvestigatorContact> InvestigatorContact { get; set; }
        public DbSet<Activity> Activity { get; set; }
        public DbSet<AppScreen> AppScreen { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<ClientAddress> ClientAddress { get; set; }
        public DbSet<ClientContact> ClientContact { get; set; }
        public DbSet<ClientHistory> ClientHistory { get; set; }
        public DbSet<ContactType> ContactType { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<DesignTrial> DesignTrial { get; set; }
        public DbSet<DocumentType> DocumentType { get; set; }
        public DbSet<DocumentName> DocumentName { get; set; }
        public DbSet<Data.Entities.Master.Domain> Domain { get; set; }
        public DbSet<DomainClass> DomainClass { get; set; }
        public DbSet<Drug> Drug { get; set; }
        public DbSet<FoodType> FoodType { get; set; }
        public DbSet<Freezer> Freezer { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<ClientType> ClientType { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<MaritalStatus> MaritalStatus { get; set; }
        public DbSet<Occupation> Occupation { get; set; }
        public DbSet<PopulationType> PopulationType { get; set; }
        public DbSet<ProductType> ProductType { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<Race> Race { get; set; }
        public DbSet<Religion> Religion { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
        public DbSet<ScopeName> ScopeName { get; set; }
        public DbSet<SecurityRole> SecurityRole { get; set; }
        public DbSet<State> State { get; set; }
        public DbSet<TrialType> TrialType { get; set; }
        public DbSet<UserAccessScreen> UserAccessScreen { get; set; }
        public DbSet<UserLoginReport> UserLoginReport { get; set; }
        public DbSet<UserPassword> UserPassword { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<VariableGroup> VariableGroup { get; set; }
        public DbSet<Volunteer> Volunteer { get; set; }
        public DbSet<VolunteerAddress> VolunteerAddress { get; set; }
        public DbSet<VolunteerBiometric> VolunteerBiometric { get; set; }
        public DbSet<VolunteerContact> VolunteerContact { get; set; }
        public DbSet<VolunteerDocument> VolunteerDocument { get; set; }
        public DbSet<VolunteerFood> VolunteerFood { get; set; }
        public DbSet<VolunteerHistory> VolunteerHistory { get; set; }
        public DbSet<VolunteerLanguage> VolunteerLanguage { get; set; }
        public DbSet<LoginPreference> LoginPreference { get; set; }
        public DbSet<EmailTemplate> EmailTemplate { get; set; }
        public DbSet<VolunteerImage> VolunteerImage { get; set; }
        public DbSet<Variable> Variable { get; set; }
        public DbSet<UserImage> UserImage { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<UploadSetting> UploadSetting { get; set; }
        public DbSet<EmailSetting> EmailSetting { get; set; }
        public DbSet<VariableTemplate> VariableTemplate { get; set; }
        public DbSet<VariableTemplateDetail> VariableTemplateDetail { get; set; }
        public DbSet<UserOtp> UserOtp { get; set; }
        public DbSet<VariableCategory> VariableCategory { get; set; }
        public DbSet<VariableValue> VariableValue { get; set; }
        public DbSet<VariableRemarks> VariableRemarks { get; set; }
        public DbSet<CityArea> CityArea { get; set; }
        public DbSet<AnnotationType> AnnotationType { get; set; }
        public DbSet<Unit> Unit { get; set; }
        public DbSet<MProductForm> MProductForm { get; set; }
        public DbSet<NumberFormat> NumberFormat { get; set; }
        public DbSet<Test> Test { get; set; }
        public DbSet<TestGroup> TestGroup { get; set; }
        public DbSet<AuditReason> AuditReason { get; set; }
        public DbSet<VolunteerAuditTrail> VolunteerAuditTrail { get; set; }
        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<AppSetting> AppSetting { get; set; }
        public DbSet<ProjectDesign> ProjectDesign { get; set; }
        public DbSet<ProjectDesignPeriod> ProjectDesignPeriod { get; set; }
        public DbSet<ProjectDesignVisit> ProjectDesignVisit { get; set; }
        public DbSet<ProjectDesignTemplate> ProjectDesignTemplate { get; set; }
        public DbSet<ProjectDesignVariable> ProjectDesignVariable { get; set; }
        public DbSet<ProjectDesignVariableValue> ProjectDesignVariableValue { get; set; }
        public DbSet<ProjectDesignVariableRemarks> ProjectDesignVariableRemarks { get; set; }
        public DbSet<ProjectDesignTemplateNote> ProjectDesignTemplateNote { get; set; }
        public DbSet<ProjectRight> ProjectRight { get; set; }
        public DbSet<ProjectDocument> ProjectDocument { get; set; }
        public DbSet<ProjectDocumentReview> ProjectDocumentReview { get; set; }
        public DbSet<ProjectWorkflow> ProjectWorkflow { get; set; }
        public DbSet<ProjectWorkflowIndependent> ProjectWorkflowIndependent { get; set; }
        public DbSet<ProjectWorkflowLevel> ProjectWorkflowLevel { get; set; }
        public DbSet<ProjectSchedule> ProjectSchedule { get; set; }
        public DbSet<ProjectScheduleTemplate> ProjectScheduleTemplate { get; set; }

        public DbSet<VariableTemplateNote> VariableTemplateNote { get; set; }
        public DbSet<UserFavoriteScreen> UserFavoriteScreen { get; set; }
        public DbSet<ScreeningEntry> ScreeningEntry { get; set; }
        public DbSet<ScreeningTemplate> ScreeningTemplate { get; set; }
        public DbSet<ScreeningVisit> ScreeningVisit { get; set; }
        public DbSet<ScreeningVisitHistory> ScreeningVisitHistory { get; set; }
        public DbSet<ScreeningTemplateValue> ScreeningTemplateValue { get; set; }
        public DbSet<ScreeningTemplateValueAudit> ScreeningTemplateValueAudit { get; set; }
        public DbSet<ScreeningTemplateValueComment> ScreeningTemplateValueComment { get; set; }
        public DbSet<ScreeningTemplateValueChild> ScreeningTemplateValueChild { get; set; }
        public DbSet<ScreeningTemplateRemarksChild> ScreeningTemplateRemarksChild { get; set; }
        public DbSet<ScreeningTemplateValueQuery> ScreeningTemplateValueQuery { get; set; }

        public DbSet<VolunteerBlockHistory> VolunteerBlockHistory { get; set; }
        public DbSet<TemplateRights> TemplateRights { get; set; }
        public DbSet<TemplateRightsRoleList> TemplateRightsRoleList { get; set; }
        public DbSet<UserRecentItem> UserRecentItem { get; set; }
        public DbSet<BlockCategory> BlockCategory { get; set; }
        public DbSet<ScreeningHistory> ScreeningHistory { get; set; }
        public DbSet<UserGridSetting> UserGridSetting { get; set; }
        public DbSet<ScreeningTemplateReview> ScreeningTemplateReview { get; set; }
        public DbSet<PharmacyConfig> PharmacyConfig { get; set; }
        public DbSet<BarcodeType> BarcodeType { get; set; }
        public DbSet<BarcodeConfig> BarcodeConfig { get; set; }
        public DbSet<PharmacyTemplateValue> PharmacyTemplateValue { get; set; }
        public DbSet<PharmacyTemplateValueAudit> PharmacyTemplateValueAudit { get; set; }
        public DbSet<PharmacyTemplateValueChild> PharmacyTemplateValueChild { get; set; }
        public DbSet<PharmacyEntry> PharmacyEntry { get; set; }
        //public DbSet<PharmacyTemplate> PharmacyTemplate { get; set; }
        public DbSet<CustomTable> CustomTable { get; set; }
        public DbSet<CompanyData> CompanyData { get; set; }
        public DbSet<CntTable> CntTable { get; set; }
        public DbSet<ProjectSubject> ProjectSubject { get; set; }
        public DbSet<EditCheck> EditCheck { get; set; }
        public DbSet<EditCheckDetail> EditCheckDetail { get; set; }
        public DbSet<Randomization> Randomization { get; set; }
        public DbSet<ReportSetting> ReportSetting { get; set; }


        public DbSet<BarcodeGenerate> BarcodeGenerate { get; set; }
        public DbSet<BarcodeSubjectDetail> BarcodeSubjectDetail { get; set; }

        public DbSet<PharmacyVerificationTemplateValue> PharmacyVerificationTemplateValue { get; set; }
        public DbSet<PharmacyVerificationTemplateValueAudit> PharmacyVerificationTemplateValueAudit { get; set; }
        public DbSet<PharmacyVerificationTemplateValueChild> PharmacyVerificationTemplateValueChild { get; set; }
        public DbSet<PharmacyVerificationEntry> PharmacyVerificationEntry { get; set; }
        public DbSet<AttendanceHistory> AttendanceHistory { get; set; }

        public DbSet<MedraConfig> MedraConfig { get; set; }
        public DbSet<MedraVersion> MedraVersion { get; set; }
        public DbSet<ScreeningTemplateLockUnlockAudit> ScreeningTemplateLockUnlockAudit { get; set; }

        public DbSet<Dictionary> Dictionary { get; set; }
        public DbSet<ProjectDesignReportSetting> ProjectDesignReportSetting { get; set; }
        public DbSet<StudyScoping> StudyScoping { get; set; }
        public DbSet<MedraLanguage> MedraLanguage { get; set; }
        public DbSet<MeddraHlgtHltComp> MeddraHlgtHltComp { get; set; }
        public DbSet<MeddraHlgtPrefTerm> MeddraHlgtPrefTerm { get; set; }
        public DbSet<MeddraHltPrefComp> MeddraHltPrefComp { get; set; }
        public DbSet<MeddraHltPrefTerm> MeddraHltPrefTerm { get; set; }
        public DbSet<MeddraLowLevelTerm> MeddraLowLevelTerm { get; set; }
        public DbSet<MeddraMdHierarchy> MeddraMdHierarchy { get; set; }
        public DbSet<MeddraPrefTerm> MeddraPrefTerm { get; set; }
        public DbSet<MeddraSmqContent> MeddraSmqContent { get; set; }
        public DbSet<MeddraSmqList> MeddraSmqList { get; set; }
        public DbSet<MeddraSocHlgtComp> MeddraSocHlgtComp { get; set; }
        public DbSet<MeddraSocIntlOrder> MeddraSocIntlOrder { get; set; }
        public DbSet<MeddraSocTerm> MeddraSocTerm { get; set; }
        public DbSet<MeddraCoding> MeddraCoding { get; set; }
        public DbSet<MeddraCodingComment> MeddraCodingComment { get; set; }
        public DbSet<MeddraCodingAudit> MeddraCodingAudit { get; set; }
        public DbSet<ElectronicSignature> ElectronicSignature { get; set; }
        //public DbSet<EtmfZoneMasterLibrary> EtmfZoneMasterLibrary { get; set; }
        //public DbSet<EtmfSectionMasterLibrary> EtmfSectionMasterLibrary { get; set; }
        public DbSet<EtmfMasterLibrary> EtmfMasterLibrary { get; set; }
        public DbSet<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }

        public DbSet<EtmfProjectWorkPlace> EtmfProjectWorkPlace { get; set; }
        //public DbSet<EtmfProjectWorkPlace> EtmfProjectWorkPlace { get; set; }
        //public DbSet<ProjectWorkplaceArtificate> ProjectWorkplaceArtificate { get; set; }
        //public DbSet<ProjectWorkplaceDetail> ProjectWorkplaceDetail { get; set; }
        //public DbSet<ProjectWorkplaceSection> ProjectWorkplaceSection { get; set; }
        //public DbSet<ProjectWorkPlaceZone> ProjectWorkPlaceZone { get; set; }
        public DbSet<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
        //public DbSet<ProjectWorkplaceSubSection> ProjectWorkplaceSubSection { get; set; }
        //public DbSet<ProjectWorkplaceSubSectionArtifact> ProjectWorkplaceSubSectionArtifact { get; set; }
        public DbSet<ProjectWorkplaceSubSecArtificatedocument> ProjectWorkplaceSubSecArtificatedocument { get; set; }
        public DbSet<InvestigatorContactDetail> InvestigatorContactDetail { get; set; }
        public DbSet<ProjectDesignVisitStatus> ProjectDesignVisitStatus { get; set; }
        public DbSet<Holiday> Holiday { get; set; }
        public DbSet<ManageSite> ManageSite { get; set; }
        public DbSet<Iecirb> Iecirb { get; set; }
        public DbSet<JobMonitoring> JobMonitoring { get; set; }
        public DbSet<PatientStatus> PatientStatus { get; set; }
        public DbSet<VisitStatus> VisitStatus { get; set; }
        public DbSet<ReportScreen> ReportScreen { get; set; }
        public DbSet<ReportFavouriteScreen> ReportFavouriteScreen { get; set; }
        public DbSet<ProjectArtificateDocumentReview> ProjectArtificateDocumentReview { get; set; }
        public DbSet<ProjectArtificateDocumentComment> ProjectArtificateDocumentComment { get; set; }

        public DbSet<ProjectArtificateDocumentHistory> ProjectArtificateDocumentHistory { get; set; }

        //public DbSet<EconsentSetupPatientStatus> EconsentSetupPatientStatus { get; set; }
        public DbSet<EconsentSetup> EconsentSetup { get; set; }
        public DbSet<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        public DbSet<EconsentSectionReference> EconsentSectionReference { get; set; }
        public DbSet<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        public DbSet<EconsentChat> EconsentChat { get; set; }
        //public DbSet<EconsentSetupRoles> EconsentSetupRoles { get; set; }
        public DbSet<RegulatoryType> RegulatoryType { get; set; }
        public DbSet<ProjectArtificateDocumentApprover> ProjectArtificateDocumentApprover { get; set; }
        public DbSet<Site> Site { get; set; }
        public DbSet<ManageSiteRole> ManageSiteRole { get; set; }
        public DbSet<ProjectSubSecArtificateDocumentApprover> ProjectSubSecArtificateDocumentApprover { get; set; }
        public DbSet<ProjectSubSecArtificateDocumentComment> ProjectSubSecArtificateDocumentComment { get; set; }
        public DbSet<ProjectSubSecArtificateDocumentHistory> ProjectSubSecArtificateDocumentHistory { get; set; }
        public DbSet<ProjectSubSecArtificateDocumentReview> ProjectSubSecArtificateDocumentReview { get; set; }

        public DbSet<ProjectModuleRights> ProjectModuleRights { get; set; }
        public DbSet<VisitLanguage> VisitLanguage { get; set; }
        public DbSet<TemplateLanguage> TemplateLanguage { get; set; }
        public DbSet<VariableLanguage> VariableLanguage { get; set; }
        public DbSet<VariableNoteLanguage> VariableNoteLanguage { get; set; }
        public DbSet<TemplateNoteLanguage> TemplateNoteLanguage { get; set; }
        public DbSet<VisitDeviationReport> VisitDeviationReport { get; set; }
        public DbSet<VariableValueLanguage> VariableValueLanguage { get; set; }
        public DbSet<SiteTeam> SiteTeam { get; set; }
        public DbSet<AppScreenPatientRights> AppScreenPatientRights { get; set; }
        public DbSet<AppScreenPatient> AppScreenPatient { get; set; }
        public DbSet<SMSSetting> SMSSetting { get; set; }
        public DbSet<ScreeningTemplateEditCheckValue> ScreeningTemplateEditCheckValue { get; set; }
        public DbSet<VariableCategoryLanguage> VariableCategoryLanguage { get; set; }
        public DbSet<AEReporting> AEReporting { get; set; }
        public DbSet<AdverseEventSettings> AdverseEventSettings { get; set; }
        public DbSet<AEReportingValue> AEReportingValue { get; set; }
        public DbSet<EtmfUserPermission> EtmfUserPermission { get; set; }
        public DbSet<AdverseEventSettingsLanguage> AdverseEventSettingsLanguage { get; set; }
        public DbSet<PhaseManagement> PhaseManagement { get; set; }
        public DbSet<ResourceType> ResourceType { get; set; }
        public DbSet<TaskTemplate> TaskTemplate { get; set; }
        public DbSet<TaskMaster> TaskMaster { get; set; }
        public DbSet<StudyPlan> StudyPlan { get; set; }

        public DbSet<StudyPlanTask> StudyPlanTask { get; set; }

        public DbSet<ProjectDesignVariableEncryptRole> ProjectDesignVariableEncryptRole { get; set; }
        public DbSet<ProjectDesingTemplateRestriction> ProjectDesingTemplateRestriction { get; set; }
        public DbSet<UserSetting> UserSetting { get; set; }
        public DbSet<RandomizationNumberSettings> RandomizationNumberSettings { get; set; }
        public DbSet<ScreeningNumberSettings> ScreeningNumberSettings { get; set; }
        public DbSet<EConsentVideo> EConsentVideo { get; set; }
        public DbSet<DependentTaskDto> DependentTaskDto { get; set; }
        public DbSet<HolidayMaster> HolidayMaster { get; set; }
        public DbSet<WeekEndMaster> WeekEndMaster { get; set; }
        public DbSet<SupplyLocation> SupplyLocation { get; set; }
        public DbSet<CentralDepot> CentralDepot { get; set; }
        public DbSet<StudyPlanTaskResource> StudyPlanTaskResource { get; set; }
        public DbSet<StudyVersion> StudyVersion { get; set; }
        public DbSet<StudyVersionStatus> StudyVersionStatus { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.DefalutMappingValue();
            modelBuilder.DefalutDeleteValueFilter();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<VolunteerQuery> VolunteerQuery { get; set; }
        public DbSet<PharmacyStudyProductType> PharmacyStudyProductType { get; set; }
        public DbSet<ProductReceipt> ProductReceipt { get; set; }
        public DbSet<ProductVerification> ProductVerification { get; set; }
        public DbSet<ProductVerificationDetail> ProductVerificationDetail { get; set; }
        public DbSet<LanguageConfiguration> LanguageConfiguration { get; set; }
        public DbSet<LanguageConfigurationDetails> LanguageConfigurationDetails { get; set; }
        public DbSet<VerificationApprovalTemplate> VerificationApprovalTemplate { get; set; }
        public DbSet<VerificationApprovalTemplateValue> VerificationApprovalTemplateValue { get; set; }
        public DbSet<VerificationApprovalTemplateValueChild> VerificationApprovalTemplateValueChild { get; set; }
        public DbSet<TableFieldName> TableFieldName { get; set; }
        public DbSet<BarcodeCombination> BarcodeCombination { get; set; }
        public DbSet<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; }
        public DbSet<UploadLimit> UploadLimit { get; set; }
        public DbSet<VerificationApprovalTemplateHistory> VerificationApprovalTemplateHistory { get; set; }
        public DbSet<VerificationApprovalTemplateValueAudit> VerificationApprovalTemplateValueAudit { get; set; }
        public DbSet<SupplyManagementConfiguration> SupplyManagementConfiguration { get; set; }
        public DbSet<AttendanceBarcodeGenerate> AttendanceBarcodeGenerate { get; set; }
        public DbSet<BarcodeAudit> BarcodeAudit { get; set; }
        public DbSet<LabManagementConfiguration> LabManagementConfiguration { get; set; }
        public DbSet<LabManagementVariableMapping> LabManagementVariableMapping { get; set; }
        public DbSet<LabManagementUploadData> LabManagementUploadData { get; set; }
        public DbSet<LabManagementUploadExcelData> LabManagementUploadExcelData { get; set; }
        public DbSet<EconsentReviewDetailsAudit> EconsentReviewDetailsAudit { get; set; }

        public DbSet<SyncConfigurationMaster> SyncConfigurationMaster { get; set; }
        public DbSet<SyncConfigurationMasterDetails> SyncConfigurationMasterDetails { get; set; }
        public DbSet<SyncConfigurationMasterDetailsAudit> SyncConfigurationMasterDetailsAudit { get; set; }
        public DbSet<FileSizeConfiguration> FileSizeConfiguration { get; set; }
        public DbSet<SupplyManagementUploadFile> SupplyManagementUploadFile { get; set; }

        public DbSet<LabManagementSendEmailUser> LabManagementSendEmailUser { get; set; }

        public DbSet<CtmsActivity> CtmsActivity { get; set; }
        public DbSet<ProjectSettings> ProjectSettings { get; set; }
        public DbSet<StudyLevelForm> StudyLevelForm { get; set; }
        public DbSet<StudyLevelFormVariable> StudyLevelFormVariable { get; set; }
        public DbSet<StudyLevelFormVariableValue> StudyLevelFormVariableValue { get; set; }
        public DbSet<StudyLevelFormVariableRemarks> StudyLevelFormVariableRemarks { get; set; }

        public DbSet<AdverseEventSettingsDetails> AdverseEventSettingsDetails { get; set; }
        public DbSet<CtmsMonitoring> CtmsMonitoring { get; set; }
        public DbSet<CtmsMonitoringReport> CtmsMonitoringReport { get; set; }
        public DbSet<CtmsMonitoringReportReview> CtmsMonitoringReportReview { get; set; }
        public DbSet<CtmsMonitoringReportVariableValue> CtmsMonitoringReportVariableValue { get; set; }
        public DbSet<CtmsMonitoringReportVariableValueQuery> CtmsMonitoringReportVariableValueQuery { get; set; }
        public DbSet<CtmsMonitoringReportVariableValueAudit> CtmsMonitoringReportVariableValueAudit { get; set; }
        public DbSet<CtmsMonitoringReportVariableValueChild> CtmsMonitoringReportVariableValueChild { get; set; }
        public DbSet<CtmsActionPoint> CtmsActionPoint { get; set; }
        public DbSet<CtmsMonitoringStatus> CtmsMonitoringStatus { get; set; }


        public DbSet<SupplyManagementRequest> SupplyManagementRequest { get; set; }
        public DbSet<SupplyManagementShipment> SupplyManagementShipment { get; set; }
        public DbSet<SupplyManagementReceipt> SupplyManagementReceipt { get; set; }
        public DbSet<SupplyManagementUploadFileVisit> SupplyManagementUploadFileVisit { get; set; }
        public DbSet<SupplyManagementUploadFileDetail> SupplyManagementUploadFileDetail { get; set; }
        public DbSet<PageConfiguration> PageConfiguration { get; set; }
        public DbSet<PageConfigurationFields> PageConfigurationFields { get; set; }

        public DbSet<SendEmailOnVariableChangeSetting> SendEmailOnVariableChangeSetting { get; set; }
        public DbSet<SendEmailOnVariableValue> SendEmailOnVariableValue { get; set; }
        //  public DbSet<KitManagement> KitManagement { get; set; }
        //  public DbSet<DisplayMessageandLableSetting> DisplayMessageandLableSetting { get; set; }
        public DbSet<ScheduleTerminateDetail> ScheduleTerminateDetail { get; set; }
        public DbSet<TemplateVariableSequenceNoSetting> TemplateVariableSequenceNoSetting { get; set; }

        public DbSet<SupplyManagementAllocation> SupplyManagementAllocation { get; set; }

        public DbSet<SupplyManagementKIT> SupplyManagementKIT { get; set; }

        public DbSet<SupplyManagementKITDetail> SupplyManagementKITDetail { get; set; }

        public DbSet<ScreeningEntryStudyHistory> ScreeningEntryStudyHistory { get; set; }
        public DbSet<EconsentGlossary> EconsentGlossary { get; set; }
        public DbSet<ScreeningSetting> ScreeningSetting { get; set; }
        public DbSet<VolunteerFinger> VolunteerFinger { get; set; }

        public DbSet<ProjectStatus> ProjectStatus { get; set; }

        public DbSet<SupplyManagementFector> SupplyManagementFector { get; set; }

        public DbSet<SupplyManagementFectorDetail> SupplyManagementFectorDetail { get; set; }

        public DbSet<SupplyManagementKitAllocationSettings> SupplyManagementKitAllocationSettings { get; set; }

        public DbSet<SupplyManagementKitNumberSettings> SupplyManagementKitNumberSettings { get; set; }

        public DbSet<SupplyManagementVisitKITDetail> SupplyManagementVisitKITDetail { get; set; }

        public DbSet<SupplyManagementKITDetailHistory> SupplyManagementKITDetailHistory { get; set; }

        public DbSet<SupplyManagementKITReturn> SupplyManagementKITReturn { get; set; }

        public DbSet<SupplyManagementKITDiscard> SupplyManagementKITDiscard { get; set; }

        public DbSet<SupplyManagementEmailConfiguration> SupplyManagementEmailConfiguration { get; set; }

        public DbSet<SupplyManagementEmailConfigurationDetail> SupplyManagementEmailConfigurationDetail { get; set; }

        public DbSet<SupplyManagementEmailConfigurationDetailHistory> SupplyManagementEmailConfigurationDetailHistory { get; set; }

        public DbSet<SupplyManagementKITSeries> SupplyManagementKITSeries { get; set; }

        public DbSet<SupplyManagementKITSeriesDetail> SupplyManagementKITSeriesDetail { get; set; }

        public DbSet<SupplyManagementKITSeriesDetailHistory> SupplyManagementKITSeriesDetailHistory { get; set; }

        public DbSet<SupplyManagementVisitKITSequenceDetail> SupplyManagementVisitKITSequenceDetail { get; set; }

        public DbSet<SupplyManagementKITReturnVerification> SupplyManagementKITReturnVerification { get; set; }

        public DbSet<SupplyManagementKITReturnSeries> SupplyManagementKITReturnSeries { get; set; }

        public DbSet<SupplyManagementKITReturnVerificationSeries> SupplyManagementKITReturnVerificationSeries { get; set; }

        public DbSet<SupplyManagementUnblindTreatment> SupplyManagementUnblindTreatment { get; set; }

        public DbSet<SupplyManagementFactorMapping> SupplyManagementFactorMapping { get; set; }
        public DbSet<PKBarcode> PKBarcode { get; set; }
        public DbSet<DossingBarcode> DossingBarcode { get; set; }
        public DbSet<SampleBarcode> SampleBarcode { get; set; }
        public DbSet<ManageSiteAddress> ManageSiteAddress { get; set; }
        public DbSet<ProjectSiteAddress> ProjectSiteAddress { get; set; }
        public DbSet<Centrifugation> Centrifugation { get; set; }
        public DbSet<CentrifugationDetails> CentrifugationDetails { get; set; }
        public DbSet<SampleSeparation> SampleSeparation { get; set; }
        public DbSet<PkBarcodeGenerate> PkBarcodeGenerate { get; set; }
        public DbSet<SampleBarcodeGenerate> SampleBarcodeGenerate { get; set; }
        public DbSet<DossingBarcodeGenerate> DossingBarcodeGenerate { get; set; }

        public DbSet<SupplyManagementApproval> SupplyManagementApproval { get; set; }

        public DbSet<SupplyManagementApprovalDetails> SupplyManagementApprovalDetails { get; set; }

        public DbSet<SupplyManagementShipmentApproval> SupplyManagementShipmentApproval { get; set; }
        public DbSet<VendorManagement> VendorManagement { get; set; }
        public DbSet<PlanMetrics> PlanMetrics { get; set; }
        public DbSet<OverTimeMetrics> OverTimeMetrics { get; set; }

        public DbSet<EmailConfigurationEditCheck> EmailConfigurationEditCheck { get; set; }
        public DbSet<EmailConfigurationEditCheckDetail> EmailConfigurationEditCheckDetail { get; set; }
        public DbSet<EmailConfigurationEditCheckRole> EmailConfigurationEditCheckRole { get; set; }

        public DbSet<EmailConfigurationEditCheckSendMailHistory> EmailConfigurationEditCheckSendMailHistory { get; set; }
        public DbSet<VariableLabelLanguage> VariableLabelLanguage { get; set; }
        public DbSet<RefrenceTypes> RefrenceTypes { get; set; }


        public DbSet<SupplyManagementKitDosePriority> SupplyManagementKitDosePriority { get; set; }
        public DbSet<WorkflowVisit> WorkflowVisit { get; set; }
        public DbSet<WorkflowTemplate> WorkflowTemplate { get; set; }
        public DbSet<ProjectDesignVisitRestriction> ProjectDesignVisitRestriction { get; set; }
        public DbSet<VisitEmailConfiguration> VisitEmailConfiguration { get; set; }
        public DbSet<VisitEmailConfigurationRoles> VisitEmailConfigurationRoles { get; set; }
        public DbSet<LettersFormate> LettersFormate { get; set; }
        public DbSet<LettersActivity> LettersActivity { get; set; }
        public DbSet<LabReport> LabReport { get; set; }
        public DbSet<WorkingDay> WorkingDay { get; set; }
        public DbSet<SiteTypes> SiteTypes { get; set; }
        public DbSet<IDVerification> IDVerification { get; set; }
        public DbSet<IDVerificationFile> IDVerificationFile { get; set; }
        public DbSet<Designation> Designation { get; set; }
        public DbSet<StudyPlanResource> StudyPlanResource { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<UserAccess> UserAccess { get; set; }
        public DbSet<SupplyManagementKitNumberSettingsRole> SupplyManagementKitNumberSettingsRole { get; set; }
        public DbSet<PharmacyBarcodeConfig> PharmacyBarcodeConfig { get; set; }
        public DbSet<PharmacyBarcodeDisplayInfo> PharmacyBarcodeDisplayInfo { get; set; }
        public DbSet<CurrencyRate> CurrencyRate { get; set; }
        public DbSet<Procedure> Procedure { get; set; }
        public DbSet<PatientCost> PatientCost { get; set; }
        public DbSet<PassThroughCostActivity> PassThroughCostActivity { get; set; }
        public DbSet<PassThroughCost> PassThroughCost { get; set; }
        public DbSet<SupplyManagementEmailScheduleLog> SupplyManagementEmailScheduleLog { get; set; }

        public DbSet<SupplyManagementThresholdHistory> SupplyManagementThresholdHistory { get; set; }
        public DbSet<ProjectDesignTemplateSiteAccess> ProjectDesignTemplateSiteAccess { get; set; }
        public DbSet<PaymentMilestone> PaymentMilestone { get; set; }
        public DbSet<PaymentMilestoneTaskDetail> PaymentMilestoneTaskDetail { get; set; }

        public DbSet<BudgetPaymentFinalCost> BudgetPaymentFinalCost { get; set; }

    }
}