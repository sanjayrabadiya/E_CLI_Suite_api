﻿using AutoMapper;
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
using GSC.Data.Entities.SupplyManagement;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Data.Entities.Barcode;
using GSC.Data.Dto.Barcode;
using System.Configuration;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Dto.LabReportManagement;
using GSC.Data.Entities.LabReportManagement;
using GSC.Data.Entities.IDVerificationSystem;
using GSC.Data.Dto.IDVerificationSystem;
using GSC.Helper;

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
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName)).ReverseMap();
            CreateMap<City, CityGridDto>()
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.State.StateName))
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.State.Country.CountryName)).ReverseMap();
            CreateMap<Country, CountryGridDto>().ReverseMap();
            CreateMap<ContactType, ContactTypeGridDto>().ReverseMap();
            CreateMap<ClientType, ClientTypeGridDto>().ReverseMap();
            CreateMap<Department, DepartmentGridDto>().ReverseMap();
            CreateMap<DocumentType, DocumentTypeGridDto>().ReverseMap();
            CreateMap<DocumentName, DocumentNameGridDto>()
                .ForMember(x => x.PickFromTypeName, a => a.MapFrom(x => x.PickFromType.GetDescription()))
                .ReverseMap();
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
            CreateMap<Unit, UnitGridDto>()
                .ForMember(x => x.AppScreenName, a => a.MapFrom(x => x.AppScreen.ScreenName))
                .ReverseMap();
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
                 .ForMember(x => x.CollectionValue, x => x.MapFrom(a => string.Join(", ", a.Values.AsEnumerable().Select(x => x.ValueName))))
                 .ForMember(x => x.DateValidate, x => x.MapFrom(a => a.DateValidate)).ReverseMap();
            CreateMap<PatientStatus, PatientStatusGridDto>().ReverseMap();
            CreateMap<VisitStatus, VisitStatusGridDto>().ReverseMap();
            CreateMap<SecurityRole, SecurityRoleGridDto>().ReverseMap();
            CreateMap<Iecirb, IecirbGridDto>()
                .ReverseMap();
            CreateMap<User, UserGridDto>()
                .ForMember(x => x.Role, x => x.MapFrom(a => string.Join(", ", a.UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList())))
                .ForMember(x => x.ScreeningNumber, x => x.MapFrom(a => a.Randomization.ScreeningNumber))
                .ForMember(x => x.CompanyName, x => x.MapFrom(a => a.Company.CompanyName))
                .ForMember(x => x.RandomizationNumber, x => x.MapFrom(a => a.Randomization.RandomizationNumber))
                .ForMember(x => x.DateOfScreening, x => x.MapFrom(a => a.Randomization.DateOfScreening))
                .ForMember(x => x.DateOfRandomization, x => x.MapFrom(a => a.Randomization.DateOfRandomization))
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
                .ForMember(x => x.PatientStatusName, x => x.MapFrom(a => a.PatientStatusId.GetDescription()))
                .ForMember(x => x.IsFirstTime, x => x.MapFrom(a => a.User == null ? false : a.User.IsFirstTime))
                .ForMember(x => x.GenderfactorName, x => x.MapFrom(a => a.Genderfactor.GetDescription()))
                .ForMember(x => x.DiatoryfactorName, x => x.MapFrom(a => a.Diatoryfactor.GetDescription()))
                .ForMember(x => x.JointfactorName, x => x.MapFrom(a => a.Jointfactor.GetDescription()))
                .ForMember(x => x.EligibilityfactorName, x => x.MapFrom(a => a.Eligibilityfactor.GetDescription()))
                .ReverseMap();

            CreateMap<EtmfProjectWorkPlace, ETMFWorkplaceGridDto>()
                 .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ReverseMap();

            CreateMap<Data.Entities.Master.Project, ProjectGridDto>()
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
                .ForMember(x => x.ParentProjectCode, x => x.MapFrom(a => a.ChildProject.Where(x => x.DeletedDate == null).Select(r => r.ProjectCode).FirstOrDefault()))
                .ForMember(x => x.NoofSite, x => x.MapFrom(a => a.ChildProject.Count(x => x.DeletedDate == null)))
                .ReverseMap();

            CreateMap<ManageSite, ManageSiteGridDto>()
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.City.State.Country.CountryName))
                .ForMember(x => x.StateName, x => x.MapFrom(a => a.City.State.StateName))
                .ForMember(x => x.CityName, x => x.MapFrom(a => a.City.CityName))
                .ForMember(x => x.SiteAddresses, x => x.MapFrom(a => a.ManageSiteAddress.Select(s => s.SiteAddress).ToList()))
                .ForMember(x => x.TherapeuticIndicationName, x => x.MapFrom(a => string.Join(", ", a.ManageSiteRole.Where(x => x.DeletedDate == null).Select(s => s.TrialType.TrialTypeName).ToList())))
                .ReverseMap();
            CreateMap<VariableTemplate, VariableTemplateGridDto>()
               .ForMember(x => x.DomainName, x => x.MapFrom(a => a.Domain.DomainName))
               .ForMember(x => x.ActivityMode, x => x.MapFrom(a => a.ActivityMode.GetDescription()))
               .ForMember(x => x.ModuleName, x => x.MapFrom(a => a.AppScreen.ScreenName))
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
               .ForMember(x => x.IECIRBName, x => x.MapFrom(a => string.Join(", ", a.ManageSite.Iecirb.AsEnumerable().Select(x => x.IECIRBName))))
               .ReverseMap();

            CreateMap<ProjectDesignVisitStatus, ProjectDesignVisitStatusGridDto>()
              .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
              .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.ProjectDesignVariable.ProjectDesignTemplate.TemplateName))
              .ForMember(x => x.ProjectDesignVariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
              .ForMember(x => x.VisitStatus, x => x.MapFrom(a => a.VisitStatusId.GetDescription()))
              .ReverseMap();

            CreateMap<ProjectDesignTemplateNote, ProjectDesignTemplateNoteGridDto>()
               .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName)).ReverseMap();
            CreateMap<EconsentSetup, EconsentSetupGridDto>()
              .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
              .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
              .ForMember(x => x.DocumentStatus, x => x.MapFrom(a => a.DocumentStatusId.GetDescription()))
              .ReverseMap();

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
            CreateMap<ResourceType, ResourceTypeGridDto>()
                .ForMember(x => x.ResourceType, x => x.MapFrom(a => a.ResourceTypes.GetDescription()))
                .ForMember(x => x.ResourceSubType, x => x.MapFrom(a => a.ResourceSubType.GetDescription()))
                .ForMember(x => x.Role, x => x.MapFrom(a => a.Role.RoleName))
                .ForMember(x => x.User, x => x.MapFrom(a => a.User.FirstName + ' ' + a.User.LastName))
                .ForMember(x => x.Unit, x => x.MapFrom(a => a.Unit.UnitName))
                .ForMember(x => x.Designation, x => x.MapFrom(a => a.Designation.NameOFDesignation))
                .ForMember(x => x.YersOfExperience, x => x.MapFrom(a => a.Designation.YersOfExperience))
                .ForMember(x => x.CurrencyType, x => x.MapFrom(a => a.Currency != null ? a.Currency.CurrencyName + "-" + a.Currency.CurrencySymbol + "   -" + a.Currency.Country.CountryName : ""))
                .ReverseMap();
            CreateMap<TaskTemplate, TaskTemplateGridDto>().ReverseMap();

            CreateMap<TaskMaster, TaskMasterGridDto>().ReverseMap();
            CreateMap<StudyPlan, StudyPlanGridDto>()
                   .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                   .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectName))
                   .ForMember(x => x.GlobalCurrency, x => x.MapFrom(a => a.Currency.CurrencyName + " - " + a.Currency.CurrencySymbol))
                   .ForMember(x => x.GlobalCurrencySymbol, x => x.MapFrom(a => a.Currency.CurrencySymbol))
                   .ForMember(x => x.TotalCost, x => x.MapFrom(a => a.TotalCost == null ? 0 : a.TotalCost))
                  .ReverseMap();
            CreateMap<Activity, ActivityGridDto>()
                 .ForMember(x => x.ActivityName, x => x.MapFrom(a => a.CtmsActivity.ActivityName))
                 .ForMember(x => x.ModuleName, x => x.MapFrom(a => a.AppScreen.ScreenName))
                .ReverseMap();

            CreateMap<TaskMaster, TaskMasterGridDto>()
                .ForMember(x => x.Predecessor, x => x.MapFrom(a => a.DependentTaskId > 0 ? a.DependentTaskId + "" + a.ActivityType + "+" + a.OffSet : ""))
                .ForMember(x => x.RefrenceTypes, x => x.MapFrom(a => string.Join(", ", a.RefrenceTypes.Where(x => x.DeletedDate == null).Select(s => s.RefrenceType.GetDescription()).ToList())))
                .ReverseMap();
            CreateMap<RefrenceTypes, TaskMasterGridDto>().ReverseMap();

            CreateMap<StudyPlanTask, StudyPlanTaskDto>()
                           .ForMember(x => x.Predecessor, x => x.MapFrom(a => a.DependentTaskId > 0 ? a.DependentTaskId + "" + a.ActivityType + "+" + a.OffSet : ""))
                           .ForMember(x => x.IsManual, x => x.MapFrom(a => a.ParentId != 0))
                           .ForMember(x => x.RefrenceTypeName, x => x.MapFrom(a => a.RefrenceType.GetDescription()))
                           .ForMember(x => x.EndDateDay, x => x.MapFrom(a => a.EndDate))
                           .ForMember(x => x.StartDateDay, x => x.MapFrom(a => a.StartDate))
                           .ForMember(x => x.DurationDay, x => x.MapFrom(a => a.Duration))
                           .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
                           .ForMember(x => x.ParentProjectId, x => x.MapFrom(a => a.StudyPlan.Project.ParentProjectId))
                           .ForMember(x => x.StudayName, x => x.MapFrom(a => a.StudyPlan.Project.ProjectCode))
                           .ForMember(x => x.SiteName, x => x.MapFrom(a => a.StudyPlan.Project.ManageSite.SiteName))
                           .ForMember(x => x.Site, x => x.MapFrom(a => a.StudyPlan.Project.ManageSite.SiteName))
                           .ForMember(x => x.DependenceTaskName, x => x.MapFrom(a => a.TaskName + " - " + (a.IsCountry ? a.Country.CountryName : a.StudyPlan.Project.ProjectCode ?? a.StudyPlan.Project.ManageSite.SiteName)))
                          .ReverseMap();


            CreateMap<StudyPlanTaskResource, StudyPlanTaskResourceGridDto>()
                           .ForMember(x => x.RoleName, x => x.MapFrom(a => a.SecurityRole.RoleShortName))
                           .ForMember(x => x.UserName, x => x.MapFrom(a => a.User.FirstName + ' ' + a.User.LastName))
                          .ReverseMap();
            CreateMap<HolidayMaster, HolidayMasterGridDto>()
                .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.IsSite ? a.Project.ProjectCode == null ? a.Project.ManageSite.SiteName : a.Project.ProjectCode + " - " + a.Project.ManageSite.SiteName : ""))
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.IsSite ? Convert.ToString(a.Project.ParentProjectId) : a.Project.ProjectCode))
                .ReverseMap();
            CreateMap<SupplyLocation, SupplyLocationGridDto>().ReverseMap();

            CreateMap<CentralDepot, CentralDepotGridDto>()
         .ForMember(x => x.DepotType, x => x.MapFrom(a => a.DepotType.GetDescription()))
         .ForMember(x => x.SupplyLocation, x => x.MapFrom(a => a.SupplyLocation.LocationName))
         .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectCode))
         .ForMember(x => x.Country, x => x.MapFrom(a => a.Country.CountryName)).ReverseMap();


            CreateMap<StudyVersion, StudyVersionGridDto>()
                .ForMember(x => x.StudyName, x => x.MapFrom(a => a.ProjectDesign.Project.ProjectCode))
                .ForMember(x => x.VersionStatus, x => x.MapFrom(a => a.VersionStatus.GetDescription()))
                .ForMember(x => x.GoLiveBy, x => x.MapFrom(a => a.GoLiveByUser.UserName))
                .ForMember(x => x.PatientStatus, x => x.MapFrom(a => string.Join(", ", a.StudyVersionStatus.AsEnumerable().Select(x => x.PatientStatusId.GetDescription()))))
                .ReverseMap();

            CreateMap<WeekEndMaster, WeekEndGridDto>()
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.IsSite == true ? Convert.ToString(a.Project.ParentProjectId) : a.Project.ProjectCode))
                .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.IsSite == true ? a.Project.ProjectCode == null ? a.Project.ManageSite.SiteName : a.Project.ProjectCode + " - " + a.Project.ManageSite.SiteName : ""))
                .ForMember(x => x.AllWeekOff, x => x.MapFrom(a => a.AllWeekOff.GetDescription()))
                .ForMember(x => x.Frequency, x => x.MapFrom(a => a.Frequency.GetDescription()))
                .ReverseMap();
            CreateMap<EconsentSectionReference, EconcentSectionRefrenceDetailListDto>().ReverseMap();
            CreateMap<EconsentSectionReference, EconsentSectionReferenceDto>().ReverseMap();
            CreateMap<VolunteerBlockHistory, VolunteerBlockHistoryGridDto>()
               .ForMember(x => x.CategoryName, x => x.MapFrom(a => a.BlockCategory.BlockCategoryName))
               .ReverseMap();

            CreateMap<PharmacyStudyProductType, PharmacyStudyProductTypeGridDto>()
              .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectCode))
              .ForMember(x => x.ProductType, x => x.MapFrom(a => a.ProductType.ProductTypeName))
              .ForMember(x => x.ProductUnitType, x => x.MapFrom(a => a.ProductUnitType.GetDescription()))
              .ReverseMap();

            CreateMap<ProductReceipt, ProductReceiptGridDto>()
              .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.Project.ProjectCode))
              .ForMember(x => x.PharmacyStudyProductType, x => x.MapFrom(a => a.PharmacyStudyProductType.ProductType.ProductTypeName))
              .ForMember(x => x.StorageArea, x => x.MapFrom(a => a.CentralDepot.StorageArea))
              .ForMember(x => x.Status, x => x.MapFrom(a => a.Status.GetDescription()))
              .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
              .ReverseMap();

            CreateMap<ProductVerification, ProductVerificationGridDto>().ReverseMap();
            CreateMap<LanguageConfiguration, LanguageConfigurationGridDto>().ReverseMap();
            CreateMap<LanguageConfigurationDetails, LanguageConfigurationDetailsGridDto>()
               .ForMember(x => x.LanguageName, y => y.MapFrom(a => a.Language.LanguageName))
               .ReverseMap();

            CreateMap<BarcodeConfig, BarcodeConfigGridDto>()
                .ForMember(x => x.BarcodeTypeName, x => x.MapFrom(a => a.BarcodeType.BarcodeTypeName))
                .ForMember(x => x.ModuleName, x => x.MapFrom(a => a.AppScreen.ScreenName))
                .ForMember(x => x.PageName, x => x.MapFrom(a => a.Page.ScreenName))
                .ForMember(x => x.BarcodeCombination, x => x.MapFrom(a => string.Join(",", a.BarcodeCombination.Where(x => x.DeletedDate == null).Select(s => s.TableFieldName.LabelName).ToList())))
                .ForMember(x => x.BarcodeDisplayInfo, x => x.MapFrom(a => string.Join(",", a.BarcodeDisplayInfo.Where(x => x.DeletedDate == null).Select(s => s.TableFieldName.LabelName).ToList())))
                .ReverseMap();

            CreateMap<SupplyManagementConfiguration, SupplyManagementConfigurationGridDto>()
                .ForMember(x => x.PageName, x => x.MapFrom(a => a.AppScreen.ScreenName))
                .ForMember(x => x.TemplateName, x => x.MapFrom(a => a.VariableTemplate.TemplateName))
                .ReverseMap();

            CreateMap<Data.Entities.LabManagement.LabManagementConfiguration, LabManagementConfigurationGridDto>()
                .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
                .ForMember(x => x.ParentProjectId, x => x.MapFrom(a => a.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id))
                .ForMember(x => x.ProjectDesignVisitName, x => x.MapFrom(a => a.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
                .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
                .ForMember(x => x.ApproveProfile, x => x.MapFrom(a => a.SecurityRole.RoleName))
                .ReverseMap();

            CreateMap<LabManagementUploadData, LabManagementUploadDataGridDto>()
                .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.LabManagementConfiguration.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
                .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ForMember(x => x.Status, x => x.MapFrom(a => a.LabManagementUploadStatus.GetDescription()))
                .ForMember(x => x.ProjectDesignVisitName, x => x.MapFrom(a => a.LabManagementConfiguration.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
                .ForMember(x => x.ProjectDesignTemplateName, x => x.MapFrom(a => a.LabManagementConfiguration.ProjectDesignTemplate.TemplateName))
                .ForMember(x => x.Reason, x => x.MapFrom(a => a.AuditReason.ReasonName))
                .ForMember(x => x.SecurityRoleId, x => x.MapFrom(a => a.LabManagementConfiguration.SecurityRoleId))
                .ForMember(x => x.LabManagementUploadExcelDatas, x => x.MapFrom(a => a.LabManagementUploadExcelDatas))
                .ReverseMap();

            CreateMap<SyncConfigurationMaster, SyncConfigurationMasterGridDto>()
                .ForMember(x => x.ReportName, x => x.MapFrom(a => a.ReportScreen.ReportName))
                .ReverseMap();

            CreateMap<FileSizeConfiguration, FileSizeConfigurationGridDto>()
                .ForMember(x => x.ScreenName, x => x.MapFrom(a => a.AppScreens.ScreenName))
                .ForMember(x => x.ScreenCode, x => x.MapFrom(a => a.AppScreens.ScreenCode)).ReverseMap();

            CreateMap<SupplyManagementUploadFile, SupplyManagementUploadFileGridDto>()
                .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ForMember(x => x.Country, x => x.MapFrom(a => a.Country.CountryName))
                .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Site.ProjectCode))
                .ForMember(x => x.Level, x => x.MapFrom(a => a.SupplyManagementUploadFileLevel.GetDescription()))
                .ForMember(x => x.StatusName, x => x.MapFrom(a => a.Status.GetDescription()))
                .ForMember(x => x.Reason, x => x.MapFrom(a => a.AuditReason.ReasonName)).ReverseMap();

            CreateMap<LabManagementVariableMapping, LabManagementVariableMappingGridDto>()
                .ForMember(x => x.ProjectDesignVariable, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
                .ForMember(x => x.AuditReason, x => x.MapFrom(a => a.AuditReason.ReasonName)).ReverseMap();

            CreateMap<StudyLevelForm, StudyLevelFormGridDto>()
                .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
                .ForMember(x => x.VariableTemplateName, x => x.MapFrom(a => a.VariableTemplate.TemplateName))
                .ForMember(x => x.Activity, x => x.MapFrom(a => a.Activity.CtmsActivity.ActivityName))
                .ForMember(x => x.AppScreenName, x => x.MapFrom(a => a.AppScreen.ScreenName)).ReverseMap();

            CreateMap<VerificationApprovalTemplateHistory, VerificationApprovalTemplateHistoryViewDto>()
               .ForMember(x => x.SendBy, x => x.MapFrom(a => a.User.UserName))
               .ForMember(x => x.AuditReason, x => x.MapFrom(a => a.AuditReason.ReasonName))
               .ForMember(x => x.Role, x => x.MapFrom(a => a.SecurityRole.RoleName))
               .ForMember(x => x.Status, x => x.MapFrom(a => a.Status.GetDescription())).ReverseMap();

            CreateMap<CtmsMonitoring, CtmsMonitoringGridDto>().ReverseMap();

            CreateMap<SupplyManagementRequest, SupplyManagementRequestGridDto>()
                  .ForMember(x => x.FromProjectCode, x => x.MapFrom(a => a.FromProject.ProjectCode))
                  .ForMember(x => x.ToProjectCode, x => x.MapFrom(a => a.ToProject.ProjectCode))
                  .ForMember(x => x.StudyProductTypeName, x => x.MapFrom(a => a.PharmacyStudyProductType.ProductType.ProductTypeName))
                  .ForMember(x => x.StudyProductTypeUnitName, x => x.MapFrom(a => a.PharmacyStudyProductType.ProductUnitType.GetDescription()))
                  .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
                  .ReverseMap();
            CreateMap<SupplyManagementShipment, SupplyManagementShipmentGridDto>()
                  .ForMember(x => x.FromProjectCode, x => x.MapFrom(a => a.SupplyManagementRequest.FromProject.ProjectCode))
                  .ForMember(x => x.ProjectId, x => x.MapFrom(a => a.SupplyManagementRequest.FromProject.ParentProjectId))
                  .ForMember(x => x.ToProjectCode, x => x.MapFrom(a => a.SupplyManagementRequest.ToProject.ProjectCode))
                  .ForMember(x => x.StatusName, x => x.MapFrom(a => a.Status.GetDescription()))
                  .ForMember(x => x.IsSiteRequest, x => x.MapFrom(a => a.SupplyManagementRequest.IsSiteRequest))
                  .ForMember(x => x.RequestQty, x => x.MapFrom(a => a.SupplyManagementRequest.RequestQty))
                  .ForMember(x => x.FromProjectId, x => x.MapFrom(a => a.SupplyManagementRequest.FromProjectId))
                  .ForMember(x => x.ToProjectId, x => x.MapFrom(a => a.SupplyManagementRequest.ToProjectId))
                  .ForMember(x => x.IsSiteRequest, x => x.MapFrom(a => a.SupplyManagementRequest.IsSiteRequest))
                  .ForMember(x => x.RequestBy, x => x.MapFrom(a => a.SupplyManagementRequest.CreatedByUser.UserName))
                  .ForMember(x => x.RequestDate, x => x.MapFrom(a => a.SupplyManagementRequest.CreatedDate))
                  .ForMember(x => x.StudyProductTypeName, x => x.MapFrom(a => a.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName))
                  .ForMember(x => x.StudyProductTypeUnitName, x => x.MapFrom(a => a.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType.GetDescription()))
                  .ReverseMap();

            CreateMap<SupplyManagementReceipt, SupplyManagementReceiptGridDto>()
                 .ForMember(x => x.FromProjectId, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.FromProjectId))
                 .ForMember(x => x.ToProjectId, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.ToProjectId))
                 .ForMember(x => x.FromProjectCode, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode))
                 .ForMember(x => x.ToProjectCode, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.ToProject.ProjectCode))
                 .ForMember(x => x.Status, x => x.MapFrom(a => a.SupplyManagementShipment.Status))
                 .ForMember(x => x.AuditReason, x => x.MapFrom(a => a.AuditReason.ReasonName))
                 .ForMember(x => x.ShipmentReasonOth, x => x.MapFrom(a => a.SupplyManagementShipment.ReasonOth))
                 .ForMember(x => x.StatusName, x => x.MapFrom(a => a.SupplyManagementShipment.Status.GetDescription()))
                 .ForMember(x => x.ApprovedQty, x => x.MapFrom(a => a.SupplyManagementShipment.ApprovedQty))
                 .ForMember(x => x.RequestQty, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.RequestQty))
                 .ForMember(x => x.ApproveRejectDateTime, x => x.MapFrom(a => a.SupplyManagementShipment.CreatedDate))
                 .ForMember(x => x.ShipmentNo, x => x.MapFrom(a => a.SupplyManagementShipment.ShipmentNo))
                 .ForMember(x => x.CourierName, x => x.MapFrom(a => a.SupplyManagementShipment.CourierName))
                 .ForMember(x => x.CourierDate, x => x.MapFrom(a => a.SupplyManagementShipment.CourierDate))
                 .ForMember(x => x.CourierTrackingNo, x => x.MapFrom(a => a.SupplyManagementShipment.CourierTrackingNo))
                 .ForMember(x => x.ApproveRejectBy, x => x.MapFrom(a => a.SupplyManagementShipment.CreatedByUser.UserName))
                 .ForMember(x => x.ProductUnitType, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType))
                 .ForMember(x => x.StudyProductTypeName, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName))
                 .ForMember(x => x.StudyProductTypeUnitName, x => x.MapFrom(a => a.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType.GetDescription()))
                 .ReverseMap();

            CreateMap<SupplyManagementRequest, SupplyManagementShipmentGridDto>()
                  .ForMember(x => x.FromProjectCode, x => x.MapFrom(a => a.FromProject.ProjectCode))
                  .ForMember(x => x.ToProjectCode, x => x.MapFrom(a => a.ToProject.ProjectCode))
                  .ForMember(x => x.SupplyManagementRequestId, x => x.MapFrom(a => a.Id))
                  .ForMember(x => x.RequestBy, x => x.MapFrom(a => a.CreatedByUser.UserName))
                  .ForMember(x => x.RequestDate, x => x.MapFrom(a => a.CreatedDate))
                  .ForMember(x => x.RequestDate, x => x.MapFrom(a => a.CreatedDate))
                  .ReverseMap();

            CreateMap<SupplyManagementShipment, SupplyManagementReceiptGridDto>()
                 .ForMember(x => x.FromProjectId, x => x.MapFrom(a => a.SupplyManagementRequest.FromProjectId))
                 .ForMember(x => x.ToProjectId, x => x.MapFrom(a => a.SupplyManagementRequest.ToProjectId))
                 .ForMember(x => x.FromProjectCode, x => x.MapFrom(a => a.SupplyManagementRequest.FromProject.ProjectCode))
                 .ForMember(x => x.ToProjectCode, x => x.MapFrom(a => a.SupplyManagementRequest.ToProject.ProjectCode))
                 .ForMember(x => x.Status, x => x.MapFrom(a => a.Status))
                 .ForMember(x => x.StatusName, x => x.MapFrom(a => a.Status.GetDescription()))
                 .ForMember(x => x.ApprovedQty, x => x.MapFrom(a => a.ApprovedQty))
                 .ForMember(x => x.ApproveRejectDateTime, x => x.MapFrom(a => a.CreatedDate))
                 .ForMember(x => x.ApproveRejectBy, x => x.MapFrom(a => a.CreatedByUser.UserName))
                 .ForMember(x => x.ShipmentReasonOth, x => x.MapFrom(a => a.ReasonOth))
                 .ForMember(x => x.ShipmentNo, x => x.MapFrom(a => a.ShipmentNo))
                 .ForMember(x => x.CourierName, x => x.MapFrom(a => a.CourierName))
                 .ForMember(x => x.CourierDate, x => x.MapFrom(a => a.CourierDate))
                 .ForMember(x => x.CourierTrackingNo, x => x.MapFrom(a => a.CourierTrackingNo))
                 .ForMember(x => x.SupplyManagementShipmentId, x => x.MapFrom(a => a.Id))
                 .ForMember(x => x.ProductUnitType, x => x.MapFrom(a => a.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType))
                 .ForMember(x => x.StudyProductTypeName, x => x.MapFrom(a => a.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName))
                 .ForMember(x => x.StudyProductTypeUnitName, x => x.MapFrom(a => a.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType.GetDescription()))
                 .ReverseMap();

            CreateMap<PageConfiguration, PageConfigurationGridDto>()
               .ForMember(x => x.ActualFieldName, a => a.MapFrom(m => m.PageConfigurationFields.FieldName))
               .ReverseMap();
            CreateMap<PageConfigurationFields, PageConfigurationFieldsGridDto>()
                .ForMember(x => x.AppScreen, a => a.MapFrom(m => m.AppScreens.ScreenName)).ReverseMap();

            CreateMap<CtmsActionPoint, CtmsActionPointGridDto>()
                .ForMember(x => x.StatusName, a => a.MapFrom(m => m.Status.GetDescription()))
                .ForMember(x => x.Activity, a => a.MapFrom(m => m.CtmsMonitoring.StudyLevelForm.Activity.CtmsActivity.ActivityName))
                .ForMember(x => x.QueryDate, a => a.MapFrom(m => m.CreatedDate))
                .ForMember(x => x.QueryBy, a => a.MapFrom(m => m.CreatedByUser.UserName))
                .ForMember(x => x.ResponseBy, a => a.MapFrom(m => m.User.UserName))
                .ForMember(x => x.CloseBy, a => a.MapFrom(m => m.CloseUser.UserName))
                .ReverseMap();

            CreateMap<CtmsMonitoringStatus, CtmsMonitoringStatusGridDto>()
                .ForMember(x => x.StatusName, a => a.MapFrom(m => m.Status.GetDescription()))
                .ReverseMap();

            CreateMap<SendEmailOnVariableChangeSetting, SendEmailOnVariableChangeSettingGridDto>()
               .ForMember(x => x.ProjectCode, a => a.MapFrom(m => m.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
               .ForMember(x => x.ProjectDesignVisit, a => a.MapFrom(m => m.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
               .ForMember(x => x.ProjectDesignTemplate, a => a.MapFrom(m => m.ProjectDesignVariable.ProjectDesignTemplate.TemplateName))
               .ForMember(x => x.ProjectDesignVariable, a => a.MapFrom(m => m.ProjectDesignVariable.VariableName))
               .ForMember(x => x.Email, a => a.MapFrom(m => m.Email))
               .ForMember(x => x.EmailTemplate, a => a.MapFrom(m => m.EmailTemplate))
               .ReverseMap();

            CreateMap<SupplyManagementAllocation, SupplyManagementAllocationGridDto>()
             .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
             .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
             .ForMember(x => x.TemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
             .ForMember(x => x.TypeName, x => x.MapFrom(a => a.Type.GetDescription()))
             .ForMember(x => x.VariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
             .ReverseMap();

            CreateMap<SupplyManagementKITDetail, SupplyManagementKITGridDto>()
               .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.SupplyManagementKIT.Project.ProjectCode))
               .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.SupplyManagementKIT.Site.ProjectCode))
               .ForMember(x => x.VisitName, x => x.MapFrom(a => a.SupplyManagementKIT.ProjectDesignVisit.DisplayName))
               .ForMember(x => x.ProductTypeName, x => x.MapFrom(a => a.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode))
               .ForMember(x => x.KitStatus, x => x.MapFrom(a => a.Status.GetDescription()))
               .ForMember(x => x.NoOfImp, x => x.MapFrom(a => a.SupplyManagementKIT.NoOfImp))
               .ForMember(x => x.NoofPatient, x => x.MapFrom(a => a.SupplyManagementKIT.NoofPatient))
               .ForMember(x => x.ProjectId, x => x.MapFrom(a => a.SupplyManagementKIT.ProjectId))
               .ForMember(x => x.Dose, x => x.MapFrom(a => a.SupplyManagementKIT.Dose))
               .ForMember(x => x.IpAddress, x => x.MapFrom(a => a.SupplyManagementKIT.IpAddress))
               .ForMember(x => x.TimeZone, x => x.MapFrom(a => a.SupplyManagementKIT.TimeZone))
               .ForMember(x => x.Reason, x => x.MapFrom(a => a.SupplyManagementKIT.AuditReason.ReasonName)).ReverseMap();

            CreateMap<SupplyManagementUploadFileDetail, SupplyManagementUploadFileDetailDto>()
             .ForMember(x => x.SiteName, x => x.MapFrom(a => a.Randomization.Project.ProjectCode))
             .ForMember(x => x.ScreeningNumber, x => x.MapFrom(a => a.Randomization.ScreeningNumber))
             .ReverseMap();

            CreateMap<EconsentGlossary, EconsentGlossaryGridDto>()
                .ForMember(x => x.Project, x => x.MapFrom(a => a.EconsentSetup.Project.ProjectCode))
                .ForMember(x => x.Document, x => x.MapFrom(a => a.EconsentSetup.DocumentName))
                .ReverseMap();
            CreateMap<SupplyManagementFector, SupplyManagementFectorGridDto>()
                .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                .ReverseMap();
            CreateMap<SupplyManagementKitAllocationSettings, SupplyManagementKitAllocationSettingsGridDto>()
               .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
               .ReverseMap();
            CreateMap<SupplyManagementKitNumberSettings, SupplyManagementKitNumberSettingsGridDto>()
               .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
               .ForMember(x => x.KitCreationTypeName, x => x.MapFrom(a => a.KitCreationType.GetDescription()))
               .ReverseMap();
            CreateMap<SupplyManagementVisitKITDetail, SupplyManagementVisitKITDetailGridDto>()
              .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
              .ForMember(x => x.ParentProjectId, x => x.MapFrom(a => a.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id))
              .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Randomization.Project.ProjectCode))
              .ForMember(x => x.ProjectId, x => x.MapFrom(a => a.Randomization.Project.Id))
              .ForMember(x => x.ReasonName, x => x.MapFrom(a => a.AuditReason.ReasonName))
              .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
              .ForMember(x => x.ScreeningNo, x => x.MapFrom(a => a.Randomization.ScreeningNumber))
              .ForMember(x => x.RandomizationNo, x => x.MapFrom(a => a.Randomization.RandomizationNumber))
              .ForMember(x => x.SupplyManagementKITDetailId, x => x.MapFrom(a => a.SupplyManagementKITDetailId))
              .ReverseMap();

            CreateMap<SupplyManagementKITDetailHistory, SupplyManagementKITDetailHistoryDto>()
            .ForMember(x => x.StatusName, x => x.MapFrom(a => a.Status.GetDescription()))
            .ReverseMap();

            CreateMap<ProjectArtificateDocumentHistory, ProjectArtificateDocumentHistoryDto>();
            CreateMap<ProjectSubSecArtificateDocumentHistory, ProjectSubSecArtificateDocumentHistoryDto>();

            CreateMap<SupplyManagementEmailConfiguration, SupplyManagementEmailConfigurationGridDto>()
              .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
              .ForMember(x => x.TriggersName, x => x.MapFrom(a => a.Triggers.GetDescription()))
              .ReverseMap();

            CreateMap<SupplyManagementEmailConfigurationDetail, SupplyManagementEmailConfigurationDetailGridDto>()
              .ForMember(x => x.RoleName, x => x.MapFrom(a => a.SecurityRole.RoleName))
              .ForMember(x => x.UserName, x => x.MapFrom(a => a.Users.UserName))
              .ReverseMap();

            CreateMap<SupplyManagementEmailConfigurationDetailHistory, SupplyManagementEmailConfigurationDetailHistoryGridDto>()
              .ForMember(x => x.RoleName, x => x.MapFrom(a => a.SupplyManagementEmailConfigurationDetail.SecurityRole.RoleName))
              .ForMember(x => x.UserName, x => x.MapFrom(a => a.SupplyManagementEmailConfigurationDetail.Users.UserName))
              .ForMember(x => x.TriggerName, x => x.MapFrom(a => a.SupplyManagementEmailConfigurationDetail.SupplyManagementEmailConfiguration.Triggers.GetDescription()))
              .ReverseMap();

            CreateMap<SupplyManagementKITSeries, SupplyManagementKITSeriesGridDto>()
             .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.Project.ProjectCode))
             .ForMember(x => x.RandomizationNo, x => x.MapFrom(a => a.Randomization.RandomizationNumber))
             .ForMember(x => x.statusName, x => x.MapFrom(a => a.Status.GetDescription()))
             .ReverseMap();

            CreateMap<SupplyManagementKITSeriesDetail, SupplyManagementKITSeriesDetailGridDto>()
            .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
            .ForMember(x => x.RandomizationNo, x => x.MapFrom(a => a.Randomization.RandomizationNumber))
            .ForMember(x => x.KitNo, x => x.MapFrom(a => a.SupplyManagementKITSeries.KitNo))
            .ForMember(x => x.ProductType, x => x.MapFrom(a => a.PharmacyStudyProductType.ProductType.ProductTypeCode))
            .ReverseMap();

            CreateMap<SupplyManagementKITSeriesDetailHistory, SupplyManagementKITSeriesDetailHistoryGridDto>()
              .ForMember(x => x.KitNo, x => x.MapFrom(a => a.SupplyManagementKITSeries.KitNo))
              .ForMember(x => x.StatusName, x => x.MapFrom(a => a.Status.GetDescription()))
              .ReverseMap();

            CreateMap<SupplyManagementVisitKITSequenceDetail, SupplyManagementVisitKITDetailGridDto>()
             .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode))
             .ForMember(x => x.ParentProjectId, x => x.MapFrom(a => a.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id))
             .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Randomization.Project.ProjectCode))
             .ForMember(x => x.ProjectId, x => x.MapFrom(a => a.Randomization.Project.Id))
             .ForMember(x => x.ReasonName, x => x.MapFrom(a => a.AuditReason.ReasonName))
             .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
             .ForMember(x => x.ScreeningNo, x => x.MapFrom(a => a.Randomization.ScreeningNumber))
             .ForMember(x => x.RandomizationNo, x => x.MapFrom(a => a.Randomization.RandomizationNumber))
             .ForMember(x => x.SupplyManagementKITDetailId, x => x.MapFrom(a => a.SupplyManagementKITSeriesdetailId))
             .ReverseMap();

            CreateMap<Randomization, SupplyManagementUnblindTreatmentGridDto>()
             .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Project.ProjectCode))
             .ForMember(x => x.ScreeningNo, x => x.MapFrom(a => a.ScreeningNumber))
             .ForMember(x => x.RandomizationNo, x => x.MapFrom(a => a.RandomizationNumber))
             .ForMember(x => x.ProjectId, x => x.MapFrom(a => a.ProjectId))
             .ForMember(x => x.ParentProjectId, x => x.MapFrom(a => a.Project.ParentProjectId))
             .ForMember(x => x.RandomizationId, x => x.MapFrom(a => a.Id))
             .ReverseMap();

            CreateMap<SupplyManagementFactorMapping, SupplyManagementFactorMappingGridDto>()
           .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
           .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
           .ForMember(x => x.TemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
           .ForMember(x => x.VariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
           .ForMember(x => x.Reason, x => x.MapFrom(a => a.AuditReason.ReasonName))
           .ForMember(x => x.FactorName, x => x.MapFrom(a => a.Factor.GetDescription()))
           .ReverseMap();

            CreateMap<PKBarcode, PKBarcodeGridDto>()
            .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectName))
            .ForMember(x => x.Site, x => x.MapFrom(a => a.Site.ManageSite.SiteName))
            .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
            .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Site.ProjectCode))
            .ForMember(x => x.Visit, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
            .ForMember(x => x.Template, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
            .ForMember(x => x.VolunteerName, x => x.MapFrom(a => a.Volunteer.FirstName + " " + a.Volunteer.LastName))
            .ForMember(x => x.BarcodeType, x => x.MapFrom(a => a.BarcodeType.BarcodeTypeName))
            .ForMember(x => x.PKBarcodeOption, x => x.MapFrom(a => a.PKBarcodeOption.GetDescription()))
            .ForMember(x => x.isBarcodeGenerated, x => x.MapFrom(a => a.BarcodeDate != null))
            .ReverseMap();

            CreateMap<SampleBarcode, SampleBarcodeGridDto>()
           .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectName))
           .ForMember(x => x.Site, x => x.MapFrom(a => a.Site.ManageSite.SiteName))
           .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
           .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Site.ProjectCode))
           .ForMember(x => x.Visit, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
           .ForMember(x => x.Template, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
           .ForMember(x => x.VolunteerName, x => x.MapFrom(a => a.Volunteer.FirstName + " " + a.Volunteer.LastName))
           .ForMember(x => x.BarcodeType, x => x.MapFrom(a => a.BarcodeType.BarcodeTypeName))
           .ForMember(x => x.PKBarcodeOption, x => x.MapFrom(a => a.PKBarcodeOption.GetDescription()))
           .ForMember(x => x.isBarcodeGenerated, x => x.MapFrom(a => a.BarcodeDate != null))
           .ReverseMap();


            CreateMap<DossingBarcode, DossingBarcodeGridDto>()
           .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectName))
           .ForMember(x => x.Site, x => x.MapFrom(a => a.Site.ManageSite.SiteName))
           .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
           .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Site.ProjectCode))
           .ForMember(x => x.Visit, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
           .ForMember(x => x.Template, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
           .ForMember(x => x.VolunteerName, x => x.MapFrom(a => a.Volunteer.FirstName + " " + a.Volunteer.LastName))
           .ForMember(x => x.BarcodeType, x => x.MapFrom(a => a.BarcodeType.BarcodeTypeName))
           .ForMember(x => x.PKBarcodeOption, x => x.MapFrom(a => a.PKBarcodeOption.GetDescription()))
           .ForMember(x => x.isBarcodeGenerated, x => x.MapFrom(a => a.BarcodeDate != null))
           .ReverseMap();

            CreateMap<ManageSiteAddress, ManageSiteAddressGridDto>()
           .ForMember(x => x.City, x => x.MapFrom(a => a.City.CityName))
           .ForMember(x => x.State, x => x.MapFrom(a => a.City.State.StateName))
           .ForMember(x => x.Country, x => x.MapFrom(a => a.City.State.Country.CountryName))
           .ReverseMap();

            CreateMap<ProjectSiteAddress, ProjectSiteAddressGridDto>()
           .ForMember(x => x.City, x => x.MapFrom(a => a.ManageSiteAddress.City.CityName))
           .ForMember(x => x.State, x => x.MapFrom(a => a.ManageSiteAddress.City.State.StateName))
           .ForMember(x => x.Country, x => x.MapFrom(a => a.ManageSiteAddress.City.State.Country.CountryName))
           .ForMember(x => x.ContactNumber, x => x.MapFrom(a => a.ManageSiteAddress.ContactNumber))
           .ForMember(x => x.ContactName, x => x.MapFrom(a => a.ManageSiteAddress.ContactName))
           .ForMember(x => x.SiteAddress, x => x.MapFrom(a => a.ManageSiteAddress.SiteAddress))
           .ForMember(x => x.SiteEmail, x => x.MapFrom(a => a.ManageSiteAddress.SiteEmail))
           .ForMember(x => x.Facilities, x => x.MapFrom(a => a.ManageSiteAddress.Facilities))
           .ReverseMap();

            CreateMap<CentrifugationDetails, CentrifugationDetailsGridDto>()
           .ForMember(x => x.StudyCode, x => x.MapFrom(a => a.PKBarcode.Project.ProjectCode))
           .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.PKBarcode.Site.ProjectCode))
           .ForMember(x => x.RandomizationNumber, x => x.MapFrom(a => a.PKBarcode.Volunteer.RandomizationNumber))
           .ForMember(x => x.PKBarcode, x => x.MapFrom(a => a.PKBarcode.BarcodeString))
           .ForMember(x => x.CentrifugationByUser, x => x.MapFrom(a => a.Centrifugationed.UserName))
           .ForMember(x => x.ReCentrifugationByUser, x => x.MapFrom(a => a.ReCentrifugation.UserName))
           .ForMember(x => x.AuditReason, x => x.MapFrom(a => a.AuditReason.ReasonName))
           .ForMember(x => x.Status, x => x.MapFrom(a => a.Status.GetDescription()))
           .ReverseMap();

            CreateMap<SupplyManagementApproval, SupplyManagementApprovalGridDto>()
             .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
             .ForMember(x => x.ApprovalTypeName, x => x.MapFrom(a => a.ApprovalType.GetDescription()))
             .ReverseMap();
            CreateMap<VendorManagement, VendorManagementGridDto>()
                .ForMember(x => x.VendorManagementAudit, x => x.MapFrom(a => a.VendorManagementAuditId.GetDescription()))
                .ReverseMap();

            CreateMap<PlanMetrics, PlanMetricsGridDto>()
                   .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                   .ForMember(x => x.MetricsType, x => x.MapFrom(a => a.MetricsType.GetDescription())).ReverseMap();
            CreateMap<OverTimeMetrics, OverTimeMetricsGridDto>()
                    .ForMember(x => x.SiteName, x => x.MapFrom(a => a.Project.ProjectCode))
                    .ForMember(x => x.PlanningType, x => x.MapFrom(a => a.PlanningType.GetDescription())).ReverseMap();

            CreateMap<EmailConfigurationEditCheck, EmailConfigurationEditCheckGridDto>()
             .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
             .ReverseMap();

            CreateMap<EmailConfigurationEditCheckDetail, EmailConfigurationEditCheckDetailGridDto>()
            .ForMember(x => x.TemplateName, x => x.MapFrom(a => a.ProjectDesignTemplate.TemplateName))
            .ForMember(x => x.VariableName, x => x.MapFrom(a => a.ProjectDesignVariable.VariableName))
            .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignTemplate.ProjectDesignVisit.DisplayName))
            .ForMember(x => x.OperatorName, x => x.MapFrom(a => a.Operator.GetDescription()))
            .ReverseMap();

            CreateMap<EmailConfigurationEditCheckSendMailHistory, EmailConfigurationEditCheckMailHistoryGridDto>()
           .ForMember(x => x.Email, x => x.MapFrom(a => a.User.Email))
           .ForMember(x => x.RoleName, x => x.MapFrom(a => a.Role.RoleName))
           .ReverseMap();

            CreateMap<VariableLabelLanguage, VariableLabelLanguageGridDto>()
           .ForMember(x => x.Note, x => x.MapFrom(a => a.ProjectDesignVariable.Note))
           .ForMember(x => x.LanguageName, x => x.MapFrom(a => a.Language.LanguageName))
           .ReverseMap();

            CreateMap<VisitEmailConfiguration, VisitEmailConfigurationGridDto>()
              .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
              .ForMember(x => x.UserRoles, x => x.MapFrom(a => string.Join(",", a.VisitEmailConfigurationRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleShortName).ToList())))
              .ForMember(x => x.VisitStatus, x => x.MapFrom(a => a.VisitStatusId.GetDescription()))
              .ReverseMap();
            CreateMap<LettersFormate, LettersFormateGridDto>().ReverseMap();

            CreateMap<LettersActivity, LettersActivityGridDto>()
              .ForMember(x => x.Project, x => x.MapFrom(a => a.Project.ProjectCode))
              .ForMember(x => x.Activity, x => x.MapFrom(a => a.Activity.ActivityName))
              .ForMember(x => x.LettersFormate, x => x.MapFrom(a => a.LettersFormate.LetterName))
              .ForMember(x => x.ScheduleStartDate, x => x.MapFrom(a => a.CtmsMonitoring.ScheduleStartDate))
              .ReverseMap();

            CreateMap<LabReport, LabReportGridDto>().ReverseMap();

            CreateMap<WorkingDay, WorkingDayListDto>()
            .ForMember(x => x.SiteCode, x => x.MapFrom(a => string.Join(", ", a.siteTypes.Where(x => x.DeletedDate == null).Select(s => s.Project.ProjectCode == null ? s.Project.ManageSite.SiteName : s.Project.ProjectCode + " - " + s.Project.ManageSite.SiteName).ToList())))
            .ReverseMap();
            CreateMap<SiteTypes, WorkingDayListDto>().ReverseMap();

            CreateMap<IDVerification, IDVerificationDto>().ReverseMap();

            CreateMap<Designation, DesignationGridDto>()
                 .ForMember(x => x.Department, x => x.MapFrom(a => a.Department.DepartmentName))
           .ReverseMap();

            CreateMap<StudyPlanResource, StudyPlanResourceGridDto>()
            .ForMember(x => x.ResourceType, x => x.MapFrom(a => a.ResourceType.ResourceTypes.GetDescription()))
            .ForMember(x => x.ResourceSubType, x => x.MapFrom(a => a.ResourceType.ResourceSubType.GetDescription()))
            .ReverseMap();

            CreateMap<Currency, CurrencyGridDto>()
               .ForMember(x => x.Country, x => x.MapFrom(a => a.Country.CountryName))
         .ReverseMap();

            CreateMap<UserAccess, UserAccessGridDto>()
           .ForMember(x => x.RoleUser, x => x.MapFrom(a => a.UserRole.User.FirstName + ' ' + a.UserRole.User.LastName))
           .ForMember(x => x.UserId, x => x.MapFrom(a => a.UserRole.User.Id))
           .ForMember(x => x.roleId, x => x.MapFrom(a => a.UserRole.SecurityRole.Id))
           .ForMember(x => x.Role, x => x.MapFrom(a => a.UserRole.SecurityRole.RoleName))
           .ReverseMap();

            CreateMap<PharmacyBarcodeConfig, PharmacyBarcodeConfigGridDto>()
                           .ForMember(x => x.BarcodeTypeName, x => x.MapFrom(a => a.BarcodeType.GetDescription()))
                           .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
                           .ForMember(x => x.SiteCode, x => x.MapFrom(a => a.Site.ProjectCode))
                           .ForMember(x => x.BarcodeModuleTypeName, x => x.MapFrom(a => a.BarcodeModuleType.GetDescription()))
                           .ForMember(x => x.BarcodeDisplayInfo, x => x.MapFrom(a => string.Join(",", a.BarcodeDisplayInfo.Where(x => x.DeletedDate == null).Select(s => s.TableFieldName.LabelName).ToList())))
                           .ReverseMap();
            CreateMap<Procedure, ProcedureGridDto>()
               .ForMember(x => x.Unit, x => x.MapFrom(a => a.Unit.UnitName))
               .ForMember(x => x.CurrencyType, x => x.MapFrom(a => a.Currency != null ? a.Currency.CurrencyName + "-" + a.Currency.CurrencySymbol + "   -" + a.Currency.Country.CountryName : ""))
               .ReverseMap();
            CreateMap<PassThroughCostActivity, PassThroughCostActivityGridDto>().ReverseMap();
            CreateMap<PassThroughCost, PassThroughCostGridDto>()
                .ForMember(x => x.PassThroughCostActivityName, x => x.MapFrom(a => a.PassThroughCostActivity.ActivityName))
                .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
                 .ForMember(x => x.BudgetFlgType, x => x.MapFrom(a => a.BudgetFlgType))
                .ForMember(x => x.BudgetFlgTypeName, x => x.MapFrom(a => a.BudgetFlgType.GetDescription()))
                .ForMember(x => x.UnitType, x => x.MapFrom(a => a.Unit.UnitName))
                .ForMember(x => x.CurrencyRate, x => x.MapFrom(a => a.CurrencyRate.LocalCurrencyRate))
                .ForMember(x => x.CurrencyName, x => x.MapFrom(a => a.CurrencyRate.Currency.CurrencyName + " -" + a.CurrencyRate.Currency.CurrencySymbol))
                .ForMember(x => x.GlobleCurrencyId, x => x.MapFrom(a => a.CurrencyRate.GlobalCurrencyId))
                .ForMember(x => x.LocalCurrencySymbol, x => x.MapFrom(a => a.CurrencyRate.Currency.CurrencySymbol))
                .ReverseMap();
            CreateMap<ResourceMilestone, ResourceMilestoneGridDto>()
              .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
               .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
               .ForMember(x => x.DateTypeResource, x => x.MapFrom(a => a.DateTypeResource.GetDescription()))
               .ForMember(x => x.StudyPlanTask, x => x.MapFrom(a => a.StudyPlanTask.TaskName))
               .ForMember(x => x.IsInvoiceGenerated, x => x.MapFrom(a => a.ResourceMilestoneInvoice.Any(x => x.DeletedDate == null)))
              .ReverseMap();
            CreateMap<PatientMilestone, PatientMilestoneGridDto>()
                    .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
                    .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
                    .ForMember(x => x.PaymentTypePatient, x => x.MapFrom(a => a.PaymentTypePatient.GetDescription()))
                    .ForMember(x => x.IsInvoiceGenerated, x => x.MapFrom(a => a.PatientMilestoneInvoice.Any(x => x.DeletedDate == null)))
                    .ReverseMap();
            CreateMap<PassthroughMilestone, PassthroughMilestoneGridDto>()
                   .ForMember(x => x.PassThroughCostActivity, x => x.MapFrom(a => a.PassThroughCostActivity.ActivityName))
                    .ForMember(x => x.PaymentTypePassThrough, x => x.MapFrom(a => a.PaymentTypePassThrough.GetDescription()))
                   .ForMember(x => x.PassThroughCostActivity, x => x.MapFrom(a => a.PassThroughCostActivity.ActivityName))
                   .ForMember(x => x.IsInvoiceGenerated, x => x.MapFrom(a => a.PassthroughMilestoneInvoice.Any(x => x.DeletedDate == null)))
                  .ReverseMap();
            CreateMap<BudgetPaymentFinalCost, BudgetPaymentFinalCostGridDto>()
                .ForMember(x => x.MilestoneTypeName, x => x.MapFrom(a => a.MilestoneType.GetDescription()))
                .ForMember(x => x.MilestoneType, x => x.MapFrom(a => a.MilestoneType))
            .ReverseMap();

            CreateMap<CtmsApprovalRoles, CtmsApprovalRolesGridDto>()
             .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.Project.ProjectCode))
             .ForMember(x => x.TriggerTypeName, x => x.MapFrom(a => a.TriggerType.GetDescription()))
             .ForMember(x => x.RoleName, x => x.MapFrom(a => a.SecurityRole.RoleName))
             .ForMember(x => x.CtmsApprovalRolesId, x => x.MapFrom(a => a.Id))
             .ForMember(x => x.SiteName, x => x.MapFrom(a => a.Site.ManageSite.SiteName))
             .ReverseMap();

            CreateMap<CtmsWorkflowApproval, CtmsWorkflowApprovalGridDto>()
            .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
            .ForMember(x => x.SenderName, x => x.MapFrom(a => a.Sender.UserName))
            .ForMember(x => x.ApproverName, x => x.MapFrom(a => a.User.UserName))
            .ForMember(x => x.ApproverRole, x => x.MapFrom(a => a.Role.RoleName))
            .ReverseMap();

            CreateMap<CtmsSiteContractWorkflowApproval, CtmsSiteContractWorkflowApprovalGridDto>()
         .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
         .ForMember(x => x.SenderName, x => x.MapFrom(a => a.Sender.UserName))
         .ForMember(x => x.ApproverName, x => x.MapFrom(a => a.User.UserName))
         .ForMember(x => x.ApproverRole, x => x.MapFrom(a => a.Role.RoleName))
         .ReverseMap();

            CreateMap<CtmsApprovalUsers, CtmsApprovalUsersGridDto>()
           .ForMember(x => x.UserName, x => x.MapFrom(a => a.Users.UserName))
           .ForMember(x => x.TriggerTypeName, x => x.MapFrom(a => a.CtmsApprovalRoles.TriggerType.GetDescription()))
           .ForMember(x => x.ProjectCode, x => x.MapFrom(a => a.CtmsApprovalRoles.Project.ProjectCode))
           .ForMember(x => x.RoleName, x => x.MapFrom(a => a.CtmsApprovalRoles.SecurityRole.RoleName))
           .ReverseMap();


            CreateMap<CtmsStudyPlanTaskComment, CtmsStudyPlanTaskCommentGridDto>()
            .ForMember(x => x.RoleId, x => x.MapFrom(a => a.CtmsWorkflowApproval.RoleId))
            .ForMember(x => x.ApproverRole, x => x.MapFrom(a => a.CtmsWorkflowApproval.Role.RoleName))
            .ForMember(x => x.SendDate, x => x.MapFrom(a => a.CtmsWorkflowApproval.SendDate))
            .ForMember(x => x.ActionDate, x => x.MapFrom(a => a.CtmsWorkflowApproval.ActionDate))
            .ForMember(x => x.IsApprove, x => x.MapFrom(a => a.CtmsWorkflowApproval.IsApprove))
            .ForMember(x => x.ApproverName, x => x.MapFrom(a => a.CtmsWorkflowApproval.User.UserName))
            .ForMember(x => x.SenderId, x => x.MapFrom(a => a.CtmsWorkflowApproval.SenderId))
            .ForMember(x => x.SenderName, x => x.MapFrom(a => a.CtmsWorkflowApproval.Sender.UserName))
            .ForMember(x => x.ChildId, x => x.MapFrom(a => a.CtmsWorkflowApproval.CtmsWorkflowApprovalId))
            .ReverseMap();


            CreateMap<CtmsWorkflowApprovalGridDto, CtmsStudyPlanTaskCommentGridDto>()
           .ForMember(x => x.ChildId, x => x.MapFrom(a => a.CtmsWorkflowApprovalId))
           .ForMember(x => x.Id, x => x.MapFrom(a => 0))
           .ForMember(x => x.HasChild, x => x.MapFrom(a => (a.CtmsWorkflowApprovalId != null)))
           .ForMember(x => x.CtmsWorkflowApprovalId, x => x.MapFrom(a => (a.Id)))
           .ReverseMap();

            CreateMap<PaymentTerms, PaymentTermsGridDto>().ReverseMap();

            CreateMap<PassthroughMilestoneInvoice, PassthroughMilestoneInvoiceGridDto>().ReverseMap();
            CreateMap<PatientMilestoneInvoice, PatientMilestoneInvoiceGridDto>().ReverseMap();
            CreateMap<ResourceMilestoneInvoice, ResourceMilestoneInvoiceGridDto>().ReverseMap();
            CreateMap<SitePayment, SitePaymentGridDto>()
                 .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
                 .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
                 .ForMember(x => x.BudgetPaymentType, x => x.MapFrom(a => a.BudgetPaymentType.GetDescription()))
                 .ForMember(x => x.Visit, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
                 .ForMember(x => x.Activity, x => x.MapFrom(a => a.PassThroughCostActivity.ActivityName))
                 .ForMember(x => x.BudgetFlgType, x => x.MapFrom(a => a.BudgetFlgType.GetDescription()))
                 .ForMember(x => x.UnitName, x => x.MapFrom(a => a.Unit.UnitName))

         .ReverseMap();

            CreateMap<SiteContract, SiteContractGridDto>()
               .ForMember(x => x.ProjectName, x => x.MapFrom(a => a.Project.ProjectCode))
               .ForMember(x => x.CountryName, x => x.MapFrom(a => a.Country.CountryName))
            .ReverseMap();

            CreateMap<PatientSiteContract, PatientSiteContractGridDto>()
               .ForMember(x => x.VisitName, x => x.MapFrom(a => a.ProjectDesignVisit.DisplayName))
               .ForMember(x => x.ContractCode, x => x.MapFrom(a => a.SiteContract.ContractCode))
            .ReverseMap();

            CreateMap<PassthroughSiteContract, PassthroughSiteContractGridDto>()
               .ForMember(x => x.ActivityName, x => x.MapFrom(a => a.PassThroughCostActivity.ActivityName))
               .ForMember(x => x.ContractCode, x => x.MapFrom(a => a.SiteContract.ContractCode))
            .ReverseMap();

            CreateMap<ContractTemplateFormat, ContractTemplateFormatGridDto>().ReverseMap();
        }
    }
}
