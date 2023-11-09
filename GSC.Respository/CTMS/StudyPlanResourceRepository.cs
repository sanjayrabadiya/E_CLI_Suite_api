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
    public class StudyPlanResourceRepository : GenericRespository<StudyPlanResource>, IStudyPlanResourceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public StudyPlanResourceRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public dynamic GetTaskResourceList(bool isDeleted, int PlanTaskId)
        {
            //var gridResource = _context.TaskResource.Include(r=>r.ResourceType).ThenInclude(d=>d.Designation).Where(x => x.TaskMasterId == PlanTaskId && x.DeletedDate == null)
            //   .Select(c => new ResourceTypeGridDto
            //   {
            //       Id = c.Id,
            //       ResourceType = c.ResourceType.ResourceTypes.GetDescription(),
            //       ResourceSubType = c.ResourceType.ResourceSubType.GetDescription(),
            //       Designation = c.ResourceType.Designation.NameOFDesignation != null ? c.ResourceType.Designation.NameOFDesignation+" - "+ c.ResourceType.Designation.YersOfExperience+ " Years" : " - ",
            //       Role = c.ResourceType.Role.RoleName != null ? c.ResourceType.Role.RoleName+" - " + c.ResourceType.User.UserName : " - ",
            //       NameOfMaterial = c.ResourceType.NameOfMaterial != "" ? c.ResourceType.NameOfMaterial : " - ",
            //       CreatedDate = c.CreatedDate,
            //       CreatedByUser = c.CreatedByUser.UserName,
            //   }).ToList();

            return true;

        }
        public string Duplicate(StudyPlanResource objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StudyPlanTaskId == objSave.StudyPlanTaskId && x.ResourceTypeId== objSave.ResourceTypeId && x.DeletedDate == null))
                return "Duplicate Resource ";
            return "";
        }
        public dynamic ResourceById(int id)
        {
            var gridResource = _context.StudyPlanResource.Include(r => r.ResourceType).ThenInclude(d => d.Designation).Where(x => x.Id == id && x.DeletedDate == null)
               .Select(c => new ResourceByEdit
               {
                   resourceId = (c.ResourceType.ResourceTypes.GetDescription() == "Manpower") ? 1 : 2,
                   subresource = (c.ResourceType.ResourceSubType.GetDescription() == "Permanent") ? 1 : c.ResourceType.ResourceSubType.GetDescription() == "Contract" ? 2: c.ResourceType.ResourceSubType.GetDescription() == "Consumable" ? 3 : 4,
                   designation = c.ResourceType.Designation.Id,
                   nameOfMaterial=c.ResourceType.Id,
                   rollUser =c.ResourceType.Id
               }).FirstOrDefault();

            return gridResource;

        }
    }
}

