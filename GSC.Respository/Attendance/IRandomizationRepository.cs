﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using System.Collections.Generic;

namespace GSC.Respository.Attendance
{
    public interface IRandomizationRepository : IGenericRepository<Randomization>
    {
        string Duplicate(Randomization objSave, int projectId);
        List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted);
        void SaveRandomization(Randomization randomization, RandomizationDto randomizationDto);
    }
}