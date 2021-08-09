using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.UserMgt
{
    public class AppScreenRepository : GenericRespository<AppScreen>, IAppScreenRepository
    {
        private readonly IGSCContext _context;

        public AppScreenRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
        }


        public List<DropDownDto> GetAppScreenParentFromDropDown()
        {
            return All.Where(x =>
                    x.DeletedDate == null && x.ParentAppScreenId == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ScreenName }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetAppScreenChildParentFromDropDown(int id)
        {
            return All.Where(x =>
                    x.DeletedDate == null && x.ParentAppScreenId == id)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ScreenName }).OrderBy(o => o.Value).ToList();
        }

        public List<DropDownDto> GetTableColunms(int id)
        {
            var appscreen = All.Where(x => x.DeletedDate == null && x.Id == id).ToList();
            if (appscreen != null)
            {
                if(appscreen[0].TableName != null)
                {
                    return _context.TableFieldName.Where(x => x.DeletedDate == null && x.TableName == appscreen[0].TableName).Select(c => new DropDownDto { Id = c.Id, Value = c.LabelName }).OrderBy(o => o.Value).ToList(); 
                }    
            }
            return null;
        }

        public List<DropDownDto> GetMasterTableName()
        {
            return All.Where(x =>
                    x.DeletedDate == null && x.IsMaster == true)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.TableName }).OrderBy(o => o.Value).ToList();
        }
    }
}