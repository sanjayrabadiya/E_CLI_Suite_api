using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.ProjectRight;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignRepository : GenericRespository<ProjectDesign, GscContext>, IProjectDesignRepository
    {
        private readonly GscContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        public ProjectDesignRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = uow.Context;
            _projectRightRepository = projectRightRepository;
            _uow = uow;
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
                    Value = c.Project.ProjectCode
                }).OrderBy(o => o.Value).ToList();
        }


        public async Task<ProjectDetailDto> GetProjectDesignDetail(int projectId)
        {
            return All.Where(x => x.ProjectId == projectId).Include(t => t.Project)
                .Select(c => new ProjectDetailDto
                {
                    ProjectId = c.ProjectId,
                    ProjectName = c.Project.ProjectName,
                    ProjectCode = c.Project.ProjectCode,
                    ProjectNumber = c.Project.ProjectNumber,
                    ProjectDesignPeriod = _context.ProjectDesignPeriod.Where(r => r.ProjectDesignId == c.Id
                                                                                  && r.DeletedDate == null).Select(
                        a => new ProjectDesignPeriodDto
                        {
                            DisplayName = a.DisplayName,
                            Description = a.Description,
                            ProjectDesignVisits = a.VisitList.Where(s => s.DeletedDate == null).Select(
                                v => new ProjectDesignVisitDto
                                {
                                    DisplayName = v.DisplayName,
                                    Description = v.Description,
                                    Templates = v.Templates.Where(m => m.DeletedDate == null).Select(
                                        b => new ProjectDesignTemplateDto
                                        {
                                            TemplateCode = b.TemplateCode,
                                            TemplateName = b.TemplateName,
                                            DesignOrder = b.DesignOrder,
                                            ActivityName = b.ActivityName,
                                            ParentId = b.ParentId,
                                            //DomainName = b.Domain.DomainName,
                                            Variables = b.Variables.Where(f => f.DeletedDate == null).Select(n =>
                                                new ProjectDesignVariableDto
                                                {
                                                    VariableCode = n.VariableCode,
                                                    VariableName = n.VariableName
                                                }
                                            ).ToList()
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                }).FirstOrDefault();
        }

        public bool IsScreeningStarted(int projectDesignId)
        {
            return _context.ScreeningEntry.Any(t => t.ProjectDesignId == projectDesignId && t.DeletedDate == null);
        }

        public string CheckCompleteDesign(int id)
        {
            var msg = "";

            if (!_context.ProjectWorkflow.Any(x => x.ProjectDesignId == id && x.DeletedDate == null))
                msg = "Workflow not defined! \n";

            if (!_context.ProjectSchedule.Any(x => x.ProjectDesignId == id && x.DeletedDate == null))
                msg += "Schedule not defined! \n";

            //if (!_context.EditCheck.Any(x => x.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == id && x.DeletedDate == null))
            //    msg += "Edit check not defined! \n";

            return msg;
        }

        public string Duplicate(ProjectDesign objSave)
        {
            var project = Context.Project.Where(x => x.Id == objSave.ProjectId).FirstOrDefault();
            if (All.Any(x => x.Id != objSave.Id && x.ProjectId == objSave.ProjectId && x.DeletedDate == null))
                return "Duplicate Design : " + project.ProjectName;
            return "";
        }


        public bool IsCompleteExist(int projectDesignId, string moduleName, bool isComplete)
        {
            var exist = Context.ElectronicSignature.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).FirstOrDefault();
            var electronicSignature = new ElectronicSignature();
            electronicSignature.ProjectDesignId = projectDesignId;

            if (moduleName == "workflow")
            {
                if (exist != null)
                {
                    exist.IsCompleteWorkflow = isComplete;
                }
                else
                {
                    electronicSignature.IsCompleteWorkflow = isComplete;
                }
            }
            else if (moduleName == "schedule")
            {
                if (exist != null)
                {
                    exist.IsCompleteSchedule = isComplete;
                }
                else
                {
                    electronicSignature.IsCompleteSchedule = isComplete;
                }
            }
            else if (moduleName == "editcheck")
            {
                if (exist != null)
                {
                    exist.IsCompleteEditCheck = isComplete;
                }
                else
                {
                    electronicSignature.IsCompleteEditCheck = isComplete;
                }
            }
            else if (moduleName == "design")
            {
                if (exist != null)
                {
                    exist.IsCompleteDesign = isComplete;
                }
                else
                {
                    electronicSignature.IsCompleteDesign = isComplete;
                }
            }


            if (exist == null)
            {
                Context.ElectronicSignature.Add(electronicSignature);
            }
            else
            {
                Context.ElectronicSignature.Update(exist);
            }
            _uow.Save();
            return true;
        }

        public int GetParentProjectDetail(int ProjectDesignId)
        {
            return Find(ProjectDesignId).ProjectId;
        }
    }
}