using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class DesignationRepository : GenericRespository<Designation>, IDesignationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public DesignationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        List<DesignationGridDto> IDesignationRepository.GetDesignationList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<DesignationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public string Duplicate(Designation objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.DesignationCod == objSave.DesignationCod.Trim() && x.DeletedDate == null))
                return "Duplicate Letter Name : " + objSave.DesignationCod;
            return "";
        }
    }
}