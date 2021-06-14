using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Volunteer
{
    public class VolunteerDocument : BaseEntity, ICommonAduit
    {
        public int VolunteerId { get; set; }

        public string FileName { get; set; }

        public string PathName { get; set; }

        public byte[] DocumentBinary { get; set; }

        public string MimeType { get; set; }

        public string Note { get; set; }

        public int? DocumentTypeId { get; set; }

        public DocumentType DocumentType { get; set; }

        public int DocumentNameId { get; set; }

        public DocumentName DocumentName { get; set; }
    }
}