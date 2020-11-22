using GSC.Common.Base;

namespace GSC.Data.Entities.UserMgt
{
    public class UserImage : BaseEntity
    {
        public int UserId { get; set; }
        public byte[] BiometricBinary { get; set; }
        public byte[] ThumbData { get; set; }
        public string FilePath { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
    }
}