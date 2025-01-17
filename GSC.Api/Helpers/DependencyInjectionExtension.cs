﻿using GSC.Audit;
using GSC.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Report;
using GSC.Report.Common;
using GSC.Respository.Attendance;
using GSC.Respository.Audit;
using GSC.Respository.Barcode;
using GSC.Respository.Barcode.Generate;
using GSC.Respository.Client;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.EditCheckImpact;
using GSC.Respository.EmailSender;
using GSC.Respository.Etmf;
using GSC.Respository.InformConcent;
using GSC.Respository.LogReport;
using GSC.Respository.Master;
using GSC.Respository.Medra;
using GSC.Respository.Pharmacy;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using GSC.Respository.Project.Schedule;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Respository.PropertyMapping;
using GSC.Respository.Reports;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GSC.Respository.Project.Rights;
using GSC.Shared.Extension;
using GSC.Shared.Email;
using GSC.Shared.JWTAuth;
using GSC.Shared.Caching;
using GSC.Common.Common;
using GSC.Respository.LanguageSetup;
using GSC.Respository.AdverseEvent;
using GSC.Respository.CTMS;
using GSC.Respository.SupplyManagement;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.LabManagement;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Respository.LabReportManagement;
using GSC.Respository.IDVerificationSystem;
using GSC.Respository.FirebaseNotification;
using GSC.Data.Entities.CTMS;

