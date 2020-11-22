using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Attendance;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Screening;
using GSC.Shared;

namespace GSC.Respository.Attendance
{
    public class ProjectSubjectRepository : GenericRespository<ProjectSubject, GscContext>, IProjectSubjectRepository
    {
        private readonly IAttendanceHistoryRepository _attendanceHistoryRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        public ProjectSubjectRepository(IUnitOfWork<GscContext> uow,
            INumberFormatRepository numberFormatRepository,
            IProjectRepository projectRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository)
            : base(uow, jwtTokenAccesser)
        {
            _numberFormatRepository = numberFormatRepository;
            _projectRepository = projectRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _attendanceRepository = attendanceRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _uow = uow;
        }

        public void SaveSubjectForVolunteer(int attendanceId, int screeningTemplateId)
        {
            var attendance = _attendanceRepository.Find(attendanceId);

            if (attendance.ProjectSubjectId.HasValue) return;

            if (_screeningTemplateValueRepository.IsFitness(screeningTemplateId))
            {
                var numberType = attendance.IsStandby ? SubjectNumberType.StandBy : SubjectNumberType.Normal;

                var projectSubject = FindBy(x => x.DeletedDate == null && x.ProjectId == attendance.ProjectId
                                                                       && x.NumberType == numberType &&
                                                                       x.VolunteerId == null).OrderBy(t => t.Id)
                    .FirstOrDefault();
                if (projectSubject == null)
                {
                    projectSubject = SaveSubjectForProject(attendance.ProjectId, numberType);
                    _attendanceHistoryRepository.SaveHistory("Fitness passed", attendance.Id, null);
                }
                else
                {
                    var replaceProject = FindBy(x => x.RefProjectSubjectId == projectSubject.Id).FirstOrDefault();
                    if (replaceProject != null)
                    {
                        replaceProject.IsRepaced = true;
                        Update(replaceProject);
                        var volunteerName = Context.Volunteer.Find(replaceProject.VolunteerId ?? 0).FullName;
                        _attendanceHistoryRepository.SaveHistory("Fitness passed with auto replace " + volunteerName,
                            attendance.Id, null);
                    }
                    else
                    {
                        _attendanceHistoryRepository.SaveHistory("Fitness passed", attendance.Id, null);
                    }

                    Update(projectSubject);
                }

                projectSubject.VolunteerId = attendance.VolunteerId;
                attendance.ProjectSubject = projectSubject;
                attendance.Status = AttendaceStatus.FitnessPass;
            }
            else
            {
                _attendanceHistoryRepository.SaveHistory("Fitness Failed", attendance.Id, null);
                attendance.Status = AttendaceStatus.FitnessFailed;
            }

            _attendanceRepository.Update(attendance);
        }

        public ProjectSubject SaveSubjectForProject(int projectId, SubjectNumberType numberType)
        {
            var parentProjectId = _projectRepository.Find(projectId).ParentProjectId ?? projectId;
            var projectSubject = new ProjectSubject
            {
                ParentProjectId = parentProjectId,
                ProjectId = projectId,
                NumberType = numberType
            };

            projectSubject.Number = GetSubjectNumer(projectId, parentProjectId, numberType);
            Add(projectSubject);
            _uow.Save();

            return projectSubject;
        }

        public void DiscontinueProjectSubject(int attendanceId, int screeningTemplateId)
        {
            var attendance = _attendanceRepository.Find(attendanceId);

            if (_screeningTemplateValueRepository.IsDiscontinued(screeningTemplateId))
            {
                attendance.ProjectSubject = GetReplaceSubject(attendance);
                attendance.ProjectSubjectId = null;
                attendance.Status = AttendaceStatus.Replaced;
                _attendanceHistoryRepository.SaveHistory("Replaced", attendance.Id, null);
            }
            else
            {
                attendance.Status = AttendaceStatus.Discounted;
                _attendanceHistoryRepository.SaveHistory("Discounted", attendance.Id, null);
            }

            _attendanceRepository.Update(attendance);
        }


        public void ReplaceSubjectNumber(int currentProjectSubjectId, int attendanceId)
        {
            var projectSubject = Find(currentProjectSubjectId);
            projectSubject.IsRepaced = true;
            Update(projectSubject);

            var attendance = _attendanceRepository.Find(attendanceId);

            var oldSubject = Find((int) attendance.ProjectSubjectId);
            oldSubject.VolunteerId = null;
            Update(oldSubject);

            var replaceSubject = Find((int) projectSubject.RefProjectSubjectId);
            replaceSubject.VolunteerId = attendance.VolunteerId;
            attendance.ProjectSubjectId = replaceSubject.Id;
            Update(replaceSubject);
            var volunteerName = Context.Volunteer.Find(replaceSubject.VolunteerId ?? 0).FullName;
            _attendanceHistoryRepository.SaveHistory("Replaced with " + volunteerName, attendance.Id, null);
            _attendanceRepository.Update(attendance);
        }

        private string GetSubjectNumer(int projectId, int parentProjectId, SubjectNumberType numberType)
        {
            var project = _projectRepository.Find(projectId);
            var underTesting = Context.ProjectDesign.Any(x =>
                x.DeletedDate == null && x.ProjectId == parentProjectId && x.IsUnderTesting);
            var keyName = underTesting ? "Testing" : "";
            keyName += numberType == SubjectNumberType.StandBy ? "ExtraSub" : "Sub";
            var number = All.Count(x => x.DeletedDate == null &&
                                        x.ProjectId == parentProjectId && x.NumberType == numberType &&
                                        x.IsTesting == underTesting);
            var subjectNumber = _numberFormatRepository.GetNumberFormat(keyName, number);

            subjectNumber = subjectNumber.Replace("PRO", project.ProjectCode);

            return subjectNumber.ToUpper();
        }

        public ProjectSubject GetReplaceSubject(Data.Entities.Attendance.Attendance attendance)
        {
            var projectSubject = Find(attendance.ProjectSubjectId ?? 0);
            projectSubject.VolunteerId = null;
            Update(projectSubject);

            var replaceSubject = new ProjectSubject
            {
                ParentProjectId = projectSubject.ParentProjectId,
                ProjectId = projectSubject.ProjectId,
                Number = "*R-" + projectSubject.Number,
                NumberType = SubjectNumberType.Replaced,
                VolunteerId = attendance.VolunteerId,
                RefProjectSubjectId = projectSubject.Id
            };
            Add(replaceSubject);
            return replaceSubject;
        }
    }
}