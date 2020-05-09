using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Entities.Custom
{
    public class CustomTable
    {
        [Key]
        public int Id { get; set; }

        public string ValueName { get; set; }
        public string ValueCode { get; set; }
    }
}