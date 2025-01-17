﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.LabManagement;
using GSC.Data.Entities.LabManagement;
using System.Collections.Generic;

namespace GSC.Respository.LabManagement
{
    public interface ILabManagementUploadDataRepository : IGenericRepository<LabManagementUploadData>
    {
        List<LabManagementUploadDataGridDto> GetUploadDataList(int projectId, bool isDeleted);
        string InsertExcelDataIntoDatabaseTable(LabManagementUploadData labManagementUploadData,string SiteCode);
        // Insert data into data entry screening template, screening template value and screening template audit
        void InsertDataIntoDataEntry(LabManagementUploadData labManagementUpload);
        string CheckDataIsUploadForDeleteConfiguration(int Id);
        string CheckDataIsUploadForRemapping(int Id);
    }
}