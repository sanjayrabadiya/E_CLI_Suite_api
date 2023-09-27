using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
   public  class ResourceTypeRepository: GenericRespository<ResourceType>, IResourceTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ResourceTypeRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context=context;
        }

        //public List<DropDownDto> GetResourceTypeDropDown()
        //{
        //    return All.Select(c => new DropDownDto { Id = c.Id, Value = c.ResourceName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        //}

        public string Duplicate(ResourceType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ResourceCode == objSave.ResourceCode.Trim() && x.DeletedDate == null))
                return "Duplicate Resource Code : " + objSave.ResourceCode;

            return "";
        }
        public List<ResourceTypeGridDto> GetResourceTypeList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ResourceTypeGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }
        public List<DropDownDto> GetUnitTypeDropDown()
        {
            return _context.Unit.Where(x => x.DeletedBy == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.UnitName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetDesignationDropDown()
        {
            return _context.Designation.Where(x => x.DeletedBy == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.NameOFDesignation+" - "+ c.YersOfExperience + " years of Experience", IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetDesignationDropDown(int resourceTypeID, int resourceSubTypeID)
        { 
                return _context.ResourceType.Include(s => s.Designation).Where(x => x.DeletedBy == null && ((int)x.ResourceTypes) == resourceTypeID && ((int)x.ResourceSubType) == resourceSubTypeID)
                                .Select(c => new DropDownDto { Id = c.Designation.Id , Value = c.Designation.NameOFDesignation + " - " + c.Designation.YersOfExperience + " years of Experience" , IsDeleted = c.DeletedDate != null })
                                .Distinct()
                                .OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetNameOfMaterialDropDown(int resourceTypeID, int resourceSubTypeID)
        {
            return _context.ResourceType.Include(s => s.Designation).Where(x => x.DeletedBy == null && ((int)x.ResourceTypes) == resourceTypeID && ((int)x.ResourceSubType) == resourceSubTypeID)
                            .Select(c => new DropDownDto { Id =  c.Id, Value =  c.NameOfMaterial, IsDeleted = c.DeletedDate != null })
                            .OrderBy(o => o.Value).ToList();
        }
        public List<DropDownDto> GetRollUserDropDown(int designationID)
        {
            return _context.ResourceType.Include(s => s.Designation).Where(x => x.DeletedBy == null && x.DesignationId == designationID)
                            .Select(c => new DropDownDto { Id = c.Id, Value =  c.Role.RoleName +" - "+ c.User.UserName, IsDeleted = c.DeletedDate != null })
                            .OrderBy(o => o.Value).ToList();
        }

    }
}
