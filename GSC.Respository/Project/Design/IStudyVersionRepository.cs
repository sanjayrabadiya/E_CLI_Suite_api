﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;

namespace GSC.Respository.Project.Design
{
    public interface IStudyVersionRepository : IGenericRepository<StudyVersion>
    {
        IList<DropDownDto> GetProjectVersionDropDown();
        List<StudyVersionGridDto> GetStudyVersion(int ProjectDesignId);
        string Duplicate(StudyVersion objSave);
        void UpdateVisitStatus(StudyVersion studyVersion);
        double GetVersionNumber(int ProjectDesignId);
        void ActiveVersion(int Id, int ProjectDesignId);
    }
}