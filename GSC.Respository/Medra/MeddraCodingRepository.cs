using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Medra;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
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
        private readonly IRoleRepository _roleRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private IMeddraCodingAuditRepository _meddraCodingAuditRepository;
        private IMeddraCodingCommentRepository _meddraCodingCommentRepository;


        public MeddraCodingRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IUserRepository userRepository, IRoleRepository roleRepository, IProjectRightRepository projectRightRepository, IPropertyMappingService propertyMappingService,
            IMeddraCodingAuditRepository meddraCodingAuditRepository, IMeddraCodingCommentRepository meddraCodingCommentRepository) : base(uow, jwtTokenAccesser)

        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _projectRightRepository = projectRightRepository;
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
            _meddraCodingCommentRepository = meddraCodingCommentRepository;
        }

        public List<MeddraCodingVariableDto> SearchMain(MeddraCodingSearchDto meddraCodingSearchDto)
        {
            var ParentProjectId = Context.ProjectDesign.Find(meddraCodingSearchDto.ProjectDesignId).ProjectId;

            List<MeddraCodingVariableDto> objList = new List<MeddraCodingVariableDto>();
            var variable = Context.StudyScoping.Where(t => t.DeletedDate == null && t.ProjectId == ParentProjectId)
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
            return objList;
        }

        public MeddraCodingMainDto GetVariableCount(MeddraCodingSearchDto meddraCodingDto)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();

            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == meddraCodingDto.ProjectDesignVariableId && projectList.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningEntry.ProjectId) && x.DeletedDate == null && x.MeddraSocTermId != null);

            if (meddraCodingDto.ProjectId != 0)
            {
                Exists = Exists.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningEntry.ProjectId == meddraCodingDto.ProjectId);
            }
            if (meddraCodingDto.CountryId != 0)
            {
                Exists = Exists.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningEntry.Project.CountryId == meddraCodingDto.CountryId);
            }

            var result = Exists.ToList();

            MeddraCodingMainDto objList = new MeddraCodingMainDto();

            var variable = (from st in Context.ScreeningTemplate
                            join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                            join pdv in Context.ProjectDesignVariable on pt.Id equals pdv.ProjectDesignTemplateId
                            join se in Context.ScreeningEntry on st.ScreeningEntryId equals se.Id
                            join attendance in Context.Attendance on se.AttendanceId equals attendance.Id
                            join project in Context.Project.Where(x => projectList.Contains(x.Id)) on se.ProjectId equals project.Id
                            join counry in Context.Country on project.CountryId equals counry.Id
                            where pdv.DeletedDate == null && pdv.Id == meddraCodingDto.ProjectDesignVariableId && st.Status != ScreeningStatus.Pending && st.Status != ScreeningStatus.InProcess
                            && (meddraCodingDto.ProjectId != 0 ? se.ProjectId == meddraCodingDto.ProjectId : true)
                            && (meddraCodingDto.CountryId != 0 ? project.CountryId == meddraCodingDto.CountryId : true)
                            && attendance.NoneRegister.RandomizationNumber != null
                            group new { pdv } by new { pdv.Id } into g
                            select new MeddraCodingMainDto
                            {
                                All = g.Count()
                            }).FirstOrDefault();

            objList.All = variable == null ? 0 : variable.All;


            if (result.Count > 0)
            {
                objList.CodedData = result.Count;
                objList.ApprovalData = result.FindAll(t => t.IsApproved == true).Count();
                objList.ModifiedDate = result[result.Count - 1].ModifiedDate;
                int updated = (int)result[result.Count - 1].ModifiedBy;
                objList.ModifiedBy = _userRepository.Find(updated).UserName;
                objList.ModifiedByRole = _roleRepository.Find((int)result[result.Count - 1].CreatedRole).RoleName;
            }
            else
            {
                objList.CodedData = 0;
                objList.ApprovalData = 0;
                objList.ModifiedDate = null;
                objList.ModifiedBy = "";
                objList.ModifiedByRole = "";
            }

            var Coder = Context.StudyScoping.Where(t => t.ProjectDesignVariableId == meddraCodingDto.ProjectDesignVariableId && t.DeletedDate == null).FirstOrDefault();
            if (Coder != null)
            {
                if (Coder.CoderProfile == _jwtTokenAccesser.RoleId)
                    objList.IsCoding = true;
                else
                    objList.IsCoding = false;

                if (Coder.CoderApprover != null)
                    objList.IsShow = true;
                else
                    objList.IsShow = false;

                if (Coder.CoderApprover == _jwtTokenAccesser.RoleId)
                    objList.IsApproveProfile = true;
                else
                    objList.IsApproveProfile = false;
            }
            else
            {
                objList.IsCoding = false;
                objList.IsShow = false;
                objList.IsApproveProfile = false;
            }
            return objList;
        }

        public List<MeddraCodingVariableDto> SearchCodingDetails(MeddraCodingSearchDto filters)
        {
            List<MeddraCodingVariableDto> objList = new List<MeddraCodingVariableDto>();
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
            return objList;
        }

        public List<DropDownDto> MeddraCodingVariableDropDown(int ProjectId)
        {
            var ParentProjectId = Context.ProjectDesign.Find(ProjectId).ProjectId;
            List<DropDownDto> objList = new List<DropDownDto>();
            var variable = Context.StudyScoping.Where(t => t.DeletedDate == null && t.ProjectId == ParentProjectId)
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
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == filters.ProjectDesignVariableId && x.DeletedDate == null).ToList();
            var projectList = _projectRightRepository.GetProjectRightIdList();

            var result = (from se in Context.ScreeningEntry
                          join project in Context.Project.Where(x => projectList.Contains(x.Id)) on se.ProjectId equals project.Id
                          join st in Context.ScreeningTemplate.Where(t => t.DeletedDate == null && ((filters.TemplateStatus != null && filters.ExtraData == false) ? t.Status == ScreeningStatus.Completed
                          : true) && ((filters.TemplateStatus != null && filters.ExtraData == true) ? t.ReviewLevel == filters.TemplateStatus : true) && t.Status != ScreeningStatus.Pending && t.Status != ScreeningStatus.InProcess) on se.Id equals st.ScreeningEntryId
                          join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                          join visit in Context.ProjectDesignVisit on pt.ProjectDesignVisitId equals visit.Id
                          join pdv in Context.ProjectDesignVariable.Where(val => val.DeletedDate == null && val.Id == filters.ProjectDesignVariableId)
                                           on new { Id1 = pt.Id } equals new { Id1 = pdv.ProjectDesignTemplateId }
                          join value in Context.ScreeningTemplateValue.Where(val => val.DeletedDate == null) on new
                          { Id = st.Id, Id1 = pdv.Id } equals new
                          { Id = value.ScreeningTemplateId, Id1 = value.ProjectDesignVariableId }
                          join attendance in Context.Attendance.Where(t => t.DeletedDate == null && (filters.SubjectIds == null || filters.SubjectIds.Contains(t.Id)))
                          on se.AttendanceId equals attendance.Id
                          join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                          from volunteer in volunteerDto.DefaultIfEmpty()
                          join noneregisterTemp in Context.NoneRegister.Where(t => t.DeletedDate == null && t.RandomizationNumber != null) on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                          from nonregister in noneregisterDto.DefaultIfEmpty()
                          join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                          from projectsubject in projectsubjectDto.DefaultIfEmpty()
                          join medraCoding in Context.MeddraCoding.Where(t => t.DeletedDate == null) on value.Id equals medraCoding.ScreeningTemplateValueId into medraDto
                          from meddraCoding in medraDto.DefaultIfEmpty()
                          join soc in Context.MeddraSocTerm on meddraCoding.MeddraSocTermId equals soc.Id into socDto
                          from meddraSoc in socDto.DefaultIfEmpty()
                          join mllt in Context.MeddraLowLevelTerm on meddraCoding.MeddraLowLevelTermId equals mllt.Id into mlltDto
                          from meddraLLT in mlltDto.DefaultIfEmpty()
                          join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == filters.MeddraConfigId)
                          on meddraSoc.soc_code equals md.soc_code into mdDto
                          from meddraMD in mdDto.DefaultIfEmpty()
                          join users in Context.Users on meddraCoding.ModifiedBy equals users.Id into userDto
                          from user in userDto.DefaultIfEmpty()
                          join roles in Context.SecurityRole on meddraCoding.CreatedRole equals roles.Id into roleDto
                          from role in roleDto.DefaultIfEmpty()
                          where meddraLLT.pt_code == meddraMD.pt_code && pdv.Id == filters.ProjectDesignVariableId &&
                          ((filters.ProjectId != 0 ? se.ProjectId == filters.ProjectId : true))
                                  && ((filters.CountryId != 0 ? project.CountryId == filters.CountryId : true))
                                   && (filters.Status != null ? (filters.Status != CodedType.UnCoded ? meddraCoding.CodedType == filters.Status :
                                   !(from o in Exists
                                     where o.MeddraLowLevelTermId != null && o.MeddraSocTermId != null
                                     select o.ScreeningTemplateValueId).Contains(value.Id)) : true)
                                   && (filters.IsApproved != null ? (meddraCoding.IsApproved == true ? meddraCoding.IsApproved == filters.IsApproved :
                                   (from o in Exists where o.IsApproved == filters.IsApproved select o.ScreeningTemplateValueId).Contains(value.Id)) : true)
                                   && ((filters.FromDate.HasValue ? meddraCoding.CreatedDate >= filters.FromDate : true))
                             && ((filters.ToDate.HasValue ? meddraCoding.CreatedDate <= filters.ToDate : true))
                             && nonregister.RandomizationNumber != null
                          select new MeddraCodingSearchDetails
                          {
                              MeddraCodingId = meddraCoding.Id,
                              SubjectId = volunteer.FullName == null ? nonregister.RandomizationNumber : volunteer.VolunteerNo,
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
                              CommentStatus = meddraCoding.Id == null ? 0 :
                              Context.MeddraCodingComment.Where(x => x.MeddraCodingId == meddraCoding.Id).OrderByDescending(o => o.Id).FirstOrDefault().CommentStatus,
                              Code = meddraLLT.llt_name,
                              LastUpdateOn = meddraCoding.ModifiedDate,
                              UpdatedBy = user.UserName + " (" + role.RoleName + ")",
                              ScreeningTemplateValueId = value.Id,
                              SiteCode = project.ProjectCode,
                              PT = meddraMD.pt_name,
                              HLT = meddraMD.hlt_name,
                              HLGT = meddraMD.hlgt_name,
                              SOCValue = meddraMD.soc_name,
                              SocCode = meddraMD.pt_code.ToString(),
                              IsApproved = meddraCoding.IsApproved
                          }).ToList();

            if (filters.CommentStatus != null)
            {
                result = result.Where(x => (int?)x.CommentStatus == filters.CommentStatus).ToList();
            }

            if (filters.Value.Trim().ToLower() != "")
            {
                result = result.Where(x => x.Value.Trim().ToLower().Contains(filters.Value.Trim().ToLower())).ToList();
            }
            return result.ToList();
        }

        public IList<MeddraCodingSearchDetails> AutoCodes(MeddraCodingSearchDto meddraCodingSearchDto)
        {
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == meddraCodingSearchDto.ProjectDesignVariableId
            && x.MeddraLowLevelTermId != null
            && x.DeletedDate == null).ToList();
            var projectList = _projectRightRepository.GetProjectRightIdList();
            MeddraCodingSearchDetails objList = new MeddraCodingSearchDetails();
            var dataEntries = new List<MeddraCodingSearchDetails>();

            var r1 = (from stv in Context.ScreeningTemplateValue
                      join pdvv in Context.ProjectDesignVariableValue on stv.Value equals pdvv.ValueName
                      join st in Context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningStatus.Pending && t.Status != ScreeningStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister.Where(t => t.DeletedDate == null && t.RandomizationNumber != null) on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()
                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on pdvv.ValueName equals mllt.llt_name
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code
                      where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
                      && nonregister.RandomizationNumber != null
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.RandomizationNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = pdvv.ValueName,
                          SocCode = soc.soc_code.ToString(),
                          PT = md.pt_name,
                          HLT = md.hlt_name,
                          HLGT = md.hlgt_name,
                          SOCValue = md.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id,
                          MeddraSocTermId = soc.Id,
                          SiteCode = se.Project.ProjectCode
                      }).ToList();
            dataEntries.AddRange(r1);

            var r2 = (from stv in Context.ScreeningTemplateValue
                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on stv.Value equals mllt.llt_name
                      join st in Context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningStatus.Pending && t.Status != ScreeningStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister.Where(t => t.DeletedDate == null && t.RandomizationNumber != null) on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code

                      where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
                      && nonregister.RandomizationNumber != null
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.RandomizationNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = stv.Value,
                          SocCode = soc.soc_code.ToString(),
                          PT = md.pt_name,
                          HLT = md.hlt_name,
                          HLGT = md.hlgt_name,
                          SOCValue = soc.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id,
                          MeddraSocTermId = soc.Id,

                          SiteCode = se.Project.ProjectCode
                      }).ToList();
            dataEntries.AddRange(r2);

            var r3 = (from stv in Context.ScreeningTemplateValue
                      join stvc in Context.ScreeningTemplateValueChild on stv.Id equals stvc.ScreeningTemplateValueId
                      join pdvv in Context.ProjectDesignVariableValue on stvc.ProjectDesignVariableValueId equals pdvv.Id
                      join st in Context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningStatus.Pending && t.Status != ScreeningStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
                      join visit in Context.ProjectDesignVisit on st.ProjectDesignVisitId equals visit.Id
                      join pt in Context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in Context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningEntryId equals se.Id
                      join attendance in Context.Attendance.Where(t => t.DeletedDate == null) on se.AttendanceId equals attendance.Id
                      join volunteerTemp in Context.Volunteer on attendance.VolunteerId equals volunteerTemp.Id into volunteerDto
                      from volunteer in volunteerDto.DefaultIfEmpty()
                      join noneregisterTemp in Context.NoneRegister.Where(t => t.DeletedDate == null && t.RandomizationNumber != null) on attendance.Id equals noneregisterTemp.AttendanceId into noneregisterDto
                      from nonregister in noneregisterDto.DefaultIfEmpty()
                      join projectSubjectTemp in Context.ProjectSubject on attendance.ProjectSubjectId equals projectSubjectTemp.Id into projectsubjectDto
                      from projectsubject in projectsubjectDto.DefaultIfEmpty()
                      join mllt in Context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId)
                      on pdvv.ValueName equals mllt.llt_name
                      join pdv in Context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
                      join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code
                      where stvc.Value == "True" && pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
                      && nonregister.RandomizationNumber != null
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = volunteer.FullName == null ? nonregister.RandomizationNumber : volunteer.VolunteerNo,
                          VisitName = visit.DisplayName,
                          TemplateName = pt.TemplateName,
                          LLTValue = mllt.llt_name,
                          Value = pdvv.ValueName,
                          SocCode = soc.soc_code.ToString(),
                          PT = md.pt_name,
                          HLT = md.hlt_name,
                          HLGT = md.hlgt_name,
                          SOCValue = soc.soc_name,
                          PrimarySoc = md.primary_soc_fg,
                          MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
                          ScreeningTemplateValueId = stv.Id,
                          MeddraLowLevelTermId = mllt.Id,
                          MeddraSocTermId = soc.Id,
                          SiteCode = se.Project.ProjectCode
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

        public void UpdateScopingVersion(StudyScoping model)
        {
            var Exists = All.Where(x => x.DeletedDate == null && x.ScreeningTemplateValue.ProjectDesignVariableId == model.ProjectDesignVariableId).ToList();
            if (Exists.Count > 0)
            {
                foreach (var item in Exists)
                {
                    item.MeddraConfigId = model.MedraConfigId;
                    item.CodedType = CodedType.ReCoded;
                    item.ModifiedBy = _jwtTokenAccesser.UserId;
                    item.CreatedRole = _jwtTokenAccesser.RoleId;
                    item.ModifiedDate = DateTime.Now.ToUniversalTime();
                    item.MeddraLowLevelTermId = null;
                    item.MeddraSocTermId = null;
                    item.IsApproved = false;
                    item.ApprovedBy = null;
                    Update(item);
                    _meddraCodingCommentRepository.CheckWhileScopingVersionUpdate(item.Id);
                    _meddraCodingAuditRepository.SaveAudit("", item.Id, null, null, "Recode by dictionary version update.", null, null);
                }
            }
        }

        public void UpdateSelfCorrection(int ScreeningTemplateValueId)
        {
            var data = All.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == ScreeningTemplateValueId).FirstOrDefault();
            if (data != null)
            {
                data.CodedType = CodedType.ReCoded;
                data.ModifiedBy = _jwtTokenAccesser.UserId;
                data.CreatedRole = _jwtTokenAccesser.RoleId;
                data.ModifiedDate = DateTime.Now.ToUniversalTime();
                data.MeddraLowLevelTermId = null;
                data.MeddraSocTermId = null;
                data.IsApproved = false;
                data.ApprovedBy = null;
                Update(data);
                _meddraCodingAuditRepository.SaveAudit("", data.Id, null, null, "Recode by data entry value update.", null, null);
            }
        }

        public MeddraCoding CheckForRecode(int ScreeningTemplateValueId)
        {
            return All.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == ScreeningTemplateValueId && x.CodedType == CodedType.ReCoded).FirstOrDefault();
        }

        public MeddraCoding GetRecordForComment(int ScreeningTemplateValueId)
        {
            return All.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == ScreeningTemplateValueId).FirstOrDefault();
        }

        public MeddraCodingMainDto GetCoderandApprovalProfile(int ProjectDesignVariableId)
        {
            MeddraCodingMainDto objList = new MeddraCodingMainDto();

            var Coder = Context.StudyScoping.Where(t => t.ProjectDesignVariableId == ProjectDesignVariableId && t.DeletedDate == null).FirstOrDefault();
            if (Coder != null)
            {
                if (Coder.CoderProfile == _jwtTokenAccesser.RoleId)
                    objList.IsCoding = true;
                else
                    objList.IsCoding = false;

                if (Coder.CoderApprover != null)
                    objList.IsShow = true;
                else
                    objList.IsShow = false;

                if (Coder.CoderApprover == _jwtTokenAccesser.RoleId)
                    objList.IsApproveProfile = true;
                else
                    objList.IsApproveProfile = false;
            }
            else
            {
                objList.IsCoding = false;
                objList.IsShow = false;
                objList.IsApproveProfile = false;
            }
            return objList;
        }

        public void UpdateEditCheck(int ScreeningTemplateValueId)
        {
            var data = All.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == ScreeningTemplateValueId).FirstOrDefault();
            if (data != null)
            {
                data.CodedType = CodedType.ReCoded;
                data.CreatedRole = _jwtTokenAccesser.RoleId;
                data.MeddraLowLevelTermId = null;
                data.MeddraSocTermId = null;
                data.IsApproved = false;
                data.ApprovedBy = null;
                data.DeletedBy = _jwtTokenAccesser.UserId;
                data.DeletedDate = DateTime.Now.ToUniversalTime();
                Update(data);
                _meddraCodingAuditRepository.SaveAudit("Remove from meddra Coding", data.Id, null, null, "Removed due to edit check fired.", null, null);
            }
        }

    }
}
