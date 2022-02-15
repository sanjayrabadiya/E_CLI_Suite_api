using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Project.Design
{
    public class StudyLevelFormVariableRemarksDto : BaseDto
    {
        public int StudyLevelFormVariableId { get; set; }

        [Required(ErrorMessage = "Range is required.")]
        public int Range { get; set; }

        public string Remarks { get; set; }
        public int SeqNo { get; set; }
        //public string ScreeningValue { get; set; }
        //public int ScreeningTemplateValueChildId { get; set; }
        //public string ScreeningValueOld { get; set; }
    }

    public class StudyLevelFormVariableRemarksDropDown
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
    }
}
