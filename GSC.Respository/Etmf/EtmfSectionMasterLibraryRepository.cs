using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
   public class EtmfSectionMasterLibraryRepository : GenericRespository<EtmfSectionMasterLibrary, GscContext>, IEtmfSectionMasterLibraryRepository
    {
        public EtmfSectionMasterLibraryRepository(IUnitOfWork<GscContext> uow,
         IJwtTokenAccesser jwtTokenAccesser)
         : base(uow, jwtTokenAccesser)
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