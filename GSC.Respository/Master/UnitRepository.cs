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
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class UnitRepository : GenericRespository<Unit>, IUnitRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public UnitRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<DropDownDto> GetUnitDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.UnitName,IsDeleted=c.DeletedDate!=null}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Unit objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.UnitName == objSave.UnitName.Trim() && x.DeletedDate == null))
                return "Duplicate Unit name : " + objSave.UnitName;
            return "";
        }

        public List<UnitGridDto> GetUnitList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<UnitGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
        public List<DropDownDto> GetUnitAsModule(string screenCode)
        { 
           return _context.Unit.Include(s=>s.AppScreen).Where(x =>(x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)&& x.AppScreen.ScreenCode == screenCode && x.DeletedBy==null)
                  .Select(c => new DropDownDto { Id = c.Id, Value = c.UnitName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}