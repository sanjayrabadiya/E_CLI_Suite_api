using AutoMapper;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Custom;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Master.LanguageSetup;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Dto.Volunteer;
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
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Pharmacy;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.ProjectRight;
using GSC.Data.Entities.Report;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.SupplyManagement;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Entities.Volunteer;
using System.Linq;


namespace GSC.Api.Helpers
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<SecurityRole, SecurityRoleDto>().ReverseMap();
            CreateMap<SecurityRoleUpdationDto, SecurityRole>().ReverseMap();

            CreateMap<InsertCountryDto, Country>();
            CreateMap<UpdateCountryDto, Country>();
            CreateMap<Country, CountryDto>();

            CreateMap<InsertStateDto, State>();
            CreateMap<UpdateStateDto, State>();
            CreateMap<State, StateDto>();
            //.ForMember(x => x.CountryName, opt => opt.MapFrom(y => y.Country.CountryName));

            CreateMap<InsertCityDto, City>();
            CreateMap<UpdateCityDto, City>();
            CreateMap<City, CityDto>();

            CreateMap<Language, LanguageDto>().ReverseMap();
            CreateMap<ScopeName, ScopeNameDto>()
                .ForMember(x => x.ScopeName, opt => opt.MapFrom(y => y.Name));
            CreateMap<ScopeNameDto, ScopeName>()
                .ForMember(x => x.Name, opt => opt.MapFrom(y => y.ScopeName));


            CreateMap<Data.Entities.Master.Domain, DomainDto>().ReverseMap();

            CreateMap<ContactType, ContactTypeDto>().ReverseMap();
            CreateMap<Randomization, RandomizationDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<DomainClass, DomainClassDto>().ReverseMap();
            CreateMap<Drug, DrugDto>().ReverseMap();
            CreateMap<FoodType, FoodTypeDto>().ReverseMap();
            CreateMap<MaritalStatus, MaritalStatusDto>().ReverseMap();
            CreateMap<Occupation, OccupationDto>().ReverseMap();
            CreateMap<Race, RaceDto>().ReverseMap();
            CreateMap<Religion, ReligionDto>().ReverseMap();
            CreateMap<Variable, VariableDto>().ReverseMap();
            CreateMap<VolunteerAddress, VolunteerAddressDto>().ReverseMap();
            CreateMap<VolunteerBiometric, VolunteerBiometricDto>().ReverseMap();
            CreateMap<VolunteerContact, VolunteerContactDto>().ReverseMap();
            CreateMap<VolunteerDocument, VolunteerDocumentDto>().ReverseMap();
            CreateMap<Volunteer, VolunteerDto>().ReverseMap();
            CreateMap<VolunteerFood, VolunteerFoodDto>().ReverseMap();
            CreateMap<VolunteerHistory, VolunteerHistoryDto>().ReverseMap();
            CreateMap<VolunteerLanguage, VolunteerLanguageDto>().ReverseMap();
            CreateMap<Data.Entities.UserMgt.User, UserDto>().ReverseMap();
            CreateMap<Data.Entities.UserMgt.User, UserMobileDto>().ReverseMap();
            CreateMap<Client, ClientDto>().ReverseMap();
            CreateMap<ClientAddress, ClientAddressDto>().ReverseMap();
            CreateMap<ClientContact, ClientContactDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            //CreateMap<Project, RandomizationAndScreeningNumberFormatDto>().ReverseMap();
            CreateMap<UserRole, UserRoleDto>().ReverseMap();
            CreateMap<PopulationType, PopulationTypeDto>().ReverseMap();
            CreateMap<ProductType, ProductTypeDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<DesignTrial, DesignTrialDto>().ReverseMap();
            CreateMap<Activity, ActivityDto>().ReverseMap();
            CreateMap<TrialType, TrialTypeDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<EmailSetting, EmailSettingDto>().ReverseMap();
            CreateMap<EmailSetting, EmailSettingDto>().ReverseMap();
            CreateMap<AppScreen, AppScreenDto>().ReverseMap();
            CreateMap<EmailTemplate, EmailTemplateDto>().ReverseMap();
            CreateMap<LoginPreference, LoginPreferenceDto>().ReverseMap();
            CreateMap<UploadSetting, UploadSettingDto>().ReverseMap();
            CreateMap<DocumentType, DocumentTypeDto>().ReverseMap();
            CreateMap<VariableTemplate, VariableTemplateDto>().ReverseMap();
            CreateMap<VariableTemplateDetail, VariableTemplateDetailDto>().ReverseMap();
            CreateMap<Freezer, FreezerDto>().ReverseMap();
            CreateMap<TestGroup, TestGroupDto>().ReverseMap();
            CreateMap<Test, TestDto>().ReverseMap();
            CreateMap<MProductForm, ProductFormDto>().ReverseMap();
            CreateMap<VariableValue, VariableValueDto>().ReverseMap();
            CreateMap<VariableRemarks, VariableRemarksDto>().ReverseMap();
            CreateMap<CityArea, CityAreaDto>().ReverseMap();
            CreateMap<AnnotationType, AnnotationTypeDto>().ReverseMap();
            CreateMap<VariableCategory, VariableCategoryDto>().ReverseMap();
            CreateMap<Unit, UnitDto>().ReverseMap();
            CreateMap<NumberFormat, NumberFormatDto>().ReverseMap();
            CreateMap<RolePermission, RolePermissionDto>().ReverseMap();
            CreateMap<AuditReason, AuditReasonDto>().ReverseMap();
            CreateMap<VolunteerAuditTrail, VolunteerAuditTrailDto>().ReverseMap();
            CreateMap<ProjectDesign, ProjectDesignDto>().ReverseMap();
            CreateMap<ProjectDesignPeriod, ProjectDesignPeriodDto>().ReverseMap();
            CreateMap<ProjectDesignVisit, ProjectDesignVisitDto>().ReverseMap();
            CreateMap<ProjectDesignTemplate, ProjectDesignTemplateDto>().ReverseMap();
            CreateMap<ProjectDesignVariableValue, ScreeningVariableValueDto>().ReverseMap();
            CreateMap<ProjectDesignVariableRemarks, ScreeningVariableRemarksDto>().ReverseMap();
            CreateMap<ProjectDesignVariable, ProjectDesignVariableDto>().ReverseMap();
            CreateMap<ProjectDesignVariableValue, ProjectDesignVariableValueDto>().ReverseMap();
            CreateMap<ProjectDesignVariableRemarks, ProjectDesignVariableRemarksDto>().ReverseMap();
            CreateMap<ProjectDesignVariableValue, ProjectDesignVariableValueDropDown>().ReverseMap();
            CreateMap<ProjectDesignVariableRemarks, ProjectDesignVariableRemarksDropDown>().ReverseMap();
            CreateMap<ProjectDesignTemplateNote, ProjectDesignTemplateNoteDto>().ReverseMap();
            CreateMap<VariableTemplateNote, ProjectDesignTemplateNote>().ReverseMap();
            CreateMap<VariableTemplate, ProjectDesignTemplate>().ReverseMap();
            CreateMap<Variable, ProjectDesignVariable>().ReverseMap();
            CreateMap<VariableValue, ProjectDesignVariableValue>().ReverseMap();
            CreateMap<VariableRemarks, ProjectDesignVariableRemarks>().ReverseMap();
            CreateMap<ProjectRight, ProjectRightListDto>().ReverseMap();
            CreateMap<ProjectDocument, ProjectDocumentDto>().ReverseMap();
            CreateMap<ProjectWorkflow, ProjectWorkflowDto>().ReverseMap();
            CreateMap<InvestigatorContact, InvestigatorContactDto>().ReverseMap();
            CreateMap<ProjectWorkflowIndependent, ProjectWorkflowIndependentDto>().ReverseMap();
            CreateMap<ProjectWorkflowLevel, ProjectWorkflowLevelDto>().ReverseMap();
            CreateMap<ProjectSchedule, ProjectScheduleDto>().ReverseMap();
            CreateMap<ClientType, ClientTypeDto>().ReverseMap();
            CreateMap<DocumentName, DocumentNameDto>().ReverseMap();
            CreateMap<EditCheck, EditCheckDto>().ReverseMap();

            CreateMap<EditCheckDetail, EditCheckDetailDto>().ReverseMap();

            CreateMap<EditCheckDetail, EditCheckValidateDto>()
                .ForMember(x => x.AutoNumber, x => x.MapFrom(a => a.EditCheck.AutoNumber))
                .ForMember(x => x.CollectionSource, y => y.MapFrom(a => a.ProjectDesignVariable.CollectionSource))
                .ForMember(x => x.DataType, x => x.MapFrom(a => a.ProjectDesignVariable.DataType))
                .ForMember(x => x.ProjectDesignVisitId, x => x.MapFrom(a => a.ProjectDesignTemplate.ProjectDesignVisitId))
                .ForMember(x => x.EditCheckDetailId, x => x.MapFrom(a => a.Id))
                .ForMember(x => x.EditCheckId, x => x.MapFrom(a => a.EditCheckId))
                .ForMember(x => x.IsOnlyTarget, x => x.MapFrom(a => a.EditCheck.IsOnlyTarget))
                .ForMember(x => x.IsFormula, x => x.MapFrom(a => a.EditCheck.IsFormula));


            CreateMap<ProjectScheduleTemplate, ProjectScheduleTemplateDto>().ReverseMap();

            CreateMap<VariableTemplateNote, VariableTemplateNoteDto>().ReverseMap();

            CreateMap<ScreeningEntry, ScreeningEntryDto>().ReverseMap();
            CreateMap<ScreeningTemplateValue, ScreeningTemplateValueDto>().ReverseMap();
            CreateMap<ScreeningTemplateValue, Data.Dto.Screening.ScreeningTemplateValueBasic>()
               .ForMember(x => x.IsComment, x => x.MapFrom(a => a.Comments.Any()));

            CreateMap<ScreeningTemplateValueAudit, ScreeningTemplateValueAuditDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueComment, ScreeningTemplateValueCommentDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueChild, ScreeningTemplateValueChildDto>().ReverseMap();
            CreateMap<ScreeningTemplateRemarksChild, ScreeningTemplateRemarksChildDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueQuery, ScreeningTemplateValueQueryDto>().ReverseMap();
            CreateMap<ScreeningTemplateReview, MyReviewDto>().ReverseMap();
            CreateMap<ReportSetting, ReportSettingDto>().ReverseMap();
            CreateMap<Attendance, AttendanceDto>().ReverseMap();
            CreateMap<VolunteerBlockHistory, VolunteerBlockHistoryDto>().ReverseMap();
            CreateMap<TemplateRights, TemplateRightsDto>().ReverseMap();
            CreateMap<BlockCategory, BlockCategoryDto>().ReverseMap();
            CreateMap<UserRecentItem, UserRecentItemDto>().ReverseMap();
            CreateMap<ScreeningHistory, ScreeningHistoryDto>().ReverseMap();
            CreateMap<UserGridSetting, UserGridSettingDto>().ReverseMap();
            CreateMap<PharmacyConfig, PharmacyConfigDto>().ReverseMap();
            CreateMap<BarcodeType, BarcodeTypeDto>().ReverseMap();
            CreateMap<PharmacyTemplateValue, PharmacyTemplateValueDto>().ReverseMap();
            CreateMap<PharmacyTemplateValueAudit, PharmacyTemplateValueAuditDto>().ReverseMap();
            CreateMap<PharmacyTemplateValueChild, PharmacyTemplateValueChildDto>().ReverseMap();
            CreateMap<PharmacyEntry, PharmacyEntryDto>().ReverseMap();
            CreateMap<BarcodeConfig, BarcodeConfigDto>().ReverseMap();
            CreateMap<ProjectSubject, ProjectSubject>().ReverseMap();
            CreateMap<BarcodeGenerate, BarcodeGenerateDto>().ReverseMap();
            CreateMap<BarcodeSubjectDetail, BarcodeSubjectDetailDto>().ReverseMap();

            CreateMap<PharmacyVerificationTemplateValue, PharmacyVerificationTemplateValueDto>().ReverseMap();
            CreateMap<PharmacyVerificationTemplateValueAudit, PharmacyVerificationTemplateValueAuditDto>().ReverseMap();
            CreateMap<PharmacyVerificationTemplateValueChild, PharmacyVerificationTemplateValueChildDto>().ReverseMap();
            CreateMap<PharmacyVerificationEntry, PharmacyVerificationEntryDto>().ReverseMap();
            CreateMap<AttendanceHistory, AttendanceHistoryDto>().ReverseMap();
            CreateMap<MedraConfig, MedraConfigDto>().ReverseMap();
            CreateMap<Dictionary, DictionaryDto>().ReverseMap();
            CreateMap<MedraLanguage, MedraLanguageDto>().ReverseMap();
            CreateMap<StudyScoping, StudyScopingDto>().ReverseMap();
            CreateMap<MedraVersion, MedraVersionDto>().ReverseMap();

            CreateMap<MeddraHlgtHltComp, MeddraHlgtHltCompDto>().ReverseMap();
            CreateMap<MeddraHlgtPrefTerm, MeddraHlgtPrefTermDto>().ReverseMap();
            CreateMap<MeddraHltPrefComp, MeddraHltPrefCompDto>().ReverseMap();
            CreateMap<MeddraHltPrefTerm, MedraVersionDto>().ReverseMap();
            CreateMap<MeddraLowLevelTerm, MeddraLowLevelTermDto>().ReverseMap();
            CreateMap<MeddraMdHierarchy, MeddraMdHierarchyDto>().ReverseMap();
            CreateMap<MeddraPrefTerm, MeddraPrefTermDto>().ReverseMap();
            CreateMap<MeddraSmqContent, MeddraSmqContentDto>().ReverseMap();
            CreateMap<MeddraSmqList, MeddraSmqListDto>().ReverseMap();
            CreateMap<MeddraSocHlgtComp, MeddraSocHlgtCompDto>().ReverseMap();
            CreateMap<MeddraSocIntlOrder, MeddraSocIntlOrderDto>().ReverseMap();
            CreateMap<MeddraSocTerm, MeddraSocTermDto>().ReverseMap();
            CreateMap<ScreeningTemplateLockUnlockAudit, ScreeningTemplateLockUnlockAuditDto>().ReverseMap();
            CreateMap<MeddraCoding, MeddraCodingDto>().ReverseMap();
            CreateMap<MeddraCodingComment, MeddraCodingCommentDto>().ReverseMap();
            CreateMap<MeddraCodingAudit, MeddraCodingAuditDto>().ReverseMap();


            CreateMap<EtmfZoneMasterLibrary, EtmfZoneMasterLibraryDto>().ReverseMap();
            CreateMap<EtmfSectionMasterLibrary, EtmfSectionMasterLibraryDto>().ReverseMap();
            CreateMap<EtmfArtificateMasterLbrary, EtmfArtificateMasterLbraryDto>().ReverseMap();

            CreateMap<ProjectWorkplace, ETMFWorkplaceDto>().ReverseMap();
            CreateMap<ProjectWorkplaceArtificatedocument, ProjectWorkplaceArtificatedocumentDto>().ReverseMap();
            CreateMap<ProjectWorkplaceSubSection, ProjectWorkplaceSubSectionDto>().ReverseMap();
            CreateMap<ProjectWorkplaceSubSectionArtifact, ProjectWorkplaceSubSectionArtifactDto>().ReverseMap();
            CreateMap<ProjectWorkplaceSection, ProjectWorkplaceSectionDto>().ReverseMap();
            CreateMap<ProjectWorkplaceSubSecArtificatedocument, ProjectWorkplaceSubSecArtificatedocumentDto>().ReverseMap();
            CreateMap<InvestigatorContactDetail, InvestigatorContactDetailDto>().ReverseMap();
            CreateMap<Holiday, HolidayDto>().ReverseMap();
            CreateMap<ManageSite, ManageSiteDto>().ReverseMap();
            CreateMap<Iecirb, IecirbDto>().ReverseMap();
            CreateMap<JobMonitoring, JobMonitoringDto>().ReverseMap();
            CreateMap<CompanyData, CompanyDataDto>().ReverseMap();
            CreateMap<PatientStatus, PatientStatusDto>().ReverseMap();
            CreateMap<VisitStatus, VisitStatusDto>().ReverseMap();
            CreateMap<ProjectArtificateDocumentReview, ProjectArtificateDocumentReviewDto>().ReverseMap();
            CreateMap<ProjectArtificateDocumentComment, ProjectArtificateDocumentCommentDto>().ReverseMap();
            CreateMap<ProjectSubSecArtificateDocumentReview, ProjectSubSecArtificateDocumentReviewDto>().ReverseMap();
            CreateMap<ProjectSubSecArtificateDocumentHistory, ProjectSubSecArtificateDocumentHistoryDto>().ReverseMap();
            CreateMap<ProjectSubSecArtificateDocumentApprover, ProjectSubSecArtificateDocumentApproverDto>().ReverseMap();
            CreateMap<ProjectSubSecArtificateDocumentComment, ProjectSubSecArtificateDocumentCommentDto>().ReverseMap();

            CreateMap<ProjectDesignVisitStatus, ProjectDesignVisitStatusDto>().ReverseMap();
            CreateMap<EconsentSetup, EconsentSetupDto>().ReverseMap();
            CreateMap<EconsentReviewDetails, EconsentReviewDetailsDto>().ReverseMap();
            CreateMap<RegulatoryType, RegulatoryTypeDto>().ReverseMap();
            CreateMap<ProjectArtificateDocumentApprover, ProjectArtificateDocumentApproverDto>().ReverseMap();
            CreateMap<EconsentSetupPatientStatus, EconsentSetupPatientStatusDto>().ReverseMap();
            CreateMap<EconsentSectionReference, EconsentSectionReferenceDto>().ReverseMap();
            CreateMap<EconsentReviewDetailsSections, EconsentReviewDetailsSectionsDto>().ReverseMap();
            CreateMap<EconsentChat, EconsentChatDto>().ReverseMap();
            CreateMap<Data.Entities.UserMgt.User, EConsentUserChatDto>()
                .ForMember(x => x.UserName, y => y.MapFrom(a => a.FirstName +" "+ a.LastName ))
                .ReverseMap();
            CreateMap<EconsentSetupRoles, EconsentSetupRolesDto>().ReverseMap();
            CreateMap<Site, SiteDto>().ReverseMap();
            CreateMap<ScreeningVisitHistory, ScreeningVisitHistoryDto>().ReverseMap();
            CreateMap<VisitLanguage, VisitLanguageDto>().ReverseMap();
            CreateMap<TemplateLanguage, TemplateLanguageDto>().ReverseMap();
            CreateMap<VariableLanguage, VariableLanguageDto>().ReverseMap();
            CreateMap<VariableNoteLanguage, VariableNoteLanguageDto>().ReverseMap();
            CreateMap<TemplateNoteLanguage, TemplateNoteLanguageDto>().ReverseMap();
            CreateMap<VariableValueLanguage, VariableValueLanguageDto>().ReverseMap();
            CreateMap<SiteTeam, SiteTeamDto>().ReverseMap();
            CreateMap<AppScreenPatient, AppScreenPatientDto>().ReverseMap();
            CreateMap<AppScreenPatientRights, AppScreenPatientRightsDto>().ReverseMap();
            CreateMap<VariableCategory, VariableCategoryDto>().ReverseMap();
            CreateMap<AEReporting, AEReportingDto>().ReverseMap();
            CreateMap<AdverseEventSettings, AdverseEventSettingsDto>().ReverseMap();
            CreateMap<EtmfUserPermission, EtmfUserPermissionDto>().ReverseMap();
            CreateMap<PhaseManagement, PhaseManagementDto>().ReverseMap();
            CreateMap<ResourceType, ResourceTypeDto>().ReverseMap();

            CreateMap<ProjectDesignVariableEncryptRole, ProjectDesignVariableEncryptRoleDto>().ReverseMap();
            CreateMap<ProjectDesingTemplateRestriction, ProjectDesingTemplateRestrictionDto>().ReverseMap();
            CreateMap<UserSetting, UserSettingDto>().ReverseMap();

            CreateMap<Project, StydyDetails>()
                .ForMember(x => x.StudyCode, y => y.MapFrom(a => a.ProjectCode))
                .ForMember(x => x.ValidFrom, y => y.MapFrom(a => a.FromDate))
                .ForMember(x => x.ValidTo, y => y.MapFrom(a => a.ToDate))
                .ReverseMap();

            CreateMap<UserDto, RandomizationDto>()
                 //.ForMember(x => x.FirstName, y => y.MapFrom(a => a.FirstName))
                 //.ForMember(x => x.MiddleName, y => y.MapFrom(a => a.MiddleName))
                 //.ForMember(x => x.LastName, y => y.MapFrom(a => a.LastName))
                 //.ForMember(x => x.DateOfBirth, y => y.MapFrom(a => a.DateOfBirth))                 
                 //.ForMember(x => x.Email, y => y.MapFrom(a => a.Email))
                 .ForMember(x => x.PrimaryContactNumber, y => y.MapFrom(a => a.Phone))
                 .ReverseMap();

            CreateMap<RefreshTokanDto, LoginResponseDto>().ReverseMap();
            CreateMap<TaskTemplate, TaskTemplateDto>().ReverseMap();
            CreateMap<TaskMaster, TaskmasterDto>().ReverseMap();
            CreateMap<StudyPlan, StudyPlanDto>().ReverseMap();
            CreateMap<StudyPlanTask,TaskMaster>()
                .ForMember(x => x.Id, y => y.MapFrom(a => a.TaskId))
               .ReverseMap();
            CreateMap<ScreeningNumberSettings, ScreeningNumberSettingsDto>()
               .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
              .ReverseMap();
            CreateMap<RandomizationNumberSettings, RandomizationNumberSettingsDto>()
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ReverseMap();
            CreateMap<EconsentReviewDetails, EconsentReviewDetailsDto>()
                .ForMember(x => x.EconsentDocumentName, x => x.MapFrom(a => a.EconsentSetup.DocumentName))
                .ReverseMap();
            CreateMap<EConsentVideo, EConsentVideoDto>().ReverseMap();
            CreateMap<ManageMonitoring, ManageMonitoringDto>().ReverseMap();
            CreateMap<StudyPlanTaskDto, StudyPlanTask>().ReverseMap();
            CreateMap<StudyPlantaskParameterDto, StudyPlanTask>().ReverseMap();
            //CreateMap<DependentTaskParameterDto, DependentTask>().ReverseMap();
            CreateMap<HolidayMasterDto, HolidayMaster>().ReverseMap();
            CreateMap<HolidayMasterListDto, HolidayMaster>()
              .ForMember(x => x.HolidayName, x => x.MapFrom(a => a.Label))
              .ForMember(x => x.FromDate, x => x.MapFrom(a => a.From))
              .ForMember(x => x.ToDate, x => x.MapFrom(a => a.To))
              .ReverseMap();
            CreateMap<WeekEndMasterDto, WeekEndMaster>().ReverseMap();
            CreateMap<ReportSettingNew, ProjectDesignReportSetting>()
               .ForMember(x => x.ProjectDesignId, y => y.MapFrom(a => a.ProjectId))            
               .ReverseMap();
            CreateMap<SupplyLocation, SupplyLocationDto>().ReverseMap();
            CreateMap<CentralDepot, CentralDepotDto>().ReverseMap();
            CreateMap<StudyVersion, StudyVersionDto>().ReverseMap();

            CreateMap<Volunteer, VolunteerGridDto>()
               .ForMember(x => x.FoodType, x => x.MapFrom(a => a.FoodType.TypeName))
               .ForMember(x => x.PopulationType, x => x.MapFrom(a => a.PopulationType.PopulationName))
               .ForMember(x => x.MaritalStatus, x => x.MapFrom(a => a.MaritalStatus.MaritalStatusName))
               .ForMember(x => x.Race, x => x.MapFrom(a => a.Race.RaceName))
               .ForMember(x => x.Occupation, x => x.MapFrom(a => a.Occupation.OccupationName))
               .ForMember(x => x.Religion, x => x.MapFrom(a => a.Religion.ReligionName))
               .ForMember(x => x.Gender, x => x.MapFrom(a => a.GenderId))
               .ReverseMap();
            //CreateMap<VolunteerAddress, VolunteerAddressDto>().ReverseMap();
            //CreateMap<VolunteerBiometric, VolunteerBiometricDto>().ReverseMap();
            //CreateMap<VolunteerContact, VolunteerContactDto>().ReverseMap();
            //CreateMap<VolunteerDocument, VolunteerDocumentDto>().ReverseMap();
            //CreateMap<Volunteer, VolunteerDto>().ReverseMap();
            CreateMap<VolunteerFood, VolunteerFoodDto>().ReverseMap();
            //CreateMap<VolunteerHistory, VolunteerHistoryDto>().ReverseMap();
            //CreateMap<VolunteerLanguage, VolunteerLanguageDto>().ReverseMap();
            CreateMap<EconsentReviewDetails, EconsentDocumentDetailsDto>()
                 .ForMember(x => x.EconsentDocumentName, y => y.MapFrom(a => a.EconsentSetup.DocumentName))
                 .ReverseMap();
            CreateMap<VolunteerQuery, VolunteerQueryDto>().ReverseMap();
            CreateMap<PharmacyStudyProductType, PharmacyStudyProductTypeDto>().ReverseMap();
            CreateMap<ProductReceipt, ProductReceiptDto>().ReverseMap();
            CreateMap<StudyPlanTaskResourceDto, StudyPlanTaskResource>().ReverseMap();
            CreateMap<ProductVerification,ProductVerificationDto>().ReverseMap();
            CreateMap<ProductVerificationDetail,ProductVerificationDetailDto>().ReverseMap();
            CreateMap<LanguageConfiguration, LanguageConfigurationDto>().ReverseMap();
            CreateMap<LanguageConfigurationDetails, LanguageConfigurationDetailsDto>().ReverseMap();
        }
    }
}