﻿using GSC.Common.GenericRespository;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IProjectWorkplaceArtificatedocumentRepository : IGenericRepository<ProjectWorkplaceArtificatedocument>
    {
        int deleteFile(int id);

        void UpdateApproveDocument(int documentId, bool IsAccepted);
    }
}
