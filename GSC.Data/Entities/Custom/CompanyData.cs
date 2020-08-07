using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Entities.Custom
{
    public class CompanyData
    {
        [Key]
        public int Id { get; set; }
        public string IsComLogo { get; set; }
        public string IsClientLogo { get; set; }
        public string CompanyName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Address { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string Logo { get; set; }
        public string ClientLogo { get; set; }
        public string IsSignature { get; set; }
        public string Username { get; set; }

        public string IsSiteCode { get; set; }
        public string IsScreenNumber { get; set; }
        public string IsSponsorNumber { get; set; }
        public string IsSubjectNumber { get; set; }
    }
}
