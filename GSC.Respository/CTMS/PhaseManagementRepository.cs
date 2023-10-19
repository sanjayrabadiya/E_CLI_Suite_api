using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class PhaseManagementRepository : GenericRespository<PhaseManagement>, IPhaseManagementRepository
    {
        private readonly IMapper _mapper;

        public PhaseManagementRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
        }

        public List<DropDownDto> GetPhaseManagementDropDown()
        {
            return All.Select(c => new DropDownDto { Id = c.Id, Value = c.PhaseName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(PhaseManagement objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.PhaseCode == objSave.PhaseCode.Trim() && x.DeletedDate == null))
                return "Duplicate Phase Code : " + objSave.PhaseCode;

            if (All.Any(x => x.Id != objSave.Id && x.PhaseName == objSave.PhaseName.Trim() && x.DeletedDate == null))
                return "Duplicate Phase Name : " + objSave.PhaseName;

            return "";
        }

        public List<PhaseManagementGridDto> GetPhaseManagementList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<PhaseManagementGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }
    }
}
