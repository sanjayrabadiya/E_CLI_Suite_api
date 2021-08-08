using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
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
        public StudyVersionRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
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

        public void UpdateGoLive(int projectId)
        {
            var versions = All.Where(x => x.ProjectId == projectId && x.DeletedDate == null).ToList();
            versions.ForEach(x =>
            {
                if (x.VersionStatus == Helper.VersionStatus.OnTrial)
                {
                    x.VersionStatus = Helper.VersionStatus.GoLive;
                    x.GoLiveBy = _jwtTokenAccesser.UserId;
                    x.GoLiveOn = _jwtTokenAccesser.GetClientDate();
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

        public void UpdateVisitStatus(StudyVersion studyVersion)
        {
            //var studyVerionVisitStatus = _context.StudyVerionVisitStatus.Where(x => x.StudyVerionId == studyVersion.Id
            //                                                   && studyVersion.StudyVersionVisitStatus.Select(x => x.VisitStatusId).Contains(x.VisitStatusId)
            //                                                   && x.DeletedDate == null).ToList();

            //studyVersion.StudyVersionVisitStatus.ForEach(z =>
            //{
            //    var status = studyVerionVisitStatus.Where(x => x.StudyVerionId == z.StudyVerionId && x.VisitStatusId == z.VisitStatusId).FirstOrDefault();
            //    if (status == null)
            //    {
            //        _context.StudyVerionVisitStatus.Add(z);
            //    }
            //});

            //var studyVerionVisit = _context.StudyVerionVisitStatus.Where(x => x.StudyVerionId == studyVersion.Id && x.DeletedDate == null)
            //    .ToList();

            //studyVerionVisit.ForEach(t =>
            //{
            //    var visit = studyVerionVisitStatus.Where(x => x.StudyVerionId == t.StudyVerionId && x.VisitStatusId == t.VisitStatusId).FirstOrDefault();
            //    if (visit == null)
            //    {
            //        //delete
            //        t.DeletedBy = _jwtTokenAccesser.UserId;
            //        t.DeletedDate = _jwtTokenAccesser.GetClientDate();
            //        _context.StudyVerionVisitStatus.Update(t);
            //    }
            //});
        }

        public double GetVersionNumber(int projectId, bool isMonir)
        {
            var number = All.Count(x => x.DeletedDate == null && x.ProjectId == projectId && x.IsMinor == isMonir);
            if (number == 0) return 1;
            var parentNumber = All.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.IsMinor == isMonir).Select(t => t.ProjectDesignId).Distinct().Count();
            return Convert.ToDouble(parentNumber + "." + number);
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

        public int GetStudyVersionIdForLive(int projectId)
        {
            return All.Where(x => x.ProjectId == projectId && x.VersionStatus == Helper.VersionStatus.GoLive && x.DeletedDate == null).
                Select(t => t.Id).OrderByDescending(c => c).FirstOrDefault();
        }
    }
}
