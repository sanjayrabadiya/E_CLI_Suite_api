﻿using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Domain.Context;
using AutoMapper;

namespace GSC.Respository.Volunteer
{
    public class VolunteerFingerRepository : GenericRespository<Data.Entities.Volunteer.VolunteerFinger>,
        IVolunteerFingerRepository
    {
        
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;

        public VolunteerFingerRepository(IGSCContext context,
            IMapper mapper
        )
            : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public List<DbRecords> GetFingers()
        {
            var finger = FindByInclude(t => t.DeletedDate == null, t => t.Volunteer).Select(x => new DbRecords
            {
                m_Id = x.Volunteer.Id,
                m_UserName = x.Volunteer.VolunteerNo + " " + x.Volunteer.FirstName + " " + x.Volunteer.MiddleName + " " + x.Volunteer.LastName,
                m_Template = x.FingerImage,
                UserBlock = x.Volunteer.IsBlocked == true ? true : false,
                UserInActive = x.Volunteer.DeletedDate != null,
            }).ToList();

            return finger;
        }
    }
}