using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
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

        public string Duplicate(StudyVersion objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.VersionNumber == objSave.VersionNumber && x.DeletedDate == null))
                return "Duplicate version number: " + objSave.VersionNumber;

            return "";
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

        public double GetVersionNumber(int ProjectDesignId)
        {
            var number = All.Where(x => x.DeletedDate == null && x.ProjectDesignId == ProjectDesignId).Select(c => new { Id = c.Id, number = c.VersionNumber }).OrderByDescending(x => x.Id).FirstOrDefault();
            return number.number + 0.1;
        }

        public void ActiveVersion(int Id,int ProjectDesignId)
        {
            var version = All.Where(x => x.ProjectDesignId == ProjectDesignId && x.DeletedDate == null && x.IsRunning).FirstOrDefault();
            if (version != null)
            {
                version.IsRunning = false;
                Update(version);
            }

            var active = All.Where(x => x.Id == Id).FirstOrDefault();
            active.IsRunning = true;
            Update(active);
            _context.Save();
        }
    }
}
