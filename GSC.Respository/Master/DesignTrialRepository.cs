using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class DesignTrialRepository : GenericRespository<DesignTrial>, IDesignTrialRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public DesignTrialRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetDesignTrialDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DesignTrialName}).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetDesignTrialDropDownByTrialType(int id)
        {
            return All.Where(x => x.TrialTypeId == id)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.DesignTrialName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(DesignTrial objSave)
        {
            //if (All.Any(
            //    x => x.Id != objSave.Id && x.DesignTrialCode == objSave.DesignTrialCode.Trim() && x.DeletedDate == null))
            //    return "Duplicate DesignTrial code : " + objSave.DesignTrialCode;

            if (All.Any(
                x => x.Id != objSave.Id && x.DesignTrialName == objSave.DesignTrialName.Trim() && x.DeletedDate == null))
                return "Duplicate DesignTrial name : " + objSave.DesignTrialName;
            return "";
        }

        public List<DesignTrialGridDto> GetDesignTrialList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<DesignTrialGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}