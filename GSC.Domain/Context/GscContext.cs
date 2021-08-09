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
using GSC.Data.Dto.UserMgt;
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
        public DbSet<EtmfZoneMasterLibrary> EtmfZoneMasterLibrary { get; set; }
        public DbSet<EtmfSectionMasterLibrary> EtmfSectionMasterLibrary { get; set; }
        public DbSet<EtmfArtificateMasterLbrary> EtmfArtificateMasterLbrary { get; set; }

        public DbSet<ProjectWorkplace> ProjectWorkplace { get; set; }
        public DbSet<ProjectWorkplaceArtificate> ProjectWorkplaceArtificate { get; set; }
        public DbSet<ProjectWorkplaceDetail> ProjectWorkplaceDetail { get; set; }
        public DbSet<ProjectWorkplaceSection> ProjectWorkplaceSection { get; set; }
        public DbSet<ProjectWorkPlaceZone> ProjectWorkPlaceZone { get; set; }
        public DbSet<ProjectWorkplaceArtificatedocument> ProjectWorkplaceArtificatedocument { get; set; }
        public DbSet<ProjectWorkplaceSubSection> ProjectWorkplaceSubSection { get; set; }
        public DbSet<ProjectWorkplaceSubSectionArtifact> ProjectWorkplaceSubSectionArtifact { get; set; }
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

        public DbSet<EconsentSetupPatientStatus> EconsentSetupPatientStatus { get; set; }
        public DbSet<EconsentSetup> EconsentSetup { get; set; }
        public DbSet<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        public DbSet<EconsentSectionReference> EconsentSectionReference { get; set; }
        public DbSet<EconsentReviewDetailsSections> EconsentReviewDetailsSections { get; set; }
        public DbSet<EconsentChat> EconsentChat { get; set; }
        public DbSet<EconsentSetupRoles> EconsentSetupRoles { get; set; }
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
        public DbSet<ManageMonitoring> ManageMonitoring { get; set; }
        public DbSet<DependentTaskDto> DependentTaskDto { get; set; }
        public DbSet<HolidayMaster> HolidayMaster { get; set; }
        public DbSet<WeekEndMaster> WeekEndMaster { get; set; }
        public DbSet<SupplyLocation> SupplyLocation { get; set; }
        public DbSet<CentralDepot> CentralDepot { get; set; }
        public DbSet<StudyPlanTaskResource> StudyPlanTaskResource { get; set; }
        public DbSet<StudyVersion> StudyVersion { get; set; }
        public DbSet<StudyVerionVisitStatus> StudyVerionVisitStatus { get; set; }
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
        public DbSet<TableFieldName> TableFieldName { get; set; }
        public DbSet<BarcodeCombination> BarcodeCombination { get; set; }
        public DbSet<BarcodeDisplayInfo> BarcodeDisplayInfo { get; set; }

    }


}