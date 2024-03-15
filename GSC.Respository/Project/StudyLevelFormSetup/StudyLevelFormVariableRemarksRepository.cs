using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableRemarksRepository : GenericRespository<StudyLevelFormVariableRemarks>, IStudyLevelFormVariableRemarksRepository
    {
        public StudyLevelFormVariableRemarksRepository(IGSCContext context) : base(context)
        {
        }
    }
}
