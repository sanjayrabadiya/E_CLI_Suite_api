using System;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Helper.DocumentService;

namespace GSC.Data.Dto.Client
{
    public class ClientDto : BaseDto
    {
        public string ClientCode { get; set; }

        [Required(ErrorMessage = "Client Name is required.")]
        public string ClientName { get; set; }
        public int ClientTypeId { get; set; }
        public string ClientTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string Logo { get; set; }
        public string LogoPath { get; set; }
        public ClientType ClientType { get; set; }
        public FileModel FileModel { get; set; }

        public string CreatedByUser { get; set; }
        public string DeletedByUser { get; set; }
        public string ModifiedByUser { get; set; }
        //public int CreatedBy { get; set; }
        //public int? DeletedBy { get; set; }
        //public int? ModifiedBy { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
        //public DateTime? DeletedDate { get; set; }
        public int? CompanyId { get; set; }
        //public string CompanyName { get; set; }
    }

    public class ClientGridDto : BaseAuditDto
    {
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public int ClientTypeId { get; set; }
        public string ClientTypeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public ClientType ClientType { get; set; }
        public string Logo { get; set; }
        public string LogoPath { get; set; }
    }
}