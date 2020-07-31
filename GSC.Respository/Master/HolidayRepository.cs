using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class HolidayRepository : GenericRespository<Holiday, GscContext>, IHolidayRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public HolidayRepository(IUnitOfWork<GscContext> uow,
IJwtTokenAccesser jwtTokenAccesser)
: base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public IList<HolidayDto> GetHolidayList(int InvestigatorContactId, bool isDeleted)
        {
            return FindByInclude(t => t.InvestigatorContactId == InvestigatorContactId && (isDeleted ? t.DeletedDate != null : t.DeletedDate == null)).Select(c =>
                new HolidayDto
                {
                    Id = c.Id,
                    InvestigatorContactId = c.InvestigatorContactId,
                    HolidayType = c.HolidayType,
                    HolidayTypeName = ((HolidayType)c.HolidayType).GetDescription(),
                    HolidayName = c.HolidayName,
                    HolidayDate = c.HolidayDate,
                    Description = c.Description,
                    CompanyId = c.CompanyId,
                    IsDeleted = c.DeletedDate != null
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string DuplicateHoliday(Holiday objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.HolidayName == objSave.HolidayName && x.HolidayDate == objSave.HolidayDate && x.DeletedDate == null))
                return "Duplicate Holiday : " + objSave.HolidayName;

            return "";
        }

        public List<DropDownDto> GetHolidayDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.HolidayName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
