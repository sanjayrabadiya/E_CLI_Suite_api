using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Master
{
    public class PatientStatusRepository : GenericRespository<PatientStatus, GscContext>, IPatientStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public PatientStatusRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetPatientStatusDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StatusName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(PatientStatus objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StatusName == objSave.StatusName && x.DeletedDate == null))
                return "Duplicate Status name : " + objSave.StatusName;
            return "";
        }

        public List<PatientStatusDto> GetPatientStatusList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<PatientStatusDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
