using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.UserMgt
{
    public class AppScreenRepository : GenericRespository<AppScreen, GscContext>, IAppScreenRepository
    {
        public AppScreenRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }


        public List<DropDownDto> GetAppScreenParentFromDropDown()
        {
            return All.Where(x =>
                    x.DeletedDate == null && x.ParentAppScreenId == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ScreenName}).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetMasterTableName()
        {
            return All.Where(x =>
                    x.DeletedDate == null && x.IsMaster == true)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TableName}).OrderBy(o => o.Value).ToList();
        }
    }
}