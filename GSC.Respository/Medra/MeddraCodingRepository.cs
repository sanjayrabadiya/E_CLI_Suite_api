using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using GSC.Respository.PropertyMapping;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Medra
{
    public class MeddraCodingRepository : GenericRespository<MeddraCoding>, IMeddraCodingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IMeddraCodingAuditRepository _meddraCodingAuditRepository;
        private readonly IMeddraCodingCommentRepository _meddraCodingCommentRepository;
        private readonly IGSCContext _context;

        public MeddraCodingRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IUserRepository userRepository, IRoleRepository roleRepository, IProjectRightRepository projectRightRepository,
            IMeddraCodingAuditRepository meddraCodingAuditRepository, IMeddraCodingCommentRepository meddraCodingCommentRepository) : base(context)

        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _projectRightRepository = projectRightRepository;
            _meddraCodingAuditRepository = meddraCodingAuditRepository;
            _meddraCodingCommentRepository = meddraCodingCommentRepository;
            _context = context;
        }

        public List<MeddraCodingVariableDto> SearchMain(MeddraCodingSearchDto meddraCodingSearchDto)
        {

            var variable = _context.StudyScoping.Where(t => t.DeletedDate == null && t.ProjectId == meddraCodingSearchDto.ProjectDesignId).
                Select(r => new MeddraCodingVariableDto
                {
                    MeddraConfigId = r.MedraConfigId,
                    ProjectDesignTemplateId = r.ProjectDesignVariable.ProjectDesignTemplateId,
                    VariableName = r.ProjectDesignVariable.VariableName,
                    ProjectDesignVariableId = r.ProjectDesignVariableId,
                    VariableCode = r.ProjectDesignVariable.VariableCode,
                    VariableAlias = r.ProjectDesignVariable.VariableAlias,
                    TemplateName = r.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                    VisitName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                    PeriodName = r.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.DisplayName
                }).ToList();

            return variable;
        }

        public MeddraCodingMainDto GetVariableCount(MeddraCodingSearchDto meddraCodingDto)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();

            var Exists = All.Where(x => x.ScreeningTemplateValue.
            ProjectDesignVariableId == meddraCodingDto.ProjectDesignVariableId &&
            projectList.Contains(x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId) && x.DeletedDate == null && x.MeddraSocTermId != null);

            if (meddraCodingDto.ProjectId != 0)
            {
                Exists = Exists.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectId == meddraCodingDto.ProjectId);
            }
            if (meddraCodingDto.CountryId != 0)
            {
                Exists = Exists.Where(x => x.ScreeningTemplateValue.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.CountryId == meddraCodingDto.CountryId);
            }

            var result = Exists.ToList();

            MeddraCodingMainDto objList = new MeddraCodingMainDto();

            var variable = (from st in _context.ScreeningTemplate
                            join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                            join pdv in _context.ProjectDesignVariable on pt.Id equals pdv.ProjectDesignTemplateId
                            join se in _context.ScreeningEntry on st.ScreeningVisit.ScreeningEntryId equals se.Id
                            //   join attendance in _context.Attendance on se.AttendanceId equals attendance.Id
                            join project in _context.Project.Where(x => projectList.Contains(x.Id)) on se.ProjectId equals project.Id
                            join counry in _context.Country on project.CountryId equals counry.Id
                            where pdv.DeletedDate == null && pdv.Id == meddraCodingDto.ProjectDesignVariableId && st.Status != ScreeningTemplateStatus.Pending && st.Status != ScreeningTemplateStatus.InProcess
                            && (meddraCodingDto.ProjectId != 0 ? se.ProjectId == meddraCodingDto.ProjectId : true)
                            && (meddraCodingDto.CountryId != 0 ? project.CountryId == meddraCodingDto.CountryId : true)
                            // && se.Randomization.RandomizationNumber != null
                            group new { pdv } by new { pdv.Id } into g
                            select new MeddraCodingMainDto
                            {
                                All = g.Count()
                            }).FirstOrDefault();

            objList.All = variable == null ? 0 : variable.All;


            if (result.Any())
            {
                objList.CodedData = result.Count;
                objList.ApprovalData = result.FindAll(t => t.IsApproved).Count;
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

            var Coder = _context.StudyScoping.Where(t => t.ProjectDesignVariableId == meddraCodingDto.ProjectDesignVariableId && t.DeletedDate == null).FirstOrDefault();
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

        

        public List<DropDownDto> MeddraCodingVariableDropDown(int ProjectId)
        {
            var variable = _context.StudyScoping.Where(t => t.DeletedDate == null && t.ProjectId == ProjectId).Select(r => new DropDownDto
            {
                Id = r.ProjectDesignVariableId,
                Value = r.ProjectDesignVariable.Annotation + "-" + r.MedraConfig.MedraVersion.Dictionary.DictionaryName + "-" + r.MedraConfig.MedraVersion.Version + "-" + r.MedraConfig.Language.LanguageName,
                ExtraData = r.MedraConfigId
            }).ToList();

            return variable;
        }

        public IList<MeddraCodingSearchDetails> GetMedDRACodingDetails(MeddraCodingSearchDto filters)
        {
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == filters.ProjectDesignVariableId && x.DeletedDate == null);

            var projectList = _projectRightRepository.GetProjectRightIdList();

            var result = (from se in _context.ScreeningEntry
                          join project in _context.Project.Where(x => projectList.Contains(x.Id)) on se.ProjectId equals project.Id
                          join st in _context.ScreeningTemplate.Where(t => t.DeletedDate == null &&

                          ((filters.TemplateStatus != null && filters.ExtraData == false) ? t.Status == ScreeningTemplateStatus.Reviewed : true) &&
                          ((filters.TemplateStatus != null && filters.ExtraData == true) ? t.ReviewLevel == filters.TemplateStatus : true) &&
                          t.Status != ScreeningTemplateStatus.Pending && t.Status != ScreeningTemplateStatus.InProcess)

                          on se.Id equals st.ScreeningVisit.ScreeningEntryId


                          join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                          join visit in _context.ProjectDesignVisit on pt.ProjectDesignVisitId equals visit.Id
                          join pdv in _context.ProjectDesignVariable.Where(val => val.DeletedDate == null && val.Id == filters.ProjectDesignVariableId)
                                           on new { Id1 = pt.Id } equals new { Id1 = pdv.ProjectDesignTemplateId }
                          join value in _context.ScreeningTemplateValue.Where(val => val.DeletedDate == null) on new
                          { Id = st.Id, Id1 = pdv.Id } equals new
                          { Id = value.ScreeningTemplateId, Id1 = value.ProjectDesignVariableId }
                          join randomizationTemp in _context.Randomization.Where(t => t.DeletedDate == null)
                          on se.RandomizationId equals randomizationTemp.Id into randomizationDto
                          from randomization in randomizationDto.DefaultIfEmpty()
                          join medraCoding in _context.MeddraCoding.Where(t => t.DeletedDate == null) on value.Id equals medraCoding.ScreeningTemplateValueId into medraDto
                          from meddraCoding in medraDto.DefaultIfEmpty()
                          join soc in _context.MeddraSocTerm on meddraCoding.MeddraSocTermId equals soc.Id into socDto
                          from meddraSoc in socDto.DefaultIfEmpty()
                          join mllt in _context.MeddraLowLevelTerm on meddraCoding.MeddraLowLevelTermId equals mllt.Id into mlltDto
                          from meddraLLT in mlltDto.DefaultIfEmpty()
                          join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == filters.MeddraConfigId)
                          on meddraSoc.soc_code equals md.soc_code into mdDto
                          from meddraMD in mdDto.DefaultIfEmpty()
                          join users in _context.Users on meddraCoding.LastUpdateBy equals users.Id into userDto
                          from user in userDto.DefaultIfEmpty()
                          join roles in _context.SecurityRole on meddraCoding.CreatedRole equals roles.Id into roleDto
                          from role in roleDto.DefaultIfEmpty()
                          join users in _context.Users on meddraCoding.ApprovedBy equals users.Id into usersDto
                          from approvedBy in usersDto.DefaultIfEmpty()
                          where meddraLLT.pt_code == meddraMD.pt_code && pdv.Id == filters.ProjectDesignVariableId                        
                          && ((filters.ProjectId != 0 ? se.ProjectId == filters.ProjectId : true))
                                  && ((filters.CountryId != 0 ? project.CountryId == filters.CountryId : true))
                                  && (filters.Status != null ? (filters.Status != CodedType.UnCoded ? meddraCoding.CodedType == filters.Status :
                                   !(Exists.Where(o => o.MeddraLowLevelTermId != null && o.MeddraSocTermId != null).Select(o => o.ScreeningTemplateValueId)).Contains(value.Id)) : true)
                                   && (filters.IsApproved != null ? (meddraCoding.IsApproved == true ? meddraCoding.IsApproved == filters.IsApproved :
                                   (Exists.Where(o => o.IsApproved == filters.IsApproved).Select(o => o.ScreeningTemplateValueId)).Contains(value.Id)) : true)
                                   && ((filters.FromDate.HasValue ? meddraCoding.CreatedDate >= filters.FromDate : true))
                             && ((filters.ToDate.HasValue ? meddraCoding.CreatedDate <= filters.ToDate : true))
                          select new MeddraCodingSearchDetails
                          {
                              MeddraCodingId = meddraCoding.Id,
                              SubjectId = randomization.RandomizationNumber,
                              ScreeningNumber = randomization.ScreeningNumber,
                              VisitName = visit.DisplayName,
                              TemplateName = pt.TemplateName,
                              Value = value.ProjectDesignVariable.CollectionSource == CollectionSources.MultiCheckBox ? string.Join(";",
                                        from stvc in _context.ScreeningTemplateValueChild.Where(x => x.DeletedDate == null && x.ScreeningTemplateValueId == value.Id && x.Value == "true")
                                        join prpjectdesignvalueTemp in _context.ProjectDesignVariableValue.Where(val => val.DeletedDate == null) on stvc.ProjectDesignVariableValueId equals prpjectdesignvalueTemp.Id into
                                        prpjectdesignvalueDto
                                        from prpjectdesignvalue in prpjectdesignvalueDto.DefaultIfEmpty()
                                        select prpjectdesignvalue.ValueName)
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.CheckBox &&
                                        !string.IsNullOrEmpty(value.Value)
                                        ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                        b.ProjectDesignVariableId == value.ProjectDesignVariable.Id).ValueName
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.TextBox &&
                                        value.IsNa && string.IsNullOrEmpty(value.Value) ? "NA"
                                        : value.ProjectDesignVariable.CollectionSource == CollectionSources.ComboBox ||
                                        value.ProjectDesignVariable.CollectionSource == CollectionSources.RadioButton ||
                                         value.ProjectDesignVariable.CollectionSource == CollectionSources.NumericScale
                                        ? _context.ProjectDesignVariableValue.FirstOrDefault(b =>
                                        b.ProjectDesignVariableId == value.ProjectDesignVariable.Id &&
                                        b.Id == Convert.ToInt32(value.Value)).ValueName
                                        : value.Value,
                              CodedType = meddraCoding.CodedType,
                              CodingType = meddraCoding.CodingType,
                              CommentStatus = meddraCoding.Id == null ? 0 :
                              _context.MeddraCodingComment.Where(x => x.MeddraCodingId == meddraCoding.Id).OrderByDescending(o => o.Id).FirstOrDefault().CommentStatus,
                              Code = meddraLLT.llt_name,
                              LLT = meddraLLT.llt_code == null ? 0 : meddraLLT.llt_code,
                              LastUpdateOn = meddraCoding.ModifiedDate,
                              UpdatedBy = user.UserName + " (" + role.RoleName + ")",
                              ScreeningTemplateValueId = value.Id,
                              SiteCode = project.ProjectCode,
                              PT = meddraMD.pt_name,
                              HLT = meddraMD.hlt_name,
                              HLGT = meddraMD.hlgt_name,
                              SOCValue = meddraMD.soc_name,
                              SocCode = meddraMD.pt_code.ToString(),
                              IsApproved = meddraCoding.IsApproved,
                              PrimarySoc = meddraMD.primary_soc_fg,
                              RandomizationId = randomization.Id,
                              ApprovedBy = approvedBy.UserName,
                              ApprovedOn = meddraCoding.ApproveDate
                          }).ToList();

            if (filters.CommentStatus != null)
            {
                result = result.Where(x => (int?)x.CommentStatus == filters.CommentStatus).ToList();
            }

            if (filters.Value.Trim().ToLower() != "")
            {
                result = result.Where(x => x.Value != null ? x.Value.Trim().ToLower().Contains(filters.Value.Trim().ToLower()) : false).ToList();
            }

            if (filters.SubjectIds != null && filters.SubjectIds.Length > 0)
            {
                result = result.Where(x => filters.SubjectIds.Contains(x.RandomizationId)).ToList();
            }



            return result.ToList();
        }

        public IList<MeddraCodingSearchDetails> AutoCodes(MeddraCodingSearchDto meddraCodingSearchDto)
        {
            var Exists = All.Where(x => x.ScreeningTemplateValue.ProjectDesignVariableId == meddraCodingSearchDto.ProjectDesignVariableId
            && x.MeddraLowLevelTermId != null
            && x.DeletedDate == null).ToList();


            var projectList = meddraCodingSearchDto.ProjectId == 0 ? _projectRightRepository.GetProjectRightIdList() : new List<int> { (int)meddraCodingSearchDto.ProjectId };

            var dataEntries = new List<MeddraCodingSearchDetails>();

            //var r1 = (from stv in _context.ScreeningTemplateValue
            //          join pdvv in _context.ProjectDesignVariableValue on stv.Value equals pdvv.ValueName
            //          join st in _context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningTemplateStatus.Pending && t.Status != ScreeningTemplateStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
            //          join scrVisit in _context.ScreeningVisit.Where(t => t.DeletedDate == null) on st.ScreeningVisitId equals scrVisit.Id
            //          join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
            //          join se in _context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningVisit.ScreeningEntryId equals se.Id 
            //          join randomizationTemp in _context.Randomization.Where(t => t.DeletedDate == null) on se.RandomizationId equals randomizationTemp.Id into randomizationDto
            //          from randomization in randomizationDto.DefaultIfEmpty()      
            //          join mllt in _context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on pdvv.ValueName equals mllt.llt_name
            //          join pdv in _context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
            //          join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
            //          join soc in _context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code
            //          where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
            //          select new MeddraCodingSearchDetails
            //          {
            //              SubjectId = randomization.RandomizationNumber,
            //              ScreeningNumber = randomization.ScreeningNumber,
            //              VisitName = scrVisit.ProjectDesignVisit.DisplayName,
            //              TemplateName = pt.TemplateName,
            //              LLTValue = mllt.llt_name,
            //              Value = pdvv.ValueName,
            //              SocCode = soc.soc_code.ToString(),
            //              PT = md.pt_name,
            //              HLT = md.hlt_name,
            //              HLGT = md.hlgt_name,
            //              SOCValue = md.soc_name,
            //              PrimarySoc = md.primary_soc_fg,
            //              MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
            //              ScreeningTemplateValueId = stv.Id,
            //              MeddraLowLevelTermId = mllt.Id,
            //              MeddraSocTermId = soc.Id,
            //              SiteCode = se.Project.ProjectCode
            //          }).ToList();
            //dataEntries.AddRange(r1);

            var r2 = (from stv in _context.ScreeningTemplateValue
                      join mllt in _context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on stv.Value equals mllt.llt_name
                      join st in _context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningTemplateStatus.Pending && t.Status != ScreeningTemplateStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
                      join pdv in _context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
                      join scrVisit in _context.ScreeningVisit.Where(t => t.DeletedDate == null) on st.ScreeningVisitId equals scrVisit.Id
                      join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
                      join se in _context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningVisit.ScreeningEntryId equals se.Id
                      join randomizationTemp in _context.Randomization.Where(t => t.DeletedDate == null) on se.RandomizationId equals randomizationTemp.Id into randomizationDto
                      from randomization in randomizationDto.DefaultIfEmpty()
                      join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
                      join soc in _context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code

                      where pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
                      select new MeddraCodingSearchDetails
                      {
                          SubjectId = randomization.RandomizationNumber,
                          ScreeningNumber = randomization.ScreeningNumber,
                          VisitName = scrVisit.ProjectDesignVisit.DisplayName,
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

            //var r3 = (from stv in _context.ScreeningTemplateValue
            //          join stvc in _context.ScreeningTemplateValueChild on stv.Id equals stvc.ScreeningTemplateValueId
            //          join pdvv in _context.ProjectDesignVariableValue on stvc.ProjectDesignVariableValueId equals pdvv.Id
            //          join st in _context.ScreeningTemplate.Where(t => t.DeletedDate == null && t.Status != ScreeningTemplateStatus.Pending && t.Status != ScreeningTemplateStatus.InProcess) on stv.ScreeningTemplateId equals st.Id
            //          join scrVisit in _context.ScreeningVisit.Where(t => t.DeletedDate == null) on st.ScreeningVisitId equals scrVisit.Id
            //          join pt in _context.ProjectDesignTemplate on st.ProjectDesignTemplateId equals pt.Id
            //          join se in _context.ScreeningEntry.Where(x => projectList.Contains(x.Project.Id)) on st.ScreeningVisit.ScreeningEntryId equals se.Id
            //          join randomizationTemp in _context.Randomization.Where(t => t.DeletedDate == null) on se.RandomizationId equals randomizationTemp.Id into randomizationDto
            //          from randomization in randomizationDto.DefaultIfEmpty()
            //          join mllt in _context.MeddraLowLevelTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId)
            //          on pdvv.ValueName equals mllt.llt_name
            //          join pdv in _context.ProjectDesignVariable on stv.ProjectDesignVariableId equals pdv.Id
            //          join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId && t.primary_soc_fg == "Y") on mllt.pt_code equals md.pt_code
            //          join soc in _context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == meddraCodingSearchDto.MeddraConfigId) on md.soc_code equals soc.soc_code
            //          where stvc.Value == "True" && pdv.Id == meddraCodingSearchDto.ProjectDesignVariableId && !(from o in Exists select o.ScreeningTemplateValueId).Contains(stv.Id)
            //          select new MeddraCodingSearchDetails
            //          {
            //              SubjectId = randomization.RandomizationNumber,
            //              ScreeningNumber = randomization.ScreeningNumber,
            //              VisitName = scrVisit.ProjectDesignVisit.DisplayName,
            //              TemplateName = pt.TemplateName,
            //              LLTValue = mllt.llt_name,
            //              Value = pdvv.ValueName,
            //              SocCode = soc.soc_code.ToString(),
            //              PT = md.pt_name,
            //              HLT = md.hlt_name,
            //              HLGT = md.hlgt_name,
            //              SOCValue = soc.soc_name,
            //              PrimarySoc = md.primary_soc_fg,
            //              MeddraConfigId = meddraCodingSearchDto.MeddraConfigId,
            //              ScreeningTemplateValueId = stv.Id,
            //              MeddraLowLevelTermId = mllt.Id,
            //              MeddraSocTermId = soc.Id,
            //              SiteCode = se.Project.ProjectCode
            //          }).ToList();
            //dataEntries.AddRange(r3);
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
                    item.ModifiedDate = _jwtTokenAccesser.GetClientDate();
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
                data.LastUpdateBy = _jwtTokenAccesser.UserId;
                data.CreatedRole = _jwtTokenAccesser.RoleId;
                data.ModifiedDate = _jwtTokenAccesser.GetClientDate();
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

            var Coder = _context.StudyScoping.Where(t => t.ProjectDesignVariableId == ProjectDesignVariableId && t.DeletedDate == null).FirstOrDefault();
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
                data.DeletedDate = _jwtTokenAccesser.GetClientDate();
                Update(data);
                _meddraCodingAuditRepository.SaveAudit("Remove from meddra Coding", data.Id, null, null, "Removed due to edit check fired.", null, null);
            }
        }

    }
}
