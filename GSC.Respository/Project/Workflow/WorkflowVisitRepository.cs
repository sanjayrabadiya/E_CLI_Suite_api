using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace GSC.Respository.Project.Workflow
{
    public class WorkflowVisitRepository : GenericRespository<WorkflowVisit>, IWorkflowVisitRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public WorkflowVisitRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser) :
            base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public List<int> GetDetailById(WorkflowVisitDto workflowVisitDto)
        {
            var result = All.Where(x=>x.IsIndependent== workflowVisitDto.IsIndependent && x.ProjectWorkflowLevelId== workflowVisitDto.ProjectWorkflowLevelId
           && x.ProjectWorkflowIndependentId == workflowVisitDto.ProjectWorkflowIndependentId && x.DeletedDate==null).Select(x=>x.ProjectDesignVisitId).ToList();

            //if (result != null)
            //    user.UserRoles = user.UserRoles.Where(x => x.DeletedDate == null).ToList();

            //var userDto = _mapper.Map<UserDto>(user);
            //var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            //userDto.ProfilePicPath = imageUrl + (userDto.ProfilePic ?? DocumentService.DefulatProfilePic);

            //return Ok(userDto);

            return result;

        }
    }
}
