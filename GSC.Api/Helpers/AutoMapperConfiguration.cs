using AutoMapper;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Common;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.EditCheck;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Screening;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Pharmacy;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.EditCheck;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.ProjectRight;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Entities.Volunteer;

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
            CreateMap<NoneRegister, NoneRegisterDto>().ReverseMap();
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
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Client, ClientDto>().ReverseMap();
            CreateMap<ClientAddress, ClientAddressDto>().ReverseMap();
            CreateMap<ClientContact, ClientContactDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
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
            CreateMap<CityArea, CityAreaDto>().ReverseMap();
            CreateMap<AnnotationType, AnnotationTypeDto>().ReverseMap();
            CreateMap<VariableCategory, VariableCategoryDto>().ReverseMap();
            CreateMap<Unit, UnitDto>().ReverseMap();
            CreateMap<NumberFormat, NumberFormatDto>().ReverseMap();
            CreateMap<RolePermission, RolePermissionDto>().ReverseMap();
            CreateMap<AuditReason, AuditReasonDto>().ReverseMap();
            CreateMap<AuditTrail, AuditTrailDto>().ReverseMap();
            CreateMap<ProjectDesign, ProjectDesignDto>().ReverseMap();
            CreateMap<ProjectDesignPeriod, ProjectDesignPeriodDto>().ReverseMap();
            CreateMap<ProjectDesignVisit, ProjectDesignVisitDto>().ReverseMap();
            CreateMap<ProjectDesignTemplate, ProjectDesignTemplateDto>().ReverseMap();
            CreateMap<ProjectDesignVariable, ProjectDesignVariableDto>().ReverseMap();
            CreateMap<ProjectDesignVariableValue, ProjectDesignVariableValueDto>().ReverseMap();
            CreateMap<ProjectDesignVariableValue, ProjectDesignVariableValueDropDown>().ReverseMap();
            CreateMap<VariableTemplate, ProjectDesignTemplate>().ReverseMap();
            CreateMap<Variable, ProjectDesignVariable>().ReverseMap();
            CreateMap<VariableValue, ProjectDesignVariableValue>().ReverseMap();
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
                .ForMember(x => x.EditCheckDetailId, x => x.MapFrom(a => a.Id))
                .ForMember(x => x.EditCheckId, x => x.MapFrom(a => a.EditCheckId))
                .ForMember(x => x.IsOnlyTarget, x => x.MapFrom(a => a.EditCheck.IsOnlyTarget))
                .ForMember(x => x.IsFormula, x => x.MapFrom(a => a.EditCheck.IsFormula));


            CreateMap<ProjectScheduleTemplate, ProjectScheduleTemplateDto>().ReverseMap();

            CreateMap<VariableTemplateNote, VariableTemplateNoteDto>().ReverseMap();

            CreateMap<ScreeningEntry, ScreeningEntryDto>().ReverseMap();
            CreateMap<ScreeningTemplate, ScreeningTemplateDto>().ReverseMap();
            CreateMap<ScreeningTemplateValue, ScreeningTemplateValueDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueEditCheck, ScreeningTemplateValueEditCheckDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueSchedule, ScreeningTemplateValueScheduleDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueAudit, ScreeningTemplateValueAuditDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueComment, ScreeningTemplateValueCommentDto>().ReverseMap();
            CreateMap<ScreeningTemplateValueChild, ScreeningTemplateValueChildDto>().ReverseMap();
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
            CreateMap<VariableTemplateRight, VariableTemplateRightDto>().ReverseMap();
            CreateMap<PharmacyConfig, PharmacyConfigDto>().ReverseMap();
            CreateMap<BarcodeType, BarcodeTypeDto>().ReverseMap();
            CreateMap<PharmacyTemplateValue, PharmacyTemplateValueDto>().ReverseMap();
            CreateMap<PharmacyTemplateValueAudit, PharmacyTemplateValueAuditDto>().ReverseMap();
            CreateMap<PharmacyTemplateValueChild, PharmacyTemplateValueChildDto>().ReverseMap();
            CreateMap<PharmacyEntry, PharmacyEntryDto>().ReverseMap();
            //CreateMap<PharmacyTemplate, PharmacyTemplateDto>().ReverseMap();
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
        }
    }
}