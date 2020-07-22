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
        public HolidayRepository(IUnitOfWork<GscContext> uow,
IJwtTokenAccesser jwtTokenAccesser)
: base(uow, jwtTokenAccesser)
        {
        }


        public IList<HolidayDto> GetHolidayList(int InvestigatorContactId)
        {
            return FindByInclude(t => t.InvestigatorContactId == InvestigatorContactId && t.DeletedDate == null).Select(c =>
                new HolidayDto
                {
                    Id = c.Id,
                    InvestigatorContactId = c.InvestigatorContactId,
                    HolidayType = c.HolidayType,
                    HolidayTypeName = ((HolidayType)c.HolidayType).GetDescription(),
                    HolidayName = c.HolidayName,
                    HolidayDate = c.HolidayDate,
                    Description = c.Description,
                    CompanyId = c.CompanyId
                }).OrderByDescending(t => t.Id).ToList();
        }

        public string DuplicateHoliday(Holiday objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.HolidayName == objSave.HolidayName && x.HolidayDate == objSave.HolidayDate && x.DeletedDate == null))
                return "Duplicate Holiday : " + objSave.HolidayName;

            return "";
        }
    }
}
