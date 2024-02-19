using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;

namespace GSC.Respository.CTMS
{
   public  class PassThroughCostActivityRepository : GenericRespository<PassThroughCostActivity>, IPassThroughCostActivityRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public PassThroughCostActivityRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context=context;
        }
        public string Duplicate(PassThroughCostActivity objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ActivityName == objSave.ActivityName.Trim() && x.DeletedDate == null))
                return "Duplicate Pass Through Cost Activity : " + objSave.ActivityName;

            return "";
        }
        public List<PassThroughCostActivityGridDto> GetPassThroughCostActivityList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                  ProjectTo<PassThroughCostActivityGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }
        public List<DropDownStudyDto> GetPassThroughCostActivityDropDown()
        {
            return All.Where(x => x.DeletedDate == null)
                .Select(c => new DropDownStudyDto
                {
                    Id = (short)c.Id,
                    Value = c.ActivityName,
                }).ToList();
        }
    }
}
