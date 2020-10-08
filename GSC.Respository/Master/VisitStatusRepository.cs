using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class VisitStatusRepository : GenericRespository<VisitStatus, GscContext>, IVisitStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VisitStatusRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetVisitStatusDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.StatusName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(VisitStatus objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StatusName == objSave.StatusName && x.DeletedDate == null))
                return "Duplicate Status name : " + objSave.StatusName;
            return "";
        }

        public List<VisitStatusGridDto> GetVisitStatusList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<VisitStatusGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<DropDownDto> GetAutoVisitStatusDropDown()
        {
            return All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.IsAuto)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.DisplayName }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetManualVisitStatusDropDown()
        {
            int[] id = { 6, 8 };
            return All.Where(x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && id.Contains(x.Id))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.DisplayName }).OrderBy(o => o.Value).ToList();
        }
    }
}
