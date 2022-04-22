using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Workflow
{
    public class ProjectWorkflowRepository : GenericRespository<ProjectWorkflow>, IProjectWorkflowRepository
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;

        public ProjectWorkflowRepository(IGSCContext context,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }

        public int GetMaxWorkFlowLevel(int projectDesignId)
        {
            if (_context.ProjectWorkflowLevel.Any(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                                                          && x.DeletedDate == null))
                return _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                                                              && x.DeletedDate == null).Max(t => t.LevelNo);
            else
                return 0;
        }

        public short GetNoCRFLevel(int projectDesignId, short levelNo)
        {
            var projectWorkId = FindBy(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).Select(t => t.Id).FirstOrDefault();

            var result = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWorkId
                && x.DeletedDate == null && x.IsNoCRF
                && x.LevelNo > levelNo).Min(a => (short?)a.LevelNo) ?? 0;

            if (result == 0)
                return (short)(GetMaxWorkFlowLevel(projectDesignId) + 1);

            return result;

        }

        public short GetNextLevelWorkBreak(int projectDesignId, short levelNo)
        {

            var result = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                && x.DeletedDate == null && x.IsWorkFlowBreak
                && x.LevelNo > levelNo).Max(a => (short?)a.LevelNo) ?? 0;

            if (result == 0)
            {
                result = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                 && x.DeletedDate == null).Max(a => (short?)a.LevelNo) ?? 0;

            }

            return result;

        }


        public short GetMaxLevelWorkBreak(int projectDesignId)
        {

            var result = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflow.ProjectDesignId == projectDesignId
                && x.DeletedDate == null && x.IsWorkFlowBreak).Max(a => (short?)a.LevelNo) ?? 0;

       
            return result;

        }

        public WorkFlowLevelDto GetProjectWorkLevel(int projectDesignId)
        {
            short levelNo = -1;
            var projectWorkId = FindBy(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).Select(t => t.Id).FirstOrDefault();
            var workFlowText = new List<WorkFlowText>();

            var projectWorkflowLevel = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWorkId
                                                                      && x.DeletedDate == null).ToList();

            if (projectWorkId > 0)
            {
                workFlowText = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWorkId && x.DeletedDate == null).Select(r => new WorkFlowText
                {
                    LevelNo = r.LevelNo,
                    RoleName = r.SecurityRole.RoleShortName
                }).ToList();
            }

            if (projectWorkId == 0)
                return new WorkFlowLevelDto
                {
                    IsLock = false,
                    WorkFlowText = workFlowText,
                    IsStartTemplate = true,
                    IsWorkFlowBreak = false,
                    LevelNo = levelNo,
                    SelfCorrection = false
                };

            var independent = _context.ProjectWorkflowIndependent.Where(x => x.ProjectWorkflowId == projectWorkId
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
                    IsWorkFlowBreak = projectWorkflowLevel.Any(t => t.IsWorkFlowBreak),
                    LevelNo = levelNo,
                    SelfCorrection = independent.IsDataEntryUser,
                    IsGenerateQuery = independent.IsGenerateQuery
                };

            var level = projectWorkflowLevel.Where(x => x.SecurityRoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).FirstOrDefault();

            var totalLevels = projectWorkflowLevel.Select(c => c.LevelNo).ToList();

            int totalLevel = 0;
            if (totalLevels != null && totalLevels.Count > 0)
                totalLevel = totalLevels.Max(t => t);


            if (level != null)
                return new WorkFlowLevelDto
                {
                    IsLock = level.IsLock,
                    IsNoCRF = level.IsNoCRF,
                    WorkFlowText = workFlowText,
                    TotalLevel = totalLevel,
                    IsStartTemplate = level.IsStartTemplate,
                    IsWorkFlowBreak = projectWorkflowLevel.Any(t => t.IsWorkFlowBreak),
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
                IsWorkFlowBreak = projectWorkflowLevel.Any(t => t.IsWorkFlowBreak),
                LevelNo = levelNo,
                SelfCorrection = false
            };
        }

        public bool IsElectronicsSignatureComplete(int projectDesignId)
        {
            var IsElectronicsSignature = _context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault()?.IsCompleteWorkflow;
            if (IsElectronicsSignature == null)
            {
                IsElectronicsSignature = false;
            }
            return (bool)IsElectronicsSignature;
        }
    }
}