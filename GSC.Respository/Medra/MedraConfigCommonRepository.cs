using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MedraConfigCommonRepository : GenericRespository<MedraConfig>, IMedraConfigCommonRepository
    {
        private readonly IGSCContext _context;
        public MedraConfigCommonRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }

        public void AddDataInMedraTableUsingAsciiFile(int MedraConfigId, string path, FolderType folderType, string Language, string Version, string Rootname)
        {
            string[] paths = { path, folderType.ToString(), Language, Version, Rootname };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii";

            string[] fileEntries = Directory.GetFiles(fullPath);
            foreach (string fileName in fileEntries)
            {
                _context.Save();
            }
        }

        public SummaryDto getSummary(int MedraConfigId)
        {
            SummaryDto count = new SummaryDto();
            count.Soc = _context.MeddraSocTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Hlgt = _context.MeddraHlgtPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Hlt = _context.MeddraHltPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Pt = _context.MeddraPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Llt = _context.MeddraLowLevelTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            return count;
        }

        public void DeleteDirectory(string root)
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }

    }
}
