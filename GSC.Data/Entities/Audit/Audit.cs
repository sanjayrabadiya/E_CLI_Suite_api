using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Audit
{
    public class Audit : BaseEntity
    {
        private DateTime _DateTime;

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new long Id { get; set; }

        public DateTime DateTime
        {
            get => _DateTime.UtcDate();
            set => _DateTime = value.UtcDate();
        }

        public string Username { get; set; }

        [Required] [MaxLength(128)] public string TableName { get; set; }

        [Required] [MaxLength(50)] public string Action { get; set; }

        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}