using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.Project.Rights
{
    public class StudyModuleDto
    {        
        public int Id { get; set; }
        [Required(ErrorMessage = "Project Code is required.")]
        public string StudyCode { get; set; }
        public List<StudyModuleslistDto> StudyModules { get; set; }
    }
    public class StudyModuleslistDto
    {
        public int ModuleID { get; set; }
        public string ModuleCode { get; set; }
    }
}
