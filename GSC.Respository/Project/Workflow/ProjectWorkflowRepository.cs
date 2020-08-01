using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;

namespace GSC.Respository.Project.Workflow
{
    public class ProjectWorkflowRepository : GenericRespository<ProjectWorkflow, GscContext>, IProjectWorkflowRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly GscContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;

        public ProjectWorkflowRepository(IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _context = uow.Context;
            _projectRightRepository = projectRightRepository;
        }

        public int GetMaxWorkFlowLevel(int projectDesignId)
        {
            return _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                                                            && x.DeletedDate == null).Max(t => t.LevelNo);
        }

        public IList<ProjectWorkflowDto> GetProjectWorkFlowList(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectWorkflowDto>();

            var projectWorkflows = FindByInclude(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                    && projectList.Any(c => c == x.ProjectDesign.ProjectId), x => x.ProjectDesign.Project)
                .Select(x => new ProjectWorkflowDto
                {
                    Id = x.Id,
                    ProjectName = x.ProjectDesign.Project.ProjectName,
                    IsDeleted = x.DeletedDate != null,
                    IsIndependent = x.IsIndependent,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    DeletedBy = x.DeletedBy,
                    CreatedDate = x.CreatedDate,
                    ModifiedDate = x.ModifiedDate,
                    DeletedDate = x.DeletedDate,
                    IsLock = !x.ProjectDesign.IsUnderTesting,
                 }).OrderByDescending(x => x.Id).ToList();
           
            foreach (var b in projectWorkflows)
            {
                b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            }
            return projectWorkflows;
        }

        public WorkFlowLevelDto GetProjectWorkLevel(int projectDesignId)
        {
            short levelNo = -1;
            var project = FindBy(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();
            var workFlowText = new List<WorkFlowText>();
            if (project != null)
                workFlowText = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == project.Id
                                                                        && x.DeletedDate == null).Select(r =>
                    new WorkFlowText
                    {
                        LevelNo = r.LevelNo,
                        RoleName = r.SecurityRole.RoleShortName
                    }).ToList();

            if (project == null)
                return new WorkFlowLevelDto
                {
                    IsLock = false,
                    WorkFlowText = workFlowText,
                    IsStartTemplate = true,
                    IsWorkFlowBreak = false,
                    LevelNo = levelNo,
                    SelfCorrection = false
                };

            var independent = _context.ProjectWorkflowIndependent.Where(x => x.ProjectWorkflowId == project.Id
                                                                                      && x.SecurityRoleId ==
                                                                                      _jwtTokenAccesser.RoleId
                                                                                      && x.DeletedDate == null).FirstOrDefault();

            levelNo = 0;
            if (independent != null)
                return new WorkFlowLevelDto
                {
                    IsLock = false,
                    WorkFlowText = workFlowText,
                    IsStartTemplate = independent.IsStartTemplate,
                    IsWorkFlowBreak = independent.IsWorkFlowBreak,
                    LevelNo = levelNo,
                    SelfCorrection = independent.IsDataEntryUser,
                    IsGenerateQuery = independent.IsGenerateQuery
                };

            var level = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == project.Id
                                                                          && x.SecurityRoleId ==
                                                                          _jwtTokenAccesser.RoleId
                                                                          && x.DeletedDate == null).FirstOrDefault();

            int totalLevel = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == project.Id
                                                                      && x.DeletedDate == null).Max(x => x.LevelNo);

            if (level != null)
                return new WorkFlowLevelDto
                {
                    IsLock = level.IsLock,
                    WorkFlowText = workFlowText,
                    TotalLevel = totalLevel,
                    IsStartTemplate = level.IsStartTemplate,
                    IsWorkFlowBreak = level.IsWorkFlowBreak,
                    LevelNo = level.LevelNo,
                    SelfCorrection = level.IsDataEntryUser,
                    IsGenerateQuery = level.IsGenerateQuery,
                    IsElectricSignature = level.IsElectricSignature
                };


            levelNo = -1;

            return new WorkFlowLevelDto
            {
                IsLock = false,
                IsStartTemplate = true,
                WorkFlowText = workFlowText,
                IsWorkFlowBreak = false,
                LevelNo = levelNo,
                SelfCorrection = false
            };
        }

        public bool IsElectronicsSignatureComplete(int projectDesignId)
        {
            var IsElectronicsSignature = Context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault()?.IsCompleteWorkflow;
            if (IsElectronicsSignature == null)
            {
                IsElectronicsSignature = false;
            }
            return (bool)IsElectronicsSignature;
        }
    }
}