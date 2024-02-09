using System.Collections.Generic;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class DropDownDto : BaseDto
    {
        public string Value { get; set; }
        public string Code { get; set; }
        public object ExtraData { get; set; }
        public bool InActive { get; set; }
    }


    public class DesignDropDownDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool IsAnyLive { get; set; }
        public bool IsTrial { get; set; }
    }


    public class DropDownVaribleDto : BaseDto
    {
        public string Value { get; set; }
        public string Code { get; set; }
        public DataType? DataType { get; set; }
        public string VisitName { get; set; }
        public CollectionSources CollectionSources { get; set; }
        public bool InActive { get; set; }
        public List<ProjectDesignVariableValueDropDown> ExtraData { get; set; }        
    }

    public class DropDownVaribleAnnotationDto : BaseDto
    {
        public string Value { get; set; }
        public string Code { get; set; }
        public DataType? DataType { get; set; }
        public CollectionSources CollectionSources { get; set; }
        public List<ProjectDesignVariableValue> ExtraData { get; set; }
        public List<DropDownVaribleAnnotationDto> ListOfVariable { get; set; }
    }

    public class DropDownWithSeqDto : BaseDto
    {
        public string Value { get; set; }
        public string Code { get; set; }
        public int SeqNo { get; set; }
    }

    public class DropDownEnum : BaseDto
    {
        public short Id { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
    }

    public class ProjectDropDown : BaseDto
    {
        public string Value { get; set; }
        public string Code { get; set; }
        public bool IsTestSite { get; set; }
        public bool IsStatic { get; set; }
        public int CountryId { get; set; }
        public int ParentProjectId { get; set; }
        public int AttendanceLimit { get; set; }
        public bool IsSendSMS { get; set; }
        public bool IsSendEmail { get; set; }
    }

    public class DropDownStudyDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
       
    }
}