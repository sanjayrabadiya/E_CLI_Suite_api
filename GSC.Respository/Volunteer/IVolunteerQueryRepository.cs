﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerQueryRepository : IGenericRepository<VolunteerQuery>
    {
        VolunteerQuery GetLatest(int VolunteerId, string FiledName);
        IList<VolunteerQueryDto> GetData(int volunteerid);
        IList<VolunteerQueryDto> VolunteerQuerySearch();
    }
}