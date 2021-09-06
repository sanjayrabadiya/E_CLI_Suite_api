using GSC.Common.GenericRespository;
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
        decimal GetVersionNumber(int projectId, bool isMonir);
        List<DropDownDto> GetVersionDropDown(int projectId);
        bool IsOnTrialByProjectDesing(int projectDesignId);
        void UpdateGoLive(StudyGoLiveDto studyGoLiveDto, StudyVersion studyVersion);
        double GetStudyVersionForLive(int projectId);
        bool AnyLive(int projectDesignId);
        double GetOnTrialVersionByProjectDesign(int projectDesignId);
    }
}