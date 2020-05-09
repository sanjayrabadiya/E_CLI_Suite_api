using GSC.Common.GenericRespository;
using GSC.Data.Entities.Attendance;
using GSC.Helper;

namespace GSC.Respository.Attendance
{
    public interface IProjectSubjectRepository : IGenericRepository<ProjectSubject>
    {
        void SaveSubjectForVolunteer(int attendanceId, int screeningTemplateId);
        ProjectSubject SaveSubjectForProject(int projectId, SubjectNumberType numberType);
        void DiscontinueProjectSubject(int attendanceId, int screeningTemplateId);
        void ReplaceSubjectNumber(int currentProjectSubjectId, int attendanceId);
    }
}