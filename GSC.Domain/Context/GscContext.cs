using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Data.Entities.Client;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Configuration;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.InformConcent;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.LogReport;
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
using GSC.Data.Entities.UserMgt;
using GSC.Data.Entities.Volunteer;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GSC.Domain.Context
{
    public class GscContext : DbContext
    {
        private readonly List<string> _tablesToSkip = new List<string>
        {
            nameof(AuditTrailCommon),
            nameof(UserLoginReport),
            nameof(AuditTrail),
            nameof(Volunteer),
            nameof(VolunteerAddress),
            nameof(VolunteerBiometric),
            nameof(VolunteerContact),
            nameof(VolunteerDocument),
            nameof(VolunteerFood),
            nameof(VolunteerHistory),
            nameof(VolunteerImage),
            nameof(VolunteerLanguage),
            nameof(ScreeningTemplateValue),
            nameof(ScreeningTemplateValueAudit),
            nameof(ProjectArtificateDocumentHistory)
        };

        public GscContext(DbContextOptions options) : base(options)
        {
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
        public DbSet<UserAduit> UserAduit { get; set; }
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
        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<AuditTrailCommon> AuditTrailCommon { get; set; }
        public DbSet<AppSetting> AppSetting { get; set; }
        public DbSet<ProjectDesign> ProjectDesign { get; set; }
        public DbSet<ProjectDesignPeriod> ProjectDesignPeriod { get; set; }
        public DbSet<ProjectDesignVisit> ProjectDesignVisit { get; set; }
        public DbSet<ProjectDesignTemplate> ProjectDesignTemplate { get; set; }
        public DbSet<ProjectDesignVariable> ProjectDesignVariable { get; set; }
        public DbSet<ProjectDesignVariableValue> ProjectDesignVariableValue { get; set; }
        public DbSet<ProjectDesignVariableRemarks> ProjectDesignVariableRemarks { get; set; }
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
        public DbSet<ScreeningTemplateValue> ScreeningTemplateValue { get; set; }
        public DbSet<ScreeningTemplateValueSchedule> ScreeningTemplateValueSchedule { get; set; }
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
        public DbSet<VariableTemplateRight> VariableTemplateRight { get; set; }
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
        public DbSet<AuditValue> AuditValue { get; set; }
        public DbSet<EconsentSetup> EconsentSetup { get; set; }
        public DbSet<EconsentReviewDetails> EconsentReviewDetails { get; set; }
        public DbSet<RegulatoryType> RegulatoryType { get; set; }
        private List<string> ColumnsToSkip
        {
            get
            {
                var props = new List<string>();
                foreach (var prop in typeof(BaseEntity).GetProperties()) props.Add(prop.Name);

                props.Add("CompanyId");

                return props;
            }
        }

        public IList<EntityEntry> GetAuditTracker()
        {
            return ChangeTracker.Entries().ToList();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.DefalutMappingValue();
            modelBuilder.DefalutDeleteValueFilter();
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChanges() method.");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new Exception("Please provide IJwtTokenAccesser in SaveChangesAsync() method.");
        }

        public int SaveChanges(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);

            //var auditTrails = GetAuditTrailCommons(jwtTokenAccesser);

            var result = base.SaveChanges();

            //SaveAuditTrailCommons(auditTrails);

            return result;
        }

        public int SaveChanges(int fake)
        {
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(IJwtTokenAccesser jwtTokenAccesser)
        {
            SetModifiedInformation(jwtTokenAccesser);

            var auditTrails = GetAuditTrailCommons(jwtTokenAccesser);

            var result = await base.SaveChangesAsync();

            SaveAuditTrailCommons(auditTrails);

            return result;
        }

        public void DetectionAll()
        {
            var entries = ChangeTracker.Entries().Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Unchanged ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted)
                .ToList();
            entries.ForEach(r => r.State = EntityState.Detached);
        }

        private void SetModifiedInformation(IJwtTokenAccesser jwtTokenAccesser)
        {
            if (jwtTokenAccesser == null || jwtTokenAccesser.UserId <= 0) return;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = jwtTokenAccesser.UserId;
                    entry.Entity.CreatedDate = DateTime.Now.ToUniversalTime();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedDate).IsModified = false;

                    if (entry.Entity.InActiveRecord)
                    {
                        entry.Entity.DeletedBy = jwtTokenAccesser.UserId;
                        entry.Entity.DeletedDate = DateTime.Now.ToUniversalTime();
                    }
                    else
                    {
                        entry.Entity.ModifiedBy = jwtTokenAccesser.UserId;
                        entry.Entity.ModifiedDate = DateTime.Now.ToUniversalTime();
                    }
                }

            foreach (var entry in ChangeTracker.Entries<ScreeningTemplateValueAudit>())
            {
                entry.Entity.TimeZone = jwtTokenAccesser.GetHeader("timeZone");
                entry.Entity.IpAddress = jwtTokenAccesser.IpAddress;
            }
        }

        private IEnumerable<Tuple<EntityEntry, AuditTrailCommon>> GetAuditTrailCommons(
            IJwtTokenAccesser jwtTokenAccesser)
        {
            if (jwtTokenAccesser == null || jwtTokenAccesser.UserId <= 0) return null;

            ChangeTracker.DetectChanges();

            var changedEntityEntries = ChangeTracker.Entries().Where(e =>
                    e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            var auditTrails = new List<Tuple<EntityEntry, AuditTrailCommon>>();

            var userId = jwtTokenAccesser.UserId;
            var roleId = jwtTokenAccesser.RoleId;
            var createdDate = DateTime.Now.ToUniversalTime();

            foreach (var dbEntry in changedEntityEntries)
            {
                var tableName = dbEntry.CurrentValues.EntityType.ClrType.Name.ToString();//dbEntry.Metadata.Name;
                if (_tablesToSkip.Contains(tableName)) continue;

                var action = Enum.GetName(typeof(EntityState), dbEntry.State);

                int.TryParse(jwtTokenAccesser.GetHeader("audit-reason-id"), out var reasonId);
                var reasonOth = jwtTokenAccesser.GetHeader("audit-reason-oth");

                if ((dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Deleted ||
                    (dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Activated)
                {
                    action = Enum.GetName(typeof(AuditAction), (dbEntry.Entity as BaseEntity).AuditAction);

                    auditTrails.Add(new Tuple<EntityEntry, AuditTrailCommon>(dbEntry, new AuditTrailCommon
                    {
                        TableName = tableName,
                        Action = action,
                        UserId = userId,
                        UserRoleId = roleId,
                        CreatedDate = createdDate,
                        ReasonId = reasonId > 0 ? reasonId : (int?)null,
                        ReasonOth = reasonOth,
                        IpAddress = jwtTokenAccesser.IpAddress,
                        TimeZone = jwtTokenAccesser.GetHeader("timeZone")
                    }));
                }
                else
                {
                    var dbValueProps = dbEntry.GetDatabaseValues();

                    foreach (var prop in dbEntry.Properties)
                    {
                        var columnName = prop.Metadata.Name;
                        if (ColumnsToSkip.Contains(columnName)) continue;

                        var newValue = Convert.ToString(prop.CurrentValue);

                        if (dbEntry.State == EntityState.Added && newValue.Length == 0) continue;

                        var oldValue = "";
                        if (dbEntry.State == EntityState.Modified && dbValueProps != null)
                        {
                            oldValue = Convert.ToString(dbValueProps.GetValue<object>(columnName));
                            if (oldValue == newValue) continue;
                        }

                        auditTrails.Add(new Tuple<EntityEntry, AuditTrailCommon>(dbEntry, new AuditTrailCommon
                        {
                            TableName = tableName,
                            Action = action,
                            ColumnName = columnName,
                            OldValue = oldValue,
                            NewValue = newValue,
                            UserId = userId,
                            UserRoleId = roleId,
                            CreatedDate = createdDate,
                            IpAddress = jwtTokenAccesser.IpAddress,
                            ReasonId = reasonId > 0 ? reasonId : (int?)null,
                            ReasonOth = reasonOth,
                            TimeZone = jwtTokenAccesser.GetHeader("timeZone")
                        }));
                    }
                }
            }

            return auditTrails;
        }

        private void SaveAuditTrailCommons(IEnumerable<Tuple<EntityEntry, AuditTrailCommon>> auditTrails)
        {
            if (auditTrails == null || !auditTrails.Any()) return;

            AuditTrailCommon.AddRange(
                auditTrails.ForEach(t =>
                        t.Item2.RecordId =
                            Convert.ToInt32(t.Item1.Properties.First(p => p.Metadata.IsPrimaryKey()).CurrentValue))
                    .Select(t => t.Item2)
            );
            base.SaveChanges();
        }

        public void Begin()
        {
            base.Database.BeginTransaction();
        }

        public void Commit()
        {
            base.Database.CommitTransaction();
        }

        public void Rollback()
        {
            base.Database.RollbackTransaction();
        }
    }

    public static class Extensions
    {
        public static IDictionary<TKey, TValue> NullIfEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || !dictionary.Any()) return null;
            return dictionary;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source) action(element);
            return source;
        }
    }
}