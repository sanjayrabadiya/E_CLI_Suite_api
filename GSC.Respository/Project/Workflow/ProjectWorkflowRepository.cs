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
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ProjectWorkflowRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
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

        public short GetVisitLevel(int projectDesignVisitId, int projectDesignId, short levelNo)
        {

            var result = _context.WorkflowVisit.Where(x => !x.IsIndependent && x.ProjectDesignVisitId == projectDesignVisitId
            && x.DeletedDate == null && x.ProjectWorkflowLevel.LevelNo > levelNo).Min(a => (short?)a.ProjectWorkflowLevel.LevelNo) ?? 0;

            if (result == 0)
                return (short)(GetMaxWorkFlowLevel(projectDesignId) + 1);

            return result;

        }

        public short GetTemplateWorkFlow(int projectDesignTemplateId, int projectDesignId, short levelNo)
        {
            var result = _context.WorkflowTemplate.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId
           && x.DeletedDate == null).Select(r => r.LevelNo).ToList();

            if (result.Count() == 0)
                return 0;

            var level = result.Where(x => x > (int)levelNo).Select(b => b).DefaultIfEmpty().Min();

            if (level == 0)
                return (short)(GetMaxWorkFlowLevel(projectDesignId) + 1);

            return (short)level;

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
            var projectWork = FindBy(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).Select(t => new
            {
                t.Id,
                t.IsVisitBase
            }).FirstOrDefault();

            var workFlowText = new List<WorkFlowText>();

            if (projectWork == null)
                return new WorkFlowLevelDto
                {
                    IsLock = false,
                    WorkFlowText = workFlowText,
                    IsStartTemplate = true,
                    IsWorkFlowBreak = false,
                    LevelNo = levelNo,
                    IsVisitBase = false,
                    SelfCorrection = false
                };

            var projectWorkflowLevel = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWork.Id
                                                                      && x.DeletedDate == null).ToList();

            workFlowText = _context.ProjectWorkflowLevel.Where(x => x.ProjectWorkflowId == projectWork.Id && x.DeletedDate == null).Select(r => new WorkFlowText
            {
                LevelNo = r.LevelNo,
                RoleName = r.SecurityRole.RoleShortName
            }).ToList();


            var independent = _context.ProjectWorkflowIndependent.Where(x => x.ProjectWorkflowId == projectWork.Id
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
                    IsWorkFlowBreak = projectWorkflowLevel.Exists(t => t.IsWorkFlowBreak),
                    LevelNo = levelNo,
                    SelfCorrection = independent.IsDataEntryUser,
                    IsGenerateQuery = independent.IsGenerateQuery,
                    IsVisitBase = projectWork.IsVisitBase ?? false
                };

            var level = projectWorkflowLevel.Find(x => x.SecurityRoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null);

            var totalLevels = projectWorkflowLevel.Select(c => c.LevelNo).ToList();

            int totalLevel = 0;
            if (totalLevels.Count > 0)
                totalLevel = totalLevels.Max(t => t);


            if (level != null)
                return new WorkFlowLevelDto
                {
                    IsLock = level.IsLock,
                    IsNoCRF = level.IsNoCRF,
                    WorkFlowText = workFlowText,
                    TotalLevel = totalLevel,
                    IsStartTemplate = level.IsStartTemplate,
                    IsWorkFlowBreak = projectWorkflowLevel.Exists(t => t.IsWorkFlowBreak),
                    LevelNo = level.LevelNo,
                    SelfCorrection = level.IsDataEntryUser,
                    IsGenerateQuery = level.IsGenerateQuery,
                    IsElectricSignature = level.IsElectricSignature,
                    IsVisitBase = projectWork.IsVisitBase ?? false
                };


            levelNo = -1;

            return new WorkFlowLevelDto
            {
                IsLock = false,
                IsStartTemplate = true,
                WorkFlowText = workFlowText,
                IsWorkFlowBreak = projectWorkflowLevel.Exists(t => t.IsWorkFlowBreak),
                LevelNo = levelNo,
                SelfCorrection = false,
                IsVisitBase = projectWork.IsVisitBase ?? false
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