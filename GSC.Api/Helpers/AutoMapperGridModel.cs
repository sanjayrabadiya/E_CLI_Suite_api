using AutoMapper;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Client;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Location;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System.Linq;

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
            CreateMap<CityArea, CityAreaGridDto>().ReverseMap();
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
            CreateMap<Variable, VariableGridDto>()
                 .ForMember(x => x.DomainName, x => x.MapFrom(a => a.Domain.DomainName))
                 .ForMember(x => x.AnnotationType, x => x.MapFrom(a => a.AnnotationType.AnnotationeName))
                 //.ForMember(x => x.VariableCategory, x => x.MapFrom(a => a.Values))
                 .ForMember(x => x.RoleVariableType, x => x.MapFrom(a => a.RoleVariableType))
                 .ForMember(x => x.CoreVariableType, x => x.MapFrom(a => a.CoreVariableType))
                 .ForMember(x => x.Unit, x => x.MapFrom(a => a.Unit.UnitName))
                 .ForMember(x => x.UnitAnnotation, x => x.MapFrom(a => a.UnitAnnotation))
                 .ForMember(x => x.CollectionSource, x => x.MapFrom(a => a.CollectionSource))
                 .ForMember(x => x.DataType, x => x.MapFrom(a => a.DataType))
                 .ForMember(x => x.Length, x => x.MapFrom(a => a.Length))
                 .ForMember(x => x.ValidationType, x => x.MapFrom(a => a.ValidationType))
                 .ForMember(x => x.DateValidate, x => x.MapFrom(a => a.DateValidate)).ReverseMap();
            CreateMap<PatientStatus, PatientStatusGridDto>().ReverseMap();
            CreateMap<VisitStatus, VisitStatusGridDto>().ReverseMap();
            CreateMap<SecurityRole, SecurityRoleGridDto>().ReverseMap();
            CreateMap<Iecirb, IecirbGridDto>().ReverseMap();

            CreateMap<Randomization, RandomizationGridDto>()
                 .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                 .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                 .ReverseMap();
            CreateMap<ProjectWorkplace, ETMFWorkplaceGridDto>()
                .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ReverseMap();

            CreateMap<Data.Entities.Master.Project, ProjectGridDto>()
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ForMember(x => x.AreaName, x => x.MapFrom(a => a.CityArea.AreaName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.State.StateName))
                .ForMember(x => x.ClientName, x => x.MapFrom(a => a.Client.ClientName))
                .ForMember(x => x.DesignTrialName, x => x.MapFrom(a => a.DesignTrial.DesignTrialName))
                .ForMember(x => x.ClientName, x => x.MapFrom(a => a.Client.ClientName))
                .ForMember(x => x.DrugName, x => x.MapFrom(a => a.Drug.DrugName))
                .ForMember(x => x.RegulatoryTypeName, x => x.MapFrom(a => a.RegulatoryType.GetDescription()))
                .ForMember(x => x.ProjectDesignId, x => x.MapFrom(a => a.ProjectDesigns.Where(x => x.DeletedDate == null).Select(r => r.Id).FirstOrDefault()))
                .ForMember(x => x.Locked, x => x.MapFrom(a => !a.ProjectDesigns.Where(x => x.DeletedDate == null).Select(r => r.IsUnderTesting).FirstOrDefault()))
                .ReverseMap();

            CreateMap<ManageSite, ManageSiteGridDto>()
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ReverseMap();

            CreateMap<VariableTemplate, VariableTemplateGridDto>()
               .ForMember(x => x.DomainName, x => x.MapFrom(a => a.Domain.DomainName))
               .ForMember(x => x.ActivityMode, x => x.MapFrom(a => a.ActivityMode.GetDescription()))
               .ReverseMap();

            CreateMap<InvestigatorContact, InvestigatorContactGridDto>()
               .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName))
               .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
               .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
               .ForMember(x => x.SiteName, x => x.MapFrom(a => a.ManageSite.SiteName))
               .ForMember(x => x.IECIRBName, x => x.MapFrom(a => a.Iecirb.IECIRBName))
               .ForMember(x => x.IECIRBContactNo, x => x.MapFrom(a => a.Iecirb.IECIRBContactNumber))
               .ForMember(x => x.IECIRBContactName, x => x.MapFrom(a => a.Iecirb.IECIRBContactName))
               .ForMember(x => x.IECIRBContactEmail, x => x.MapFrom(a => a.Iecirb.IECIRBContactEmail))
               .ReverseMap();

            CreateMap<InvestigatorContactDetail, InvestigatorContactDetailGridDto>().ReverseMap();
            //.ForMember(x => x.SecurityRole, x => x.MapFrom(a => a.SecurityRole.RoleShortName))
            //.ForMember(x => x.ContactType, x => x.MapFrom(a => a.ContactType.TypeName))

            CreateMap<Holiday, HolidayGridDto>()
                .ForMember(x => x.HolidayType, x => x.MapFrom(a => a.HolidayType)).ReverseMap();
        }
    }
}
