using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.UserMgt
{
    public class UserImage : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public byte[] BiometricBinary { get; set; }
        public byte[] ThumbData { get; set; }
        public string FilePath { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
    }
}