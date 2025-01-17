﻿using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceSectionRepository: GenericRespository<EtmfProjectWorkPlace>, IProjectWorkplaceSectionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectWorkplaceSectionRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetProjectWorkPlaceSectionDropDown(int zoneId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.EtmfProjectWorkPlaceId == zoneId)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.EtmfMasterLibrary.SectionName, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value).ToList();
        }
    }
}
