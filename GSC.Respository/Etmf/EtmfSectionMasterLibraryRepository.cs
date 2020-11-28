using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class EtmfSectionMasterLibraryRepository : GenericRespository<EtmfSectionMasterLibrary>, IEtmfSectionMasterLibraryRepository
    {
        public EtmfSectionMasterLibraryRepository(IGSCContext context,
         IJwtTokenAccesser jwtTokenAccesser)
         : base(context)
        {
        }
        public string Duplicate(EtmfSectionMasterLibrary objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.SectionName == objSave.SectionName && x.DeletedDate == null))
                return "Duplicate Section name : " + objSave.SectionName;
            return "";
        }
    }
}