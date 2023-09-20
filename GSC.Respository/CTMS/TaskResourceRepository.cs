using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
namespace GSC.Respository.CTMS
{
    public class TaskResourceRepository : GenericRespository<TaskResource>, ITaskResourceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public TaskResourceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId)
        {
            var gridResource = _context.TaskResource.Include(r=>r.ResourceType).ThenInclude(d=>d.Designation).Where(x => x.TaskMasterId == PlanTaskId && x.DeletedDate == null)
               .Select(c => new ResourceTypeGridDto
               {
                   Id = c.Id,
                   ResourceType = c.ResourceType.ResourceTypes.GetDescription(),
                   ResourceSubType = c.ResourceType.ResourceSubType.GetDescription(),
                   Designation = c.ResourceType.Designation.NameOFDesignation != null ? c.ResourceType.Designation.NameOFDesignation+" - "+ c.ResourceType.Designation.YersOfExperience+ " Years" : " - ",
                   Role = c.ResourceType.Role.RoleName != null ? c.ResourceType.Role.RoleName+" - " + c.ResourceType.User.UserName : " - ",
                   NameOfMaterial = c.ResourceType.NameOfMaterial != "" ? c.ResourceType.NameOfMaterial : " - ",
                   CreatedDate = c.CreatedDate,
                   CreatedByUser = c.CreatedByUser.UserName,
               }).ToList();

            return gridResource;

        }
        public string Duplicate(TaskResource objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TaskMasterId == objSave.TaskMasterId && x.ResourceTypeId== objSave.ResourceTypeId && x.DeletedDate == null))
                return "Duplicate Resource : ";
            return "";
        }
    }
}