namespace GSC.Api.Helpers
{
    public static class DependencyInjectionExtension
    {
        public static void AddDependencyInjection<TContext>(this IServiceCollection services, IConfiguration configuration) where TContext : IContext
        {
            var connectionString = configuration["connectionStrings:dbConnectionString"];
            services.AddDbContext<GscContext>(o => o.UseSqlServer(connectionString));
            services.AddScoped<IGSCContext, GscContext>();
            services.AddScoped<IGSCContextExtension, GscContext>();
            services.AddScoped<ICommonSharedService, CommonSharedService>();
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped(typeof(IUnitOfWork<TContext>), typeof(UnitOfWork<TContext>));

            services.AddScoped<IPropertyMappingService, PropertyMappingService>();
            services.AddScoped<IAppUserClaimRepository, AppUserClaimRepository>();
            services.AddScoped<ITypeHelperService, TypeHelperService>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtTokenAccesser, JwtTokenAccesser>();
            services.AddScoped<IContactTypeRepository, ContactTypeRepository>();
            services.AddScoped<IFoodTypeRepository, FoodTypeRepository>();
            services.AddScoped<IMaritalStatusRepository, MaritalStatusRepository>();
            services.AddScoped<IOccupationRepository, OccupationRepository>();
            services.AddScoped<IRaceRepository, RaceRepository>();
            services.AddScoped<IReligionRepository, ReligionRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IDomainClassRepository, DomainClassRepository>();
            services.AddScoped<IDrugRepository, DrugRepository>();
            services.AddScoped<IFreezerRepository, FreezerRepository>();
            services.AddScoped<ICityRepository, CityRepository>();
            services.AddScoped<IStateRepository, StateRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ILanguageRepository, LanguageRepository>();
            services.AddScoped<IScopeNameRepository, ScopeNameRepository>();
            services.AddScoped<IVariableRepository, VariableRepository>();
            services.AddScoped<IVariableValueRepository, VariableValueRepository>();
            services.AddScoped<IVariableRemarksRepository, VariableRemarksRepository>();
            services.AddScoped<IVolunteerAddressRepository, VolunteerAddressRepository>();
            services.AddScoped<IVolunteerBiometricRepository, VolunteerBiometricRepository>();
            services.AddScoped<IVolunteerContactRepository, VolunteerContactRepository>();
            services.AddScoped<IVolunteerDocumentRepository, VolunteerDocumentRepository>();
            services.AddScoped<IVolunteerFoodRepository, VolunteerFoodRepository>();
            services.AddScoped<IVolunteerHistoryRepository, VolunteerHistoryRepository>();
            services.AddScoped<IVolunteerLanguageRepository, VolunteerLanguageRepository>();
            services.AddScoped<IVolunteerRepository, VolunteerRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IVolunteerImageRepository, VolunteerImageRepository>();
            services.AddScoped<IUserImageRepository, UserImageRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IEmailSettingRepository, EmailSettingRepository>();
            services.AddScoped<IAppScreenRepository, AppScreenRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IUploadSettingRepository, UploadSettingRepository>();
            services.AddScoped<ILoginPreferenceRepository, LoginPreferenceRepository>();
            services.AddScoped<IUserLoginReportRespository, UserLoginReportRespository>();
            services.AddScoped<IUserPasswordRepository, UserPasswordRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IEmailSenderRespository, EmailSenderRespository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IClientAddressRepository, ClientAddressRepository>();
            services.AddScoped<IClientContactRepository, ClientContactRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IDesignTrialRepository, DesignTrialRepository>();
            services.AddScoped<ITrialTypeRepository, TrialTypeRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IProductTypeRepository, ProductTypeRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IPopulationTypeRepository, PopulationTypeRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<ITestGroupRepository, TestGroupRepository>();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<IDesignTrialRepository, DesignTrialRepository>();
            services.AddScoped<ITrialTypeRepository, TrialTypeRepository>();
            services.AddScoped<IUserOtpRepository, UserOtpRepository>();
            services.AddScoped<IVariableCategoryRepository, VariableCategoryRepository>();
            services.AddScoped<IAnnotationTypeRepository, AnnotationTypeRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
            services.AddScoped<ICityAreaRepository, CityAreaRepository>();
            services.AddScoped<INumberFormatRepository, NumberFormatRepository>();
            services.AddScoped<IVariableTemplateDetailRepository, VariableTemplateDetailRepository>();
            services.AddScoped<IVariableTemplateRepository, VariableTemplateRepository>();
            services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
            services.AddScoped<IAuditReasonRepository, AuditReasonRepository>();
            services.AddScoped<IVolunteerAuditTrailRepository, VolunteerAuditTrailRepository>();
            services.AddScoped<IAppSettingRepository, AppSettingRepository>();
            services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();
            services.AddScoped<IProjectDesignRepository, ProjectDesignRepository>();
            services.AddScoped<IProjectDesignPeriodRepository, ProjectDesignPeriodRepository>();
            services.AddScoped<IProjectDesignVisitRepository, ProjectDesignVisitRepository>();
            services.AddScoped<IProjectDesignTemplateRepository, ProjectDesignTemplateRepository>();
            services.AddScoped<IProjectDesignVariableRepository, ProjectDesignVariableRepository>();
            services.AddScoped<IProjectDesignVariableValueRepository, ProjectDesignVariableValueRepository>();
            services.AddScoped<IProjectDesignVariableRemarksRepository, ProjectDesignVariableRemarksRepository>();
            services.AddScoped<IProjectRightRepository, ProjectRightRepository>();
            services.AddScoped<IProjectDocumentRepository, ProjectDocumentRepository>();
            services.AddScoped<IProjectDocumentReviewRepository, ProjectDocumentReviewRepository>();
            services.AddScoped<IProjectWorkflowRepository, ProjectWorkflowRepository>();
            services.AddScoped<IProjectWorkflowIndependentRepository, ProjectWorkflowIndependentRepository>();
            services.AddScoped<IProjectWorkflowLevelRepository, ProjectWorkflowLevelRepository>();
            services.AddScoped<IProjectScheduleRepository, ProjectScheduleRepository>();
            services.AddScoped<IProjectScheduleTemplateRepository, ProjectScheduleTemplateRepository>();
            services.AddScoped<IVariableTemplateNoteRepository, VariableTemplateNoteRepository>();
            services.AddScoped<IUserFavoriteScreenRepository, UserFavoriteScreenRepository>();
            services.AddScoped<IScreeningEntryRepository, ScreeningEntryRepository>();
            services.AddScoped<IDataEntryRespository, DataEntryRespository>();
            services.AddScoped<IScreeningTemplateRepository, ScreeningTemplateRepository>();
            services.AddScoped<IScreeningTemplateValueRepository, ScreeningTemplateValueRepository>();
            services.AddScoped<IAttendanceRepository, AttendanceRepository>();
            services.AddScoped<IVolunteerBlockHistoryRepository, VolunteerBlockHistoryRepository>();
            services.AddScoped<ITemplateRightsRepository, TemplateRightsRepository>();
            services.AddScoped<ITemplateRightsRoleListRepository, TemplateRightsRoleListRepository>();
            services.AddScoped<IBlockCategoryRepository, BlockCategoryRepository>();
            services.AddScoped<IUserRecentItemRepository, UserRecentItemRepository>();
            services.AddScoped<IScreeningHistoryRepository, ScreeningHistoryRepository>();
            services.AddScoped<IScreeningVisitRepository, ScreeningVisitRepository>();
            services.AddScoped<IScreeningVisitHistoryRepository, ScreeningVisitHistoryRepository>();
            services.AddScoped<IScreeningTemplateValueCommentRepository, ScreeningTemplateValueCommentRepository>();
            services.AddScoped<IScreeningTemplateValueAuditRepository, ScreeningTemplateValueAuditRepository>();
            services.AddScoped<IScreeningTemplateValueQueryRepository, ScreeningTemplateValueQueryRepository>();
            services.AddScoped<IScreeningTemplateValueChildRepository, ScreeningTemplateValueChildRepository>();
            services.AddScoped<IScreeningTemplateRemarksChildRepository, ScreeningTemplateRemarksChildRepository>();
            services.AddScoped<IUserGridSettingRepository, UserGridSettingRepository>();
            services.AddScoped<IScreeningTemplateReviewRepository, ScreeningTemplateReviewRepository>();
            services.AddScoped<IDocumentNameRepository, DocumentNameRepository>();
            services.AddScoped<IClientTypeRepository, ClientTypeRepository>();
            services.AddScoped<IProductFomRepository, ProductFormRepository>();
            services.AddScoped<IPharmacyConfigRepository, PharmacyConfigRepository>();
            services.AddScoped<IBarcodeTypeRepository, BarcodeTypeRepository>();
            services.AddScoped<IPharmacyTemplateValueRepository, PharmacyTemplateValueRepository>();
            services.AddScoped<IPharmacyTemplateValueAuditRepository, PharmacyTemplateValueAuditRepository>();
            services.AddScoped<IPharmacyEntryRepository, PharmacyEntryRepository>();
            services.AddScoped<IBarcodeConfigRepository, BarcodeConfigRepository>();
            services.AddScoped<IProjectSubjectRepository, ProjectSubjectRepository>();
            services.AddScoped<IBarcodeGenerateRepository, BarcodeGenerateRepository>();
            services.AddScoped<IBarcodeSubjectDetailRepository, BarcodeSubjectDetailRepository>();
            services.AddScoped<IRandomizationRepository, RandomizationRepository>();
            services.AddScoped<IEditCheckRepository, EditCheckRepository>();
            services.AddScoped<IEditCheckImpactRepository, EditCheckImpactRepository>();
            services.AddScoped<IEditCheckRuleRepository, EditCheckRuleRepository>();
            services.AddScoped<IScheduleRuleRespository, ScheduleRuleRespository>();
            services.AddScoped<IImpactService, ImpactService>();
            services.AddScoped<IEditCheckDetailRepository, EditCheckDetailRepository>();
            services.AddScoped<IProjectDesignReportSettingRepository, ProjectDesignReportSettingRepository>();
            services.AddScoped<IPharmacyVerificationTemplateValueRepository, PharmacyVerificationTemplateValueRepository>();
            services.AddScoped<IPharmacyVerificationEntryRepository, PharmacyVerificationEntryRepository>();
            services.AddScoped<IAttendanceHistoryRepository, AttendanceHistoryRepository>();

            services.AddScoped<IInvestigatorContactRepository, InvestigatorContactRepository>();
            services.AddScoped<IReportSettingRepository, ReportSettingRepository>();
            services.AddScoped<IProjectWorkplaceArtificatedocumentRepository, ProjectWorkplaceArtificatedocumentRepository>();

            services.AddScoped<IReportBaseRepository, ReportBaseRepository>();
            services.AddScoped<IGscReport, GscReport>();

            services.AddScoped<IMedraConfigRepository, MedraConfigRepository>();
            services.AddScoped<IDictionaryRepository, DictionaryRepository>();
            services.AddScoped<IMedraConfigCommonRepository, MedraConfigCommonRepository>();
            services.AddScoped<IStudyScopingRepository, StudyScopingRepository>();
            services.AddScoped<IMedraLanguageRepository, MedraLanguageRepository>();
            services.AddScoped<IMedraVersionRepository, MedraVersionRepository>();
            services.AddScoped<IScreeningTemplateLockUnlockRepository, ScreeningTemplateLockUnlockRepository>();

            services.AddScoped<IMeddraHlgtHltCompRepository, MeddraHlgtHltCompRepository>();
            services.AddScoped<IMeddraHlgtPrefTermRepository, MeddraHlgtPrefTermRepository>();
            services.AddScoped<IMeddraHltPrefCompRepository, MeddraHltPrefCompRepository>();
            services.AddScoped<IMeddraHltPrefTermRepository, MeddraHltPrefTermRepository>();
            services.AddScoped<IMeddraLowLevelTermRepository, MeddraLowLevelTermRepository>();
            services.AddScoped<IMeddraMdHierarchyRepository, MeddraMdHierarchyRepository>();
            services.AddScoped<IMeddraPrefTermRepository, MeddraPrefTermRepository>();
            services.AddScoped<IMeddraSmqContentRepository, MeddraSmqContentRepository>();
            services.AddScoped<IMeddraSmqListRepository, MeddraSmqListRepository>();
            services.AddScoped<IMeddraSocHlgtCompRepository, MeddraSocHlgtCompRepository>();
            services.AddScoped<IMeddraSocIntlOrderRepository, MeddraSocIntlOrderRepository>();
            services.AddScoped<IMeddraSocTermRepository, MeddraSocTermRepository>();
            services.AddScoped<IMeddraCodingRepository, MeddraCodingRepository>();
            services.AddScoped<IMeddraCodingCommentRepository, MeddraCodingCommentRepository>();
            services.AddScoped<IMeddraCodingAuditRepository, MeddraCodingAuditRepository>();
            services.AddScoped<IScreeningProgress, ScreeningProgress>();
            services.AddScoped<IEditCheckFormulaRepository, EditCheckFormulaRepository>();           
            services.AddScoped<IEtmfMasterLbraryRepository, EtmfMasterLbraryRepository>();
            services.AddScoped<IEtmfArtificateMasterLbraryRepository, EtmfArtificateMasterLbraryRepository>();
            services.AddScoped<IETMFWorkplaceRepository, ETMFWorkplaceRepository>();
            services.AddScoped<IProjectWorkPlaceZoneRepository, ProjectWorkPlaceZoneRepository>();
            services.AddScoped<IProjectWorkplaceSectionRepository, ProjectWorkplaceSectionRepository>();
            services.AddScoped<IProjectWorkplaceArtificateRepository, ProjectWorkplaceArtificateRepository>();
            services.AddScoped<IProjectWorkplaceDetailRepository, ProjectWorkplaceDetailRepository>();
            services.AddScoped<IProjectWorkplaceSubSectionRepository, ProjectWorkplaceSubSectionRepository>();
            services.AddScoped<IProjectWorkplaceSubSectionArtifactRepository, ProjectWorkplaceSubSectionArtifactRepository>();
            services.AddScoped<IProjectWorkplaceSubSecArtificatedocumentRepository, ProjectWorkplaceSubSecArtificatedocumentRepository>();
            services.AddScoped<IInvestigatorContactDetailRepository, InvestigatorContactDetailRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();
            services.AddScoped<IManageSiteRepository, ManageSiteRepository>();
            services.AddScoped<IIecirbRepository, IecirbRepository>();
            services.AddScoped<IJobMonitoringRepository, JobMonitoringRepository>();
            services.AddScoped<IPatientStatusRepository, PatientStatusRepository>();
            services.AddScoped<IVisitStatusRepository, VisitStatusRepository>();
            services.AddScoped<IReportFavouriteScreenRepository, ReportFavouriteScreenRepository>();
            services.AddScoped<IReportScreenRepository, ReportScreenRepository>();
            services.AddScoped<IProjectWorkplaceArtificateDocumentReviewRepository, ProjectWorkplaceArtificateDocumentReviewRepository>();
            services.AddScoped<IProjectArtificateDocumentCommentRepository, ProjectArtificateDocumentCommentRepository>();
            services.AddScoped<IProjectArtificateDocumentHistoryRepository, ProjectArtificateDocumentHistoryRepository>();
            services.AddScoped<IProjectDesignVisitStatusRepository, ProjectDesignVisitStatusRepository>();
            services.AddScoped<IAuditTracker, AuditTracker>();
            services.AddSingleton<IDictionaryCollection, DictionaryCollection>();
            services.AddScoped<IEconsentReviewDetailsRepository, EconsentReviewDetailsRepository>();
            services.AddScoped<IEconsentSetupRepository, EconsentSetupRepository>();
            services.AddScoped<IRegulatoryTypeRepository, RegulatoryTypeRepository>();
            services.AddScoped<IProjectArtificateDocumentApproverRepository, ProjectArtificateDocumentApproverRepository>();            
            services.AddScoped<IEconsentSectionReferenceRepository, EconsentSectionReferenceRepository>();
            services.AddScoped<IEconsentReviewDetailsSectionsRepository, EconsentReviewDetailsSectionsRepository>();
            services.AddScoped<IEconsentChatRepository, EconsentChatRepository>();            
            services.AddScoped<ISiteRepository, SiteRepository>();
            services.AddScoped<IProjectSubSecArtificateDocumentHistoryRepository, ProjectSubSecArtificateDocumentHistoryRepository>();
            services.AddScoped<IProjectSubSecArtificateDocumentReviewRepository, ProjectSubSecArtificateDocumentReviewRepository>();
            services.AddScoped<IProjectSubSecArtificateDocumentApproverRepository, ProjectSubSecArtificateDocumentApproverRepository>();
            services.AddScoped<IProjectSubSecArtificateDocumentCommentRepository, ProjectSubSecArtificateDocumentCommentRepository>();
            services.AddScoped<IProjectModuleRightsRepository, ProjectModuleRightsRepository>();
            services.AddScoped<IProjectDesignTemplateNoteRepository, ProjectDesignTemplateNoteRepository>();
            services.AddScoped<IVisitLanguageRepository, VisitLanguageRepository>();
            services.AddScoped<ITemplateLanguageRepository, TemplateLanguageRepository>();
            services.AddScoped<IVariabeLanguageRepository, VariabeLanguageRepository>();
            services.AddScoped<IVariabeNoteLanguageRepository, VariabeNoteLanguageRepository>();
            services.AddScoped<ITemplateNoteLanguageRepository, TemplateNoteLanguageRepository>();
            services.AddScoped<IVariabeValueLanguageRepository, VariabeValueLanguageRepository>();
            services.AddHttpClient<ICentreUserService, CentreUserService>();
            services.AddHttpClient<IEmailSenderRespository, EmailSenderRespository>();
            services.AddMemoryCache();
            services.AddSingleton(typeof(IGSCCaching), typeof(GSCCaching));
            services.AddScoped<ISiteTeamRepository, SiteTeamRepository>();
            services.AddScoped<IAppScreenPatientRightsRepository, AppScreenPatientRightsRepository>();
            services.AddScoped<ISMSSettingRepository, SMSSettingRepository>();
            services.AddScoped<IScreeningTemplateEditCheckValueRepository, ScreeningTemplateEditCheckValueRepository>();
            services.AddScoped<IReportSyncfusion, ReportSyncfusion>();
            services.AddScoped<IVariableCategoryLanguageRepository, VariableCategoryLanguageRepository>();
            services.AddScoped<IAEReportingRepository, AEReportingRepository>();
            services.AddScoped<IAdverseEventSettingsRepository, AdverseEventSettingsRepository>();
            services.AddScoped<IAEReportingValueRepository, AEReportingValueRepository>();
            services.AddScoped<IAdverseEventSettingsLanguageRepository, AdverseEventSettingsLanguageRepository>();
            services.AddScoped<IPhaseManagementRepository, PhaseManagementRepository>();
            services.AddScoped<IResourceTypeRepository, ResourceTypeRepository>();
            services.AddScoped<IEtmfUserPermissionRepository, EtmfUserPermissionRepository>();
            services.AddScoped<ITaskTemplateRepository, TaskTemplateRepository>();
            services.AddScoped<ITaskMasterRepository, TaskMasterRepository>();
            services.AddScoped<IStudyPlanRepository, StudyPlanRepository>();
            services.AddScoped<IStudyPlanTaskRepository, StudyPlanTaskRepository>();
            services.AddScoped<IPdfViewerRepository, PdfViewerRepository>();
            services.AddScoped<IProjectDesignVariableEncryptRoleRepository, ProjectDesignVariableEncryptRoleRepository>();
            services.AddScoped<IProjectDesingTemplateRestrictionRepository, ProjectDesingTemplateRestrictionRepository>();
            services.AddScoped<IUserSettingRepository, UserSettingRepository>();
            services.AddScoped<IScreeningNumberSettingsRepository, ScreeningNumberSettingsRepository>();
            services.AddScoped<IRandomizationNumberSettingsRepository, RandomizationNumberSettingsRepository>();
            services.AddScoped<IEConsentVideoRepository, EConsentVideoRepository>();
            services.AddScoped<IHolidayMasterRepository, HolidayMasterRepository>();
            services.AddScoped<IWeekEndMasterRepository, WeekEndMasterRepository>();
            services.AddScoped<ISupplyLocationRepository, SupplyLocationRepository>();
            services.AddScoped<ICentralDepotRepository, CentralDepotRepository>();
            services.AddScoped<IStudyVersionRepository, StudyVersionRepository>();
            services.AddScoped<IStudyVersionStatusRepository, StudyVersionStatusRepository>();
            services.AddScoped<IVolunteerSummaryReport, VolunteerSummaryReport>();
            services.AddScoped<IVolunteerQueryRepository, VolunteerQueryRepository>();
            services.AddScoped<IPharmacyStudyProductTypeRepository, PharmacyStudyProductTypeRepository>();
            services.AddScoped<IProductReceiptRepository, ProductReceiptRepository>();
            services.AddScoped<IStudyPlanTaskResourceRepository, StudyPlanTaskResourceRepository>();
            services.AddScoped<IProductVerificationRepository, ProductVerificationRepository>();
            services.AddScoped<IProductVerificationDetailRepository, ProductVerificationDetailRepository>();
            services.AddScoped<IProductVerificationReport, ProductVerificationReport>();
            services.AddScoped<ILanguageConfigurationRepository, LanguageConfigurationRepository>();
            services.AddScoped<IBarcodeCombinationRepository, BarcodeCombinationRepository>();
            services.AddScoped<IBarcodeDisplayInfoRepository, BarcodeDisplayInfoRepository>();
            services.AddScoped<IUploadlimitRepository, UploadLimitRepository>();
            services.AddScoped<IVerificationApprovalTemplateRepository, VerificationApprovalTemplateRepository>();
            services.AddScoped<IVerificationApprovalTemplateValueRepository, VerificationApprovalTemplateValueRepository>();
            services.AddScoped<IVerificationApprovalTemplateHistoryRepository, VerificationApprovalTemplateHistoryRepository>();
            services.AddScoped<IVerificationApprovalTemplateValueChildRepository, VerificationApprovalTemplateValueChildRepository>();
            services.AddScoped<IVerificationApprovalTemplateValueAuditRepository, VerificationApprovalTemplateValueAuditRepository>();
            services.AddScoped<ISupplyManagementConfigurationRepository, SupplyManagementConfigurationRepository>();
            services.AddScoped<IAttendanceBarcodeGenerateRepository, AttendanceBarcodeGenerateRepository>();
            services.AddScoped<IBarcodeAuditRepository, BarcodeAuditRepository>();
            services.AddScoped<IVersionEffectRepository, VersionEffectRepository>();
            services.AddScoped<ILabManagementConfigurationRepository, LabManagementConfigurationRepository>();
            services.AddScoped<ILabManagementVariableMappingRepository, LabManagementVariableMappingRepository>();
            services.AddScoped<ILabManagementUploadDataRepository, LabManagementUploadDataRepository>();
            services.AddScoped<ILabManagementUploadExcelDataRepository, LabManagementUploadExcelDataRepository>();
            services.AddScoped<IEconsentReviewDetailsAuditRepository, EconsentReviewDetailsAuditRepository>();
            services.AddScoped<ISyncConfigurationMasterRepository, SyncConfigurationMasterRepository>();
            services.AddScoped<ISyncConfigurationMasterDetailsRepository, SyncConfigurationMasterDetailsRepository>();
            services.AddScoped<ISyncConfigurationMasterDetailsRepositoryAudit, SyncConfigurationMasterDetailsRepositoryAudit>();
            services.AddScoped<ISupplyManagementUploadFileRepository, SupplyManagementUploadFileRepository>();
            services.AddScoped<ILabManagementSendEmailUserRepository, LabManagementSendEmailUserRepository>();
            services.AddScoped<ICtmsActivityRepository, CtmsActivityRepository>();
            services.AddScoped<IProjectSettingsRepository, ProjectSettingsRepository>();
            services.AddScoped<IStudyLevelFormRepository, StudyLevelFormRepository>();
            services.AddScoped<IStudyLevelFormVariableRepository, StudyLevelFormVariableRepository>();
            services.AddScoped<IStudyLevelFormVariableValueRepository, StudyLevelFormVariableValueRepository>();
            services.AddScoped<IStudyLevelFormVariableRemarksRepository, StudyLevelFormVariableRemarksRepository>();
            services.AddScoped<IAdverseEventSettingsDetailRepository, AdverseEventSettingsDetailRepository>();
            services.AddScoped<ICtmsMonitoringRepository, CtmsMonitoringRepository>();
            services.AddScoped<ICtmsMonitoringReportRepository, CtmsMonitoringReportRepository>();
            services.AddScoped<ICtmsMonitoringReportReviewRepository, CtmsMonitoringReportReviewRepository>();
            services.AddScoped<ICtmsMonitoringReportVariableValueRepository, CtmsMonitoringReportVariableValueRepository>();
            services.AddScoped<ICtmsMonitoringReportVariableValueQueryRepository, CtmsMonitoringReportVariableValueQueryRepository>();
            services.AddScoped<ICtmsMonitoringReportVariableValueAuditRepository, CtmsMonitoringReportVariableValueAuditRepository>();
            services.AddScoped<ICtmsMonitoringReportVariableValueChildRepository, CtmsMonitoringReportVariableValueChildRepository>();
            services.AddScoped<ICtmsActionPointRepository, CtmsActionPointRepository>();
            services.AddScoped<ICtmsMonitoringStatusRepository, CtmsMonitoringStatusRepository>();
            services.AddScoped<ISupplyManagementRequestRepository, SupplyManagementRequestRepository>();
            services.AddScoped<ISupplyManagementShipmentRepository, SupplyManagementShipmentRepository>();
            services.AddScoped<ISupplyManagementReceiptRepository, SupplyManagementReceiptRepository>();
            services.AddScoped<ISupplyManagementUploadFileVisitRepository, SupplyManagementUploadFileVisitRepository>();
            services.AddScoped<ISupplyManagementUploadFileDetailRepository, SupplyManagementUploadFileDetailRepository>();
            services.AddScoped<IPageConfigurationRepository, PageConfigurationRepository>();
            services.AddScoped<IPageConfigurationFieldsRepository, PageConfigurationFieldsRepository>();
            services.AddHttpClient<IProjectDataRemoveService, ProjectDataRemoveService>();            
            services.AddScoped<ISendEmailOnVariableChangeSettingRepository, SendEmailOnVariableChangeSettingRepository>();           
            services.AddScoped<IScheduleTerminateDetailRepository, ScheduleTerminateDetailRepository>();
            services.AddScoped<IScheduleTerminate, ScheduleTerminateRepository>();
            services.AddScoped<ITemplateVariableSequenceNoSettingRepository, TemplateVariableSequenceNoSettingRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<ISupplyManagementAllocationRepository, SupplyManagementAllocationRepository>();
            services.AddScoped<ISupplyManagementKitRepository, SupplyManagementKitRepository>();
            services.AddScoped<ISupplyManagementKitDetailRepository, SupplyManagementKitDetailRepository>();

            services.AddScoped<IEconsentGlossaryRepository, EconsentGlossaryRepository>();
            services.AddScoped<IScreeningSettingRepository, ScreeningSettingRepository>();
            services.AddScoped<IVolunteerFingerRepository, VolunteerFingerRepository>();
            services.AddScoped<ISupplyManagementFectorRepository, SupplyManagementFectorRepository>();
            services.AddScoped<ISupplyManagementFectorDetailRepository, SupplyManagementFectorDetailRepository>();
            services.AddScoped<ISupplyManagementKitAllocationSettingsRepository, SupplyManagementKitAllocationSettingsRepository>();
            services.AddScoped<ISupplyManagementKitNumberSettingsRepository, SupplyManagementKitNumberSettingsRepository>();
            services.AddScoped<ISupplyManagementEmailConfigurationRepository, SupplyManagementEmailConfigurationRepository>();
            services.AddScoped<ISupplyManagementKitSeriesRepository, SupplyManagementKitSeriesRepository>();
            services.AddScoped<ISupplyManagementFactorMappingRepository, SupplyManagementFactorMappingRepository>();
            services.AddScoped<IPKBarcodeRepository, PKBarcodeRepository>();
            services.AddScoped<ISampleBarcodeRepository, SampleBarcodeRepository>();
            services.AddScoped<IDossingBarcodeRepository, DossingBarcodeRepository>();
            services.AddScoped<IPharmacyReportRepository, PharmacyReportRepository>();
            services.AddScoped<IVersionEffectWithEditCheck, VersionEffectWithEditCheck>();
            services.AddScoped<IManageSiteAddressRepository, ManageSiteAddressRepository>();
            services.AddScoped<IProjectSiteAddressRepository, ProjectSiteAddressRepository>();            
            services.AddScoped<ICentrifugationRepository, CentrifugationRepository>();
            services.AddScoped<ICentrifugationDetailsRepository, CentrifugationDetailsRepository>();
            services.AddScoped<ISampleSeparationRepository, SampleSeparationRepository>();
            services.AddScoped<IPkBarcodeGenerateRepository, PkBarcodeGenerateRepository>();
            services.AddScoped<ISampleBarcodeGenerateRepository, SampleBarcodeGenerateRepository>();
            services.AddScoped<IDossingBarcodeGenerateRepository, DossingBarcodeGenerateRepository>();
            services.AddScoped<ISupplyManagementApprovalRepository, SupplyManagementApprovalRepository>();
            services.AddScoped<IVendorManagementRepository, VendorManagementRepository>();
            services.AddScoped<IMetricsRepository, MetricsRepository>();
            services.AddScoped<IOverTimeMetricsRepository, OverTimeMetricsRepository>();
            services.AddScoped<IEmailConfigurationEditCheckRepository, EmailConfigurationEditCheckRepository>();
            services.AddScoped<IEmailConfigurationEditCheckDetailRepository, EmailConfigurationEditCheckDetailRepository>();
            services.AddScoped<IEmailConfigurationEditCheckRoleRepository, EmailConfigurationEditCheckRoleRepository>();
            services.AddScoped<IVariabeLabelLanguageRepository, VariabeLabelLanguageRepository>();
            services.AddScoped<IWorkflowVisitRepository, WorkflowVisitRepository>();
            services.AddScoped<IWorkflowTemplateRepository, WorkflowTemplateRepository>();
            services.AddScoped<IProjectDesignVisitRestrictionRepository, ProjectDesignVisitRestrictionRepository>();
            services.AddScoped<IVisitEmailConfigurationRolesRepository, VisitEmailConfigurationRolesRepository>();
            services.AddScoped<IVisitEmailConfigurationRepository, VisitEmailConfigurationRepository>();
            services.AddScoped<ILettersFormateRepository, LettersFormateRepository>();
            services.AddScoped<ILettersActivityRepository, LettersActivityRepository>();
            services.AddScoped<IDashboardCompanyRepository, DashboardCompanyRepository>();
            services.AddScoped<ILabReportRepository, LabReportRepository>();
            services.AddScoped<IWorkingDayRepository, WorkingDayRepository>();
            services.AddScoped<IIDVerificationRepository, IDVerificationRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddHttpClient<IFirebaseNotification, FirebaseNotification>();
            services.AddScoped<IStudyPlanResourceRepository, StudyPlanResourceRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<IUserAccessRepository, UserAccessRepository>();
            services.AddScoped<IPharmacyBarcodeConfigRepository, PharmacyBarcodeConfigRepository>();
            services.AddScoped<IPharmacyBarcodeDisplayInfoRepository, PharmacyBarcodeDisplayInfoRepository>();
            services.AddScoped<IProcedureRepository, ProcedureRepository>();
            services.AddScoped<IPatientCostRepository, PatientCostRepository>();
            services.AddScoped<IPassThroughCostActivityRepository, PassThroughCostActivityRepository>();
            services.AddScoped<IPassThroughCostRepository, PassThroughCostRepository>();
            services.AddScoped<IResourceMilestoneRepository, ResourceMilestoneRepository>();
            services.AddScoped<IPatientMilestoneRepository, PatientMilestoneRepository>();
            services.AddScoped<IPassthroughMilestoneRepository, PassthroughMilestoneRepository>();
            services.AddScoped<IBudgetPaymentFinalCostRepository, BudgetPaymentFinalCostRepository>();
            services.AddScoped<ICtmsApprovalRolesRepository, CtmsApprovalRolesRepository>();
            services.AddScoped<ICtmsWorkflowApprovalRepository, CtmsWorkflowApprovalRepository>();
            services.AddScoped<ICtmsStudyPlanTaskCommentRepository, CtmsStudyPlanTaskCommentRepository>();
            services.AddScoped<IPaymentTermsRepository, PaymentTermsRepository>();
            services.AddScoped<IPassthroughMilestoneInvoiceRepository, PassthroughMilestoneInvoiceRepository>();
            services.AddScoped<IPatientMilestoneInvoiceRepository, PatientMilestoneInvoiceRepository>();
            services.AddScoped<IResourceMilestoneInvoiceRepository, ResourceMilestoneInvoiceRepository>();
            services.AddScoped<ISitePaymentRepository, SitePaymentRepository>();
            services.AddScoped<ISiteContractRepository, SiteContractRepository>();
            services.AddScoped<IPatientSiteContractRepository, PatientSiteContractRepository>();
            services.AddScoped<IPassthroughSiteContractRepository, PassthroughSiteContractRepository>();
            services.AddScoped<IContractTemplateFormatRepository, ContractTemplateFormatRepository>();
            services.AddScoped<ICtmsSiteContractWorkflowApprovalRepository, CtmsSiteContractWorkflowApprovalRepository>();
        }
    }
}