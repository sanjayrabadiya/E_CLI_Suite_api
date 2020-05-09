using GSC.Data.Dto.Medra;
using GSC.Helper;

namespace GSC.Respository.Medra
{
    public interface IMedraConfigCommonRepository
    {
        void AddDataInMedraTableUsingAsciiFile(int MedraConfigId, string path, FolderType folderType, string Language, string Version, string Rootname);
        SummaryDto getSummary(int MedraConfigId);
        void DeleteDirectory(string root);
    }
}