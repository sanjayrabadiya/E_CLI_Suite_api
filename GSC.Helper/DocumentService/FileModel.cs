using System;

namespace GSC.Shared.DocumentService
{
    public class FileModel
    {
        public string Base64 { get; set; }
        public string Extension { get; set; }
    }

    public class EtmfFileNameModel
    {
        public string FileName { get; set; }
        public DateTime FileCreateDate { get; set; }
        public string FileCodeName { get; set; }
    }
}