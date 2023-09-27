using System.ComponentModel;

namespace GSC.Shared.Generic
{
    public enum UserMasterUserType : short
    {
        [Description("SuperAdmin")] SuperAdmin = 1,
        [Description("Administrator")] Administrator = 2,
        [Description("Patient")] Patient = 3,
        [Description("User")] User = 4,
        [Description("LAR")] LAR = 5
    }

    public enum AuditAction : short
    {
        [Description("Inserted")] Inserted = 1,
        [Description("Updated")] Updated = 2,
        [Description("Deleted")] Deleted = 3,
        [Description("Activated")] Activated = 4,
        [Description("RePrint")] RePrint = 5
    }

    public enum PrefLanguage : short
    {
        [Description("English")] en = 1,
        [Description("Germany")] ge = 2
    }

    public enum EtmfTableNameTag : int
    {
        [Description("ProjectWorkPlace")] ProjectWorkPlace = 1,
        [Description("ProjectWorkPlaceDetail")] ProjectWorkPlaceDetail = 2,
        [Description("ProjectWorkPlaceZone")] ProjectWorkPlaceZone = 3,
        [Description("ProjectWorkPlaceSection")] ProjectWorkPlaceSection = 4,
        [Description("ProjectWorkPlaceArtificate")] ProjectWorkPlaceArtificate = 5,
        [Description("ProjectWorkPlaceSubSection")] ProjectWorkPlaceSubSection = 6,
        [Description("ProjectWorkPlaceSubSectionArtifact")] ProjectWorkPlaceSubSectionArtifact = 7
    }


    public enum DocumentPickFromType : int
    {
        [Description("File,Gallery,Camera")] FileGalleryCamera = 1,
        [Description("Only File")] OnlyFile = 2,
        [Description("Only Camera")] OnlyCamera = 3,
        [Description("Only Gallery")] OnlyGallery = 4,
        [Description("File and Camera")] FileAndCamera = 5,
        [Description("File and Gallery")] FileAandGallery = 6,
        [Description("Gallery and Camera")] GalleryAandCamera = 7
        
    }

}
