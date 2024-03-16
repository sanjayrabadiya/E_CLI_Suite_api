using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GSC.Data.Dto.Medra;
using System.IO;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Medra
{
    public class MeddraHltPrefCompRepository : GenericRespository<MeddraHltPrefComp>, IMeddraHltPrefCompRepository
    {
        public MeddraHltPrefCompRepository(IGSCContext context) : base(context)
        {
        }

        public int AddHltPtFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\hlt_pt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraHltPrefComp
                    {
                        MedraConfigId = obj.MedraId,
                        hlt_code = Convert.ToInt64(allField[0]),
                        pt_code = Convert.ToInt64(allField[1])
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
