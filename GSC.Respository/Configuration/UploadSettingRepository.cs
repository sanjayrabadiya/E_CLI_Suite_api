using System.IO;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Configuration
{
    public class UploadSettingRepository : GenericRespository<UploadSetting>, IUploadSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public UploadSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public string GetImagePath()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.ImagePath;
        }

        public string GetDocumentPath()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.DocumentPath;
        }

        public string GetWebImageUrl()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.ImageUrl;
        }

        public string GetWebDocumentUrl()
        {
            return FindBy(x => x.CompanyId == _jwtTokenAccesser.CompanyId && x.DeletedDate == null).FirstOrDefault()
                ?.DocumentUrl;
        }

        public object getWebImageUrl()
        {
            throw new System.NotImplementedException();
        }

        public bool IsUnlimitedUploadlimit()
        {
            var details = All.FirstOrDefault();
            if (details.UploadLimitType == UploadLimitType.Unlimited)
                return true;
            else
                return false;

        }

        public string ValidateUploadlimit(int ProjectId)
        {
            var uploadDetails = All.Where(x => x.DeletedDate == null).FirstOrDefault();
            if (uploadDetails.UploadLimitType == UploadLimitType.StudyBase)
            {
                string projectCode = GetProjectCode(ProjectId);
                string[] paths = { uploadDetails.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), projectCode };
                var fullPath = Path.Combine(paths);


                int uploadlimit = _context.UploadLimit.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null).Select(x => x.Uploadlimit).FirstOrDefault();
                if (uploadlimit == 0)
                    return "Please set upload limit for particular Study";
                double dirsize = GetDirectorySize(fullPath);
                if (dirsize > uploadlimit)
                    return "Upload limit is full Please Contact to Administrator";
            }
            return "";
        }


        private string GetProjectCode(int ProjectId)
        {
            var projectdDetail = _context.Project.Where(x => x.Id == ProjectId).Select(i => new { i.ParentProjectId, i.ProjectCode }).FirstOrDefault();
            if (projectdDetail.ParentProjectId != null)
            {
                return _context.Project.Where(x => x.Id == projectdDetail.ParentProjectId).Select(x => x.ProjectCode).FirstOrDefault();

            }
            return projectdDetail.ProjectCode;
        }

        private static double GetDirectorySize(string folderPath)
        {
            //return in bytes
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                long bytes = di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);

                double mb = (bytes / 1024f) / 1024f;

                return mb / 1024.0;
            }
            return 0;
            // return in gb
        }
    }
}