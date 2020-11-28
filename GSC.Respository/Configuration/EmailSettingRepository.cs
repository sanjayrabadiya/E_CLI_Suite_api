using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Configuration
{
    public class EmailSettingRepository : GenericRespository<EmailSetting>, IEmailSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public EmailSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public List<DropDownDto> GetEmailFromDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.EmailFrom}).OrderBy(o => o.Value).ToList();
        }
    }
}