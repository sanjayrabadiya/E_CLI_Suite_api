using AutoMapper;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.UserMgt;
using GSC.Common.Base;
using System;
using System.Linq;
using GSC.Shared.Extension;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Dto.LanguageSetup;
using GSC.Data.Dto.Master.LanguageSetup;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;

namespace GSC.Api.Helpers
{
    public class AutoMapperGridModel : Profile
    {
        public AutoMapperGridModel()
        {
            CreateMap<AnnotationType, AnnotationTypeGridDto>()
                 .ForMember(x => x.AnnotationeName, y => y.MapFrom(a => a.AnnotationeName)).ReverseMap();

            CreateMap<BaseEntity, BaseAuditDto>()
            .ForMember(x => x.CreatedByUser, y => y.MapFrom(a => a.CreatedByUser.UserName))
            .ForMember(x => x.IsDeleted, y => y.MapFrom(a => a.DeletedDate != null))
            .ForMember(x => x.DeletedByUser, y => y.MapFrom(a => a.DeletedByUser.UserName))
            .ForMember(x => x.ModifiedByUser, y => y.MapFrom(a => a.ModifiedByUser.UserName)).IncludeAllDerived();


            CreateMap<Data.Entities.Master.Domain, DomainGridDto>()
                 .ForMember(x => x.DomainClassName, x => x.MapFrom(a => a.DomainClass.DomainClassName)).ReverseMap();

            CreateMap<DomainClass, DomainClassGridDto>().ReverseMap();

            CreateMap<Drug, DrugGridDto>().ReverseMap();
            CreateMap<AuditReason, AuditReasonGridDto>().ReverseMap();
            CreateMap<BlockCategory, BlockCategoryGridDto>().ReverseMap();
            CreateMap<CityArea, CityAreaGridDto>()
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName)).ReverseMap(); ;
            CreateMap<City, CityGridDto>()
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.State.StateName))
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.State.Country.CountryName)).ReverseMap();
            CreateMap<Country, CountryGridDto>().ReverseMap();
            CreateMap<ContactType, ContactTypeGridDto>().ReverseMap();
            CreateMap<ClientType, ClientTypeGridDto>().ReverseMap();
            CreateMap<Department, DepartmentGridDto>().ReverseMap();
            CreateMap<DocumentType, DocumentTypeGridDto>().ReverseMap();
            CreateMap<DocumentName, DocumentNameGridDto>().ReverseMap();
            CreateMap<Freezer, FreezerGridDto>().ReverseMap();
            CreateMap<FoodType, FoodTypeGridDto>().ReverseMap();
            CreateMap<Language, LanguageGridDto>().ReverseMap();
            CreateMap<MaritalStatus, MaritalStatusGridDto>().ReverseMap();
            CreateMap<Occupation, OccupationGridDto>().ReverseMap();
            CreateMap<PopulationType, PopulationTypeGridDto>().ReverseMap();
            CreateMap<ProductType, ProductTypeGridDto>().ReverseMap();
            CreateMap<Race, RaceGridDto>().ReverseMap();
            CreateMap<Religion, ReligionGridDto>().ReverseMap();
            CreateMap<TestGroup, TestGroupGridDto>().ReverseMap();
            CreateMap<Test, TestGridDto>().ReverseMap();
            CreateMap<Unit, UnitGridDto>().ReverseMap();
            CreateMap<State, StateGridDto>().ReverseMap();
            CreateMap<TrialType, TrialTypeGridDto>().ReverseMap();
            CreateMap<ScopeName, ScopeNameGridDto>().ForMember(x => x.ScopeName, y => y.MapFrom(a => a.Name)).ReverseMap();
            CreateMap<Client, ClientGridDto>().ReverseMap();
            CreateMap<DesignTrial, DesignTrialGridDto>().ReverseMap();
            CreateMap<VariableCategory, VariableCategoryGridDto>()
                 .ForMember(x => x.CategoryName, y => y.MapFrom(a => a.CategoryName)).ReverseMap();
            CreateMap<MedraVersion, MedraVersionGridDto>().ReverseMap();
            CreateMap<MedraLanguage, MedraLanguageGridDto>().ReverseMap();


            //creator : Darshil
            //date : 10/02/2020
            //description : Add CollectionValue using comma separator
            //end region : VariableGridDto
            CreateMap<Variable, VariableGridDto>()
                 .ForMember(x => x.DomainName, x => x.MapFrom(a => a.Domain.DomainName))
                 .ForMember(x => x.AnnotationType, x => x.MapFrom(a => a.AnnotationType.AnnotationeName))
                 .ForMember(x => x.RoleVariableType, x => x.MapFrom(a => a.RoleVariableType))
                 .ForMember(x => x.CoreVariableType, x => x.MapFrom(a => a.CoreVariableType))
                 .ForMember(x => x.Unit, x => x.MapFrom(a => a.Unit.UnitName))
                 .ForMember(x => x.UnitAnnotation, x => x.MapFrom(a => a.UnitAnnotation))
                 .ForMember(x => x.CollectionSource, x => x.MapFrom(a => a.CollectionSource))
                 .ForMember(x => x.DataType, x => x.MapFrom(a => a.DataType))
                 .ForMember(x => x.Length, x => x.MapFrom(a => a.Length))
                 .ForMember(x => x.ValidationType, x => x.MapFrom(a => a.ValidationType))
                 .ForMember(x => x.CollectionValue, x => x.MapFrom(a => string.Join(", ", a.Values.ToList().Select(x => x.ValueName))))
                 .ForMember(x => x.DateValidate, x => x.MapFrom(a => a.DateValidate)).ReverseMap();
            CreateMap<PatientStatus, PatientStatusGridDto>().ReverseMap();
            CreateMap<VisitStatus, VisitStatusGridDto>().ReverseMap();
            CreateMap<SecurityRole, SecurityRoleGridDto>().ReverseMap();
            CreateMap<Iecirb, IecirbGridDto>().ReverseMap();
            CreateMap<User, UserGridDto>()
                .ForMember(x => x.Role, x => x.MapFrom(a => string.Join(", ", a.UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList())))
                .ForMember(x => x.CompanyName, x => x.MapFrom(a => a.Company.CompanyName))
                .ReverseMap();

            CreateMap<Randomization, RandomizationGridDto>()
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName))
                .ForMember(x => x.CountryId, x => x.MapFrom(a => a.City.State.Country.Id))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
                .ForMember(x => x.StateId, x => x.MapFrom(a => a.City.State.Id))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                 .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                 .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                 .ForMember(x => x.Language, x => x.MapFrom(a => a.Language.LanguageName))
                 .ForMember(x => x.Gen, x => x.MapFrom(a => a.Gender.GetDescription()))
                 .ForMember(x => x.LegalRelationship, x => x.MapFrom(a => a.LegalRelationship.GetDescription()))
                 .ForMember(x => x.PatientStatusName, x => x.MapFrom(a => a.PatientStatusId.GetDescription())).ReverseMap();

            CreateMap<ProjectWorkplace, ETMFWorkplaceGridDto>()
                .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                //.ForMember(x => x.NoofSite, x => x.MapFrom(a => a.ChildProject.ToList().Count() > 0 ? a.ChildProject.ToList().Count() : 0))
                .ReverseMap();

            CreateMap<Data.Entities.Master.Project, ProjectGridDto>()
                //.ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.ManageSite.City.State.Country.CountryName))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ForMember(x => x.AreaName, x => x.MapFrom(a => a.CityArea.AreaName))
                .ForMember(x => x.SiteName, x => x.MapFrom(a => a.ManageSite.SiteName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.State.StateName))
                .ForMember(x => x.ClientName, x => x.MapFrom(a => a.Client.ClientName))
                .ForMember(x => x.TherapeuticIndication, x => x.MapFrom(a => a.DesignTrial.TrialType.TrialTypeName))
                .ForMember(x => x.DesignTrialName, x => x.MapFrom(a => a.DesignTrial.DesignTrialName))
                .ForMember(x => x.ClientName, x => x.MapFrom(a => a.Client.ClientName))
                .ForMember(x => x.DrugName, x => x.MapFrom(a => a.Drug.DrugName))
                .ForMember(x => x.InvestigatorContactName, x => x.MapFrom(a => a.InvestigatorContact.NameOfInvestigator))
                .ForMember(x => x.RegulatoryTypeName, x => x.MapFrom(a => a.RegulatoryType.RegulatoryTypeName))
                .ForMember(x => x.ProjectDesignId, x => x.MapFrom(a => a.ProjectDesigns.Where(x => x.DeletedDate == null).Select(r => r.Id).FirstOrDefault()))
                .ForMember(x => x.Locked, x => x.MapFrom(a => !a.ProjectDesigns.Where(x => x.DeletedDate == null).Select(r => r.IsUnderTesting).FirstOrDefault()))
                .ForMember(x => x.ParentProjectCode, x => x.MapFrom(a => a.ChildProject.Where(x => x.DeletedDate == null).Select(r => r.ProjectCode).FirstOrDefault()))
                .ForMember(x => x.NoofSite, x => x.MapFrom(a => a.ChildProject.Where(x => x.DeletedDate == null).Count()))
                .ReverseMap();

            CreateMap<ManageSite, ManageSiteGridDto>()
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ForMember(x => x.TherapeuticIndicationName, x => x.MapFrom(a => string.Join(", ", a.ManageSiteRole.Where(x => x.DeletedDate == null).Select(s => s.TrialType.TrialTypeName).ToList())))
                .ReverseMap();

            CreateMap<VariableTemplate, VariableTemplateGridDto>()
               .ForMember(x => x.DomainName, x => x.MapFrom(a => a.Domain.DomainName))
               .ForMember(x => x.ActivityMode, x => x.MapFrom(a => a.ActivityMode.GetDescription()))
               .ReverseMap();

            CreateMap<InvestigatorContact, InvestigatorContactGridDto>()
               .ForMember(x => x.TherapeuticIndication, x => x.MapFrom(a => a.TrialType.TrialTypeName))
               .ReverseMap();

            CreateMap<InvestigatorContactDetail, InvestigatorContactDetailGridDto>().ReverseMap();

            CreateMap<Holiday, HolidayGridDto>()
                .ForMember(x => x.HolidayType, x => x.MapFrom(a => a.HolidayType.GetDescription())).ReverseMap();

            CreateMap<VisitStatus, VisitStatusGridDto>().ReverseMap();
            CreateMap<RegulatoryType, RegulatoryTypeGridDto>().ReverseMap();

            CreateMap<Company, CompanyGridDto>()
              .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Location.Country.CountryName))
              .ForMember(x => x.StateName, x => x.MapFrom(a => a.Location.State.StateName))
              .ForMember(x => x.CityName, x => x.MapFrom(a => a.Location.City.CityName))
              .ForMember(x => x.Address, x => x.MapFrom(a => a.Location.Address))
              .ReverseMap();

            CreateMap<Site, SiteGridDto>()
               .ForMember(x => x.SiteName, x => x.MapFrom(a => a.ManageSite.SiteName))
               .ForMember(x => x.ContactNumber, x => x.MapFrom(a => a.ManageSite.ContactNumber))
               .ForMember(x => x.CountryName, x => x.MapFrom(a => a.ManageSite.City.State.Country.CountryName))
               .ForMember(x => x.StateName, x => x.MapFrom(a => a.ManageSite.City.State.StateName))
               .ForMember(x => x.CityName, x => x.MapFrom(a => a.ManageSite.City.CityName))
               .ForMember(x => x.ContactName, x => x.MapFrom(a => a.ManageSite.ContactName))
               .ForMember(x => x.SiteEmail, x => x.MapFrom(a => a.ManageSite.SiteEmail))
               .ForMember(x => x.ManageSiteId, x => x.MapFrom(a => Convert.ToInt32(a.ManageSiteId)))
               .ForMember(x => x.IECIRBName, x => x.MapFrom(a => string.Join(", ", a.ManageSite.Iecirb.ToList().Select(x => x.IECIRBName))))
               .ReverseMap();

            CreateMap<ProjectDesignVisitStatus, ProjectDesignVisitStatusGridDto>()
              .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
              .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.ProjectDesignVariable.ProjectDesignTemplate.TemplateName))
              .ForMember(x => x.ProjectDesignVariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
              .ForMember(x => x.VisitStatus, x => x.MapFrom(a => a.VisitStatusId.GetDescription()))
              .ReverseMap();

            CreateMap<ProjectDesignTemplateNote, ProjectDesignTemplateNoteGridDto>()
               .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName)).ReverseMap();
            CreateMap<EconsentSetup, EconsentSetupGridDto>().ReverseMap();

            CreateMap<VisitLanguage, VisitLanguageGridDto>()
              .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
              .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
              .ReverseMap();

            CreateMap<TemplateLanguage, TemplateLanguageGridDto>()
             .ForMember(x => x.TemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
             .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
             .ReverseMap();

            CreateMap<VariableLanguage, VariableLanguageGridDto>()
             .ForMember(x => x.VariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
             .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
             .ReverseMap();

            CreateMap<VariableNoteLanguage, VariableNoteLanguageGridDto>()
             .ForMember(x => x.Note, x => x.MapFrom(a => a.ProjectDesignVariable.Note))
             .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
             .ReverseMap();

            CreateMap<TemplateNoteLanguage, TemplateNoteLanguageGridDto>()
            .ForMember(x => x.Note, x => x.MapFrom(a => a.ProjectDesignTemplateNote.Note))
            .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
            .ReverseMap();

            CreateMap<VariableValueLanguage, VariableValueLanguageGridDto>()
            .ForMember(x => x.ValueName, x => x.MapFrom(a => a.ProjectDesignVariableValue.ValueName))
            .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
            .ReverseMap();

            CreateMap<VariableCategoryLanguage, VariableCategoryLanguageGridDto>()
           .ForMember(x => x.CategoryName, x => x.MapFrom(a => a.VariableCategory.CategoryName))
           .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
           .ReverseMap();

            CreateMap<SiteTeam, SiteTeamGridDto>().ReverseMap();
            CreateMap<PhaseManagement, PhaseManagementGridDto>().ReverseMap();
            CreateMap<ResourceType, ResourceTypeGridDto>().ReverseMap();
            CreateMap<TaskTemplate, TaskTemplateGridDto>().ReverseMap();

            CreateMap<TaskMaster, TaskMasterGridDto>().ReverseMap();
            CreateMap<StudyPlan, StudyPlanGridDto>()
                   .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                   .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                  .ReverseMap();
        }
    }
}
