using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignRepository : GenericRespository<ProjectDesign>, IProjectDesignRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IGSCContext _context;
        public ProjectDesignRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _context = context;
        }

        public IList<DropDownDto> GetProjectByDesignDropDown()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;
            return All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                                  && x.DeletedDate == null
                                  && projectList.Any(c => c == x.ProjectId)
                ).Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.Project.ProjectCode,
                    ExtraData = c.StudyVersions.Any(t => t.VersionStatus == Helper.VersionStatus.OnTrial && t.DeletedDate == null)

                }).OrderBy(o => o.Value).ToList();
        }

        public bool IsWorkFlowOrEditCheck(int projectDesignid)
        {
            return _context.ProjectWorkflow.Any(x => x.ProjectDesignId == projectDesignid && x.DeletedDate == null)
                 && _context.EditCheck.Any(x => x.ProjectDesignId == projectDesignid && x.DeletedDate == null);


        }

        public bool CheckPeriodWithProjectPeriod(int projectDesignid, int projectId)
        {
            var period = _context.Project.Where(x => x.Id == projectId).Select(t => t.Period).FirstOrDefault();
            return period == _context.ProjectDesignPeriod.Count(x => x.ProjectDesignId == projectDesignid && x.DeletedDate == null);
        }


        public bool IsCompleteExist(int projectDesignId, string moduleName, bool isComplete)
        {
            var exist = _context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();
            var electronicSignature = new ElectronicSignature();
            electronicSignature.ProjectDesignId = projectDesignId;

            if (moduleName == "workflow")
            {
                if (exist != null)
                    exist.IsCompleteWorkflow = isComplete;
                else
                    electronicSignature.IsCompleteWorkflow = isComplete;
            }
            else if (moduleName == "schedule")
            {
                if (exist != null)
                    exist.IsCompleteSchedule = isComplete;
                else
                    electronicSignature.IsCompleteSchedule = isComplete;
            }
            else if (moduleName == "editcheck")
            {
                if (exist != null)
                    exist.IsCompleteEditCheck = isComplete;
                else
                    electronicSignature.IsCompleteEditCheck = isComplete;
            }
            else if (moduleName == "design")
            {
                if (exist != null)
                    exist.IsCompleteDesign = isComplete;
                else
                    electronicSignature.IsCompleteDesign = isComplete;
            }


            if (exist == null)
                _context.ElectronicSignature.Add(electronicSignature);
            else
                _context.ElectronicSignature.Update(exist);
            _context.Save();
            return true;
        }


    }
}