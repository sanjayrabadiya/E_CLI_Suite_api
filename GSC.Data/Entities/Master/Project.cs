using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Data.Entities.Location;
using GSC.Data.Entities.Project.Design;
using GSC.Shared;

namespace GSC.Data.Entities.Master
{
    public class Project : BaseEntity
    {
        private DateTime? _FromDate;

        private DateTime? _ToDate;
        public string ProjectCode { get; set; }

        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }

        public int? ParentProjectId { get; set; }

        public int DesignTrialId { get; set; }

        public int CountryId { get; set; }

        public int ClientId { get; set; }

        public int DrugId { get; set; }

        public int? CityAreaId { get; set; }
        public int? CityId { get; set; }
        public int? StateId { get; set; }
        public string SiteName { get; set; }
        public string PinCode { get; set; }

        public int Period { get; set; }
        public bool IsStatic { get; set; }
        public int? RegulatoryTypeId { get; set; }
        public RegulatoryType? RegulatoryType { get; set; }

        public DateTime? FromDate
        {
            get => _FromDate?.UtcDate();
            set => _FromDate = value?.UtcDate();
        }

        public DateTime? ToDate
        {
            get => _ToDate?.UtcDate();
            set => _ToDate = value?.UtcDate();
        }

        public int? CompanyId { get; set; }
        public int? InvestigatorContactId { get; set; }
        public IList<ProjectRight.ProjectRight> ProjectRight { get; set; }

        public ICollection<ProjectDesign> ProjectDesigns { get; set; }

        public int? AttendanceLimit { get; set; }

        public ICollection<Attendance.Attendance> Attendances { get; set; }
        public City City { get; set; }
        public Country Country { get; set; }
        public State State { get; set; }
        public CityArea CityArea { get; set; }
        public GSC.Data.Entities.Client.Client Client { get; set; }
        public Drug Drug { get; set; }
        public DesignTrial DesignTrial { get; set; }
        public InvestigatorContact InvestigatorContact { get; set; }
        [ForeignKey("ParentProjectId")]
        public List<Project> ChildProject { get; set; }
    }
}