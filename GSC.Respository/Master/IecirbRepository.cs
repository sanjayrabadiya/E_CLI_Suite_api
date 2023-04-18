using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class IecirbRepository : GenericRespository<Iecirb>, IIecirbRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public IecirbRepository(IGSCContext context,
        IJwtTokenAccesser jwtTokenAccesser)
        : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public string Duplicate(Iecirb objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ManageSiteId == objSave.ManageSiteId && x.RegistrationNumber == objSave.RegistrationNumber.Trim() && x.DeletedDate == null))
                return "Duplicate registration number : " + objSave.RegistrationNumber;

            return "";
        }

        public List<DropDownDto> GetIecirbDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId))
                .Select(c => new DropDownDto { Id = c.Id, Value = c.IECIRBName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }

        public void AddSiteAddress(IecirbDto iecirbDto)
        {
            foreach (var item in iecirbDto.manageSiteAddressId)
            {
                var siteAddress = new IecirbSiteAddress()
                {
                    IecirbId = iecirbDto.Id,
                    ManageSiteAddressId = item
                };

                _context.IecirbSiteAddress.Add(siteAddress);
            }

            _context.Save();
        }

        public void UpdateSiteAddress(IecirbDto iecirbDto)
        {
            var siterole = _context.IecirbSiteAddress.Where(x => x.IecirbId == iecirbDto.Id
                                                               && iecirbDto.manageSiteAddressId.Contains(x.ManageSiteAddressId)
                                                               && x.DeletedDate == null).ToList();

            iecirbDto.manageSiteAddressId.ForEach(z =>
            {
                var role = siterole.Where(x => x.IecirbId == iecirbDto.Id && x.ManageSiteAddressId == z).FirstOrDefault();
                if (role == null)
                {
                    var siteAddress = new IecirbSiteAddress()
                    {
                        IecirbId = iecirbDto.Id,
                        ManageSiteAddressId = z
                    };
                    _context.IecirbSiteAddress.Add(siteAddress);
                }
            });

            var managesiteRole = _context.IecirbSiteAddress.Where(x => x.IecirbId == iecirbDto.Id && x.DeletedDate == null)
                .ToList();

            managesiteRole.ForEach(t =>
            {
                var role = siterole.Where(x => x.IecirbId == t.IecirbId && x.ManageSiteAddressId == t.ManageSiteAddressId).FirstOrDefault();
                if (role == null)
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.IecirbSiteAddress.Update(t);
                }
            });

            _context.Save();
        }
    }
}
