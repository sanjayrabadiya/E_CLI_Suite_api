using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Project.Design
{
    public class StudyVersionRepository : GenericRespository<StudyVersion>, IStudyVersionRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IStudyVersionVisitStatusRepository _studyVersionVisitStatusRepository;
        public StudyVersionRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IStudyVersionVisitStatusRepository studyVersionVisitStatusRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            _studyVersionVisitStatusRepository = studyVersionVisitStatusRepository;
        }

        public List<StudyVersionGridDto> GetStudyVersion(int ProjectDesignId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignId == ProjectDesignId).
                   ProjectTo<StudyVersionGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }


        public bool IsOnTrialByProjectDesing(int projectDesignId)
        {
            return All.Any(x => x.DeletedDate == null && x.ProjectDesignId == projectDesignId && x.VersionStatus == Helper.VersionStatus.OnTrial);

        }

        public string Duplicate(StudyVersion objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.VersionNumber == objSave.VersionNumber && x.ProjectDesignId == objSave.ProjectDesignId && x.DeletedDate == null))
                return "Duplicate version number: " + objSave.VersionNumber;

            return "";
        }

        private void SetGoLive(int projectDesignId, string note)
        {
            var versions = All.Where(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null).ToList();
            versions.ForEach(x =>
            {
                if (x.VersionStatus == Helper.VersionStatus.OnTrial)
                {
                    x.VersionStatus = Helper.VersionStatus.GoLive;
                    x.GoLiveBy = _jwtTokenAccesser.UserId;
                    x.GoLiveOn = _jwtTokenAccesser.GetClientDate();
                    x.GoLiveNote = note;
                    Update(x);
                }
                else if (x.VersionStatus == Helper.VersionStatus.GoLive)
                {
                    x.VersionStatus = Helper.VersionStatus.Archive;
                    Update(x);
                }

            });
        }

        public IList<DropDownDto> GetProjectVersionDropDown()
        {
            return All.Where(x => x.DeletedDate == null).Select(c => new DropDownDto
            {
                Id = c.Id,
                Value = c.VersionNumber.ToString()
            }).OrderBy(o => o.Value).ToList();
        }

        public void UpdateGoLive(StudyGoLiveDto studyGoLiveDto, StudyVersion studyVersion)
        {

            if (studyVersion == null) return;

            if (studyGoLiveDto.IsOnTrial)
            {
                studyVersion.TestNote = studyGoLiveDto.GoLiveNote;
                studyVersion.IsTestSiteVerified = true;
                Update(studyVersion);
            }
            else
            {


                SetGoLive(studyGoLiveDto.ProjectDesignId, studyGoLiveDto.GoLiveNote);
            }

            var studyVerionVisit = _studyVersionVisitStatusRepository.All.Where(x => x.StudyVerionId == studyVersion.Id && x.DeletedDate == null).ToList();
            studyVerionVisit.ForEach(x =>
            {
                _studyVersionVisitStatusRepository.Remove(x);
            });

            if (studyGoLiveDto.VisitStatusId != null)
            {
                studyGoLiveDto.VisitStatusId.ToList().ForEach(x =>
                {
                    var studyVersionVisitStatus = new StudyVerionVisitStatus();
                    studyVersionVisitStatus.VisitStatusId = x;
                    studyVersionVisitStatus.StudyVerionId = studyVersion.Id;
                    _studyVersionVisitStatusRepository.Add(studyVersionVisitStatus);
                });
            }
        }

        public decimal GetVersionNumber(int projectId, bool isMonir)
        {
            var number = All.Count(x => x.DeletedDate == null && x.ProjectId == projectId && x.IsMinor == isMonir);
            if (number == 0) return 1;
            var parentNumber = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.IsMinor == isMonir).Select(t => t.ProjectDesignId).Distinct().Count();
            return Convert.ToDecimal(parentNumber + "." + number);
        }


        public List<DropDownDto> GetVersionDropDown(int projectId)
        {
            var result = All.Where(x => x.ProjectId == projectId)
                .Select(c => new DropDownDto
                {
                    Id = c.Id,
                    Value = c.VersionNumber.ToString(),
                    IsDeleted = c.DeletedDate != null
                }).OrderBy(o => o.Value).ToList();


            return result;
        }

        public double GetStudyVersionForLive(int projectId)
        {
            return All.Where(x => x.ProjectId == projectId && x.VersionStatus == Helper.VersionStatus.GoLive && x.DeletedDate == null).
                Select(t => t.VersionNumber).OrderByDescending(c => c).FirstOrDefault();
        }

        public double GetOnTrialVersionByProjectDesign(int projectDesignId)
        {
            return All.Where(x => x.ProjectDesignId == projectDesignId && x.VersionStatus == Helper.VersionStatus.OnTrial && x.DeletedDate == null).
                Select(t => t.VersionNumber).OrderByDescending(c => c).FirstOrDefault();
        }


        public bool AnyLive(int projectDesignId)
        {
            return All.Any(x => x.ProjectDesignId == projectDesignId && x.DeletedDate == null && x.VersionStatus == VersionStatus.GoLive);
        }
    }
}
