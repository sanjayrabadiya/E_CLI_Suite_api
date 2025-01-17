﻿using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraHlgtHltCompRepository : GenericRespository<MeddraHlgtHltComp>, IMeddraHlgtHltCompRepository
    {
        public MeddraHlgtHltCompRepository(IGSCContext context) : base(context)
        {

        }

        public int AddHlgtHltFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\hlgt_hlt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraHlgtHltComp
                    {
                        MedraConfigId = obj.MedraId,
                        hlgt_code = Convert.ToInt64(allField[0]),
                        hlt_code = Convert.ToInt64(allField[1])
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
