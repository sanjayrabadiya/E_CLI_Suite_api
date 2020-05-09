using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraCodingRepository : GenericRespository<MeddraCoding, GscContext>, IMeddraCodingRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;

        public MeddraCodingRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IUserRepository userRepository, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)

        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository= userRepository;
        }

        public List<MeddraCodingVariableDto> SearchMain(MeddraCodingSearchDto meddraCodingSearchDto)
        {
            var ParentProjectId = Context.ProjectDesign.Find(meddraCodingSearchDto.ProjectDesignId).ProjectId;

            //var Exists = All.Where(x => x.ProjectId == ParentProjectId).ToList();
            List<MeddraCodingVariableDto> objList = new List<MeddraCodingVariableDto>();
            //if (Exists.Count > 0)
            //{
            //    return null;
            //}
            //else
            //{
            var variable = Context.StudyScoping.Where(t => t.ProjectId == ParentProjectId)
            .Include(d => d.ProjectDesignVariable)
            .ThenInclude(d => d.ProjectDesignTemplate)
            .ThenInclude(d => d.ProjectDesignVisit)
            .ThenInclude(d => d.ProjectDesignPeriod)
            .AsNoTracking().ToList();

            foreach (var item in variable)
            {
                MeddraCodingVariableDto obj = new MeddraCodingVariableDto();
                obj.MeddraConfigId = item.MedraConfigId;
                obj.ProjectDesignTemplateId = item.ProjectDesignVariable.ProjectDesignTemplateId;
                obj.VariableName = item.ProjectDesignVariable.VariableName;
                obj.ProjectDesignVariableId = item.ProjectDesignVariableId;
                obj.VariableCode = item.ProjectDesignVariable.VariableCode;
                obj.VariableAlias = item.ProjectDesignVariable.VariableAlias;
                obj.TemplateName = item.ProjectDesignVariable.ProjectDesignTemplate.TemplateName;
                obj.VisitName = item.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName;
                obj.PeriodName = item.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName;
                objList.Add(obj);
            }
            // }
            return objList;
        }

        public MeddraCodingMainDto GetVariableCount(MeddraCodingSearchDto meddraCodingDto)
        {
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == meddraCodingDto.ProjectDesignVariableId).ToList();
            if (meddraCodingDto.ProjectId != 0 && Exists.Count>0) {
                Exists = Exists.FindAll(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningEntry.ProjectId == meddraCodingDto.ProjectId);
            }
            if (meddraCodingDto.CountryId != 0 && Exists.Count > 0)
            {
                Exists = Exists.FindAll(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningEntry.Project.CountryId == meddraCodingDto.CountryId);
            }

            MeddraCodingMainDto objList = new MeddraCodingMainDto();

            var variable = (from st in Context.ScreeningTemplate
                            join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                            join pdv in Context.ProjectDesignVariable on pt.Id equals pdv.ProjectDesignTemplateId
                            join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id
                            join project in Context.Project on se.ProjectId equals project.Id
                            join counry in Context.Country on project.CountryId equals counry.Id
                            where pdv.DeletedDate == null && pdv.Id == meddraCodingDto.ProjectDesignVariableId && st.Status != ScreeningStatus.Pending
                            && (meddraCodingDto.ProjectId != 0 ? se.ProjectId == meddraCodingDto.ProjectId : true)
                            && (meddraCodingDto.CountryId != 0 ? project.CountryId == meddraCodingDto.CountryId : true)
                            group new { pdv } by new { pdv.Id } into g
                            select new MeddraCodingMainDto
                            {
                                All = g.Key.Id
                            }).FirstOrDefault();

            objList.All = variable == null ? 0 : variable.All;
            

            if (Exists.Count > 0)
            {
                objList.CodedData = Exists.Count;
                objList.ApprovalData = Exists.FindAll(t=>t.IsApproved==true).Count();
                objList.ModifiedDate = Exists[Exists.Count-1].ModifiedDate ;
                int updated = (int)Exists[Exists.Count-1].ModifiedBy;
                objList.ModifiedBy = _userRepository.Find(updated).UserName;
            }
            else
            {
                objList.CodedData = 0;
                objList.ApprovalData = 0;
                objList.ModifiedDate = null;
                objList.ModifiedBy = "";
            }
            return objList;
        }

        public List<MeddraCodingVariableDto> SearchCodingDetails(MeddraCodingSearchDto filters)
        {
            //var Exists = All.Where(x => x.ProjectId == filters.ProjectId).ToList();
            List<MeddraCodingVariableDto> objList = new List<MeddraCodingVariableDto>();
            //if (Exists.Count > 0)
            //{
            //    return null;
            //}
            //else
            //{
            var variable = Context.StudyScoping.Where(t => t.ProjectId == filters.ProjectId)
            .Include(d => d.ProjectDesignVariable)
            .ThenInclude(d => d.ProjectDesignTemplate)
            .ThenInclude(d => d.ProjectDesignVisit)
            .ThenInclude(d => d.ProjectDesignPeriod)
            .AsNoTracking().ToList();

            foreach (var item in variable)
            {
                MeddraCodingVariableDto obj = new MeddraCodingVariableDto();
                obj.ProjectDesignTemplateId = item.ProjectDesignVariable.ProjectDesignTemplateId;
                obj.VariableName = item.ProjectDesignVariable.VariableName;
                obj.ProjectDesignVariableId = item.ProjectDesignVariableId;
                obj.VariableCode = item.ProjectDesignVariable.VariableCode;
                obj.VariableAlias = item.ProjectDesignVariable.VariableAlias;
                obj.TemplateName = item.ProjectDesignVariable.ProjectDesignTemplate.TemplateName;
                obj.VisitName = item.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName;
                obj.PeriodName = item.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName;
                objList.Add(obj);
            }
            //}
            return objList;
        }

        public List<DropDownDto> MeddraCodingVariableDropDown(int ProjectId)
        {
            var ParentProjectId = Context.ProjectDesign.Find(ProjectId).ProjectId;
            List<DropDownDto> objList = new List<DropDownDto>();
            var variable = Context.StudyScoping.Where(t => t.ProjectId == ParentProjectId)
            .Include(d => d.MedraConfig)
            .ThenInclude(d => d.Language)
            .Include(d => d.MedraConfig)
            .ThenInclude(d => d.MedraVersion)
            .ThenInclude(d => d.Dictionary)
            .Include(d => d.ProjectDesignVariable)
            .ThenInclude(d => d.ProjectDesignTemplate)
            .ThenInclude(d => d.ProjectDesignVisit)
            .ThenInclude(d => d.ProjectDesignPeriod)
            .AsNoTracking().ToList();

            foreach (var item in variable)
            {
                DropDownDto obj = new DropDownDto();
                obj.Id = item.ProjectDesignVariableId;
                obj.Value = item.ProjectDesignVariable.Annotation + "-" + item.MedraConfig.MedraVersion.Dictionary.DictionaryName + "-" + item.MedraConfig.MedraVersion.Version + "-" + item.MedraConfig.Language.LanguageName;
                obj.ExtraData = item.MedraConfigId;
                objList.Add(obj);
            }
            return objList;
        }

        public IList<MeddraCodingSearchDetails> GetMedDRACodingDetails(MeddraCodingSearchDto filters)
        {
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == filters.ProjectDesignVariableId).ToList();
            MeddraCodingSearchDetails objList = new MeddraCodingSearchDetails();


            //if (Exists.Count == 0)
            //{
            return (from se in Context.ScreeningEntry
                    join project in Context.Project on se.ProjectId equals project.Id
                    join st in Context.ScreeningTemplate on se.Id equals st.ScreeningEntryId
                    join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                    join visit in Context.ProjectDesignVisit on pt.ProjectDesignVisitId equals visit.Id
                    join pdv in Context.ProjectDesignVariable on pt.Id equals pdv.ProjectDesignTemplateId
                    join value in Context.ScreeningTemplateValue.Where(val => val.DeletedDate == null
                                                && val.ProjectDesignVariable
                                                    .DeletedDate == null) on new
                                                    { st.Id, st.ProjectDesignTemplateId } equals new
                                                    { Id = value.ScreeningTemplateId, value.ProjectDesignVariable.ProjectDesignTemplateId }
                    join attendance in Context.Attendance.Where(t => t.DeletedDate == null && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.Id)))
                    on se.AttendanceId equals attendance.Id
                    join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                    from volunteer in volunteerDto.DefaultIfEmpty()
                    join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                    from nonregister in noneregisterDto.DefaultIfEmpty()
                    join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                    from projectsubject in projectsubjectDto.DefaultIfEmpty()
                    join medraCoding in Context.MeddraCoding.Where(t => t.DeletedDate == null) on value.Id equals medraCoding.ScreeningTemplateValueId into medraDto
                    from meddraCoding in medraDto.DefaultIfEmpty()
                    join mllt in Context.MeddraLowLevelTerm on meddraCoding.MeddraLowLevelTermId equals mllt.Id into mlltDto
                    from meddraLLT in mlltDto.DefaultIfEmpty()
                    join users in Context.Users on meddraCoding.ModifiedBy equals users.Id into userDto
                    from user in userDto.DefaultIfEmpty()
                    where pdv.DeletedDate == null && pdv.Id == filters.ProjectDesignVariableId
                    && (filters.ProjectId != 0 ? se.ProjectId == filters.ProjectId : true)
                            && (filters.CountryId != 0 ? project.CountryId == filters.CountryId : true)
                    select new MeddraCodingSearchDetails
                    {
                        SubjectId = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                        VisitName = visit.DisplayName,
                        TemplateName = pt.TemplateName,
                        Value = value.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox ? string.Join(";",
                          from stvc in Context.ScreeningTemplateValueChild.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == value.Id && x.Value == "true")
                          join prpjectdesignvalueTemp in Context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null) on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into
                          prpjectdesignvalueDto
                          from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                          select prpjectdesignvalue.ValueName)
                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                          !string.IsNullOrEmpty(value.Value)
                          ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                          b.ProjectDesignVariableId == value.ProjectDesignVariable.Id).ValueName
                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                          value.IsNa && string.IsNullOrEmpty(value.Value) ? "NA"
                          : value.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                          value.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton
                          ? Context.ProjectDesignVariableValue.FirstOrDefault(b =>
                          b.ProjectDesignVariableId == value.ProjectDesignVariable.Id &&
                          b.Id == Convert.ToInt32(value.Value)).ValueName
                          : value.Value,
                        CodedType = meddraCoding.CodedType,
                        Code = meddraLLT.llt_name,
                        LastUpdateOn = meddraCoding.ModifiedDate,
                        UpdatedBy = user.UserName,
                        ScreeningTemplateValueId=value.Id
                    }).ToList();

            //}
            //else
            //{
            //    return new List<MeddraCodingSearchDetails>();
            //}
        }

        public IList<MeddraCodingSearchDetails> AutoCodes(MeddraCodingSearchDto meddraCodingSearchDto)
        {
            //var Exists = All.Where(x => x.ProjectDesignVariableId == meddraCodingSearchDto.ProjectDesignVariableId).ToList();

            MeddraCodingSearchDetails objList = new MeddraCodingSearchDetails();
            var dataEntries = new List<MeddraCodingSearchDetails>();

            var r1 = (from stv in Context.ScreeningTemplateValue
                      join pdvv in Context.ProjectDesignVariableValue on stv.Value equals pdvv.ValueName
                      join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id

                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()

                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on pdvv.ValueName equals mllt.llt_name
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join mpt in Context.MeddraPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals mpt.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_soc_code equals soc.soc_code
                      join hpt in Context.MeddraHltPrefComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_code equals hpt.pt_code
                      join hlt in Context.MeddraHltPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hpt.hlt_code equals hlt.hlt_code
                      join hlgtHLT in Context.MeddraHlgtHltComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlt.hlt_code equals hlgtHLT.hlt_code
                      join hlgt in Context.MeddraHlgtPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlgtHLT.hlgt_code equals hlgt.hlgt_code
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals md.pt_code
                      where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = pdvv.ValueName,
                          SocCode = soc.soc_code,
                          PT = mpt.pt_name,
                          HLT = hlt.hlt_name,
                          HLGT = hlgt.hlgt_name,
                          SOCValue = soc.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id
                      }).ToList();
            dataEntries.AddRange(r1);

            var r2 = (from stv in Context.ScreeningTemplateValue
                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on stv.Value equals mllt.llt_name
                      join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()

                      join mpt in Context.MeddraPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals mpt.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_soc_code equals soc.soc_code
                      join hpt in Context.MeddraHltPrefComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_code equals hpt.pt_code
                      join hlt in Context.MeddraHltPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hpt.hlt_code equals hlt.hlt_code
                      join hlgtHLT in Context.MeddraHlgtHltComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlt.hlt_code equals hlgtHLT.hlt_code
                      join hlgt in Context.MeddraHlgtPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlgtHLT.hlgt_code equals hlgt.hlgt_code
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals md.pt_code
                      where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = stv.Value,
                          SocCode = soc.soc_code,
                          PT = mpt.pt_name,
                          HLT = hlt.hlt_name,
                          HLGT = hlgt.hlgt_name,
                          SOCValue = soc.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id
                      }).ToList();
            dataEntries.AddRange(r2);

            var r3 = (from stv in Context.ScreeningTemplateValue
                      join stvc in Context.ScreeningTemplateValueChild on stv.Id equals stvc.ScreeningTemplateValueId
                      join pdvv in Context.ProjectDesignVariableValue on stvc.ProjectDesignVariableValueId equals pdvv.Id
                      join st in Context.ScreeningTemplate on stv.ScreeningTemplateId equals st.Id
                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()
                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId)
                      on pdvv.ValueName equals mllt.llt_name
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join mpt in Context.MeddraPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals mpt.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_soc_code equals soc.soc_code
                      join hpt in Context.MeddraHltPrefComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mpt.pt_code equals hpt.pt_code
                      join hlt in Context.MeddraHltPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hpt.hlt_code equals hlt.hlt_code
                      join hlgtHLT in Context.MeddraHlgtHltComp.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlt.hlt_code equals hlgtHLT.hlt_code
                      join hlgt in Context.MeddraHlgtPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on hlgtHLT.hlgt_code equals hlgt.hlgt_code
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on mllt.pt_code equals md.pt_code
                      where stvc.Value == "True" && pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.ScreeningNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = pdvv.ValueName,
                          SocCode = soc.soc_code,
                          PT = mpt.pt_name,
                          HLT = hlt.hlt_name,
                          HLGT = hlgt.hlgt_name,
                          SOCValue = soc.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id
                      }).ToList();
            dataEntries.AddRange(r3);
            int i = 0;
            foreach (var item in dataEntries)
            {
                item.TempId = i + 1;
                i++;
            }

            return dataEntries.ToList();
        }
    }
}
