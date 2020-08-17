using System.ComponentModel;

namespace GSC.Helper
{
    public enum Gender : short
    {
        [Description("Male")] Male = 1,
        [Description("Female")] Female = 2,
        [Description("Transgender")] Transgender = 3
    }

    public enum EditType : short
    {
        [Description("Feating Value")] FeatingValue = 1,
        [Description("Formula")] Formula = 2,
        [Description("Depend")] Depend = 3,
        [Description("Programming")] Programming = 4
    }

    public enum UserType : short
    {
        [Description("Active session")] ActiveSession = 1,
        [Description("Inactive users")] InactiveUsers = 2,
        [Description("Active users")] ActiveUsers = 3,
        [Description("Login/Logout users")] LoginLogoutUsers = 4,
    }

    public enum RegulatoryType : short
    {
        [Description("IEC/IRB")] Iecirb = 1,
        [Description("T-License")] License = 2,
        [Description("NOC")] Noc = 3,
        [Description("Others")] Others = 4
    }

    public enum FreezerType : short
    {
        [Description("Freezer")] Freezer = 1,
        [Description("Room")] Room = 2
    }

    public enum CoreVariableType : short
    {
        [Description("Required")] Required = 1,
        [Description("Permissible")] Permissible = 2,
        [Description("Expected")] Expected = 3
    }

    public enum RoleVariableType : short
    {
        [Description("Identifier Variable")] IdentifierVariable = 1,
        [Description("Topic Variable")] TopicVariable = 2,
        [Description("Timing Variable")] TimingVariable = 3,
        [Description("Grouping Qualifiers")] GroupingQualifiers = 4,
        [Description("Result Qualifiers")] ResultQualifiers = 5,
        [Description("Synonym Qualifers")] SynonymQualifers = 6,
        [Description("Record Qualifers")] RecordQualifers = 7,
        [Description("Variable Qualifers")] VariableQualifers = 8
    }

    public enum PrintType : short
    {
        [Description("Same Line")] SameLine = 1,
        [Description("New Line")] NewLine = 2,
        [Description("Two Column")] TwoColumn = 3,
        [Description("Three Column")] ThreeColumn = 4,
        [Description("Four Column")] FourColumn = 5
    }

    public enum DataType : short
    {
        [Description("Numeric")] Numeric = 1,
        [Description("Character")] Character = 2,

        //[Description("Alpha")]
        //Alpha = 3,
        [Description("Numeric With 1 Decimal")]
        Numeric1Decimal = 4,

        [Description("Numeric With 2 Decimal")]
        Numeric2Decimal = 5,

        [Description("Numeric With 3 Decimal")]
        Numeric3Decimal = 6,

        [Description("Numeric With 4 Decimal")]
        Numeric4Decimal = 7,

        [Description("Numeric With 5 Decimal")]
        Numeric5Decimal = 8
    }

    public enum ValidationType : short
    {
        [Description("None")] None = 1,
        [Description("Soft")] Soft = 2,
        [Description("Hard")] Hard = 3,
        [Description("Required")] Required = 4
    }

    public enum CollectionSources : short
    {
        [Description("Text Box")] TextBox = 1,
        [Description("Combo Box")] ComboBox = 2,
        [Description("Date")] Date = 3,
        [Description("DateTime")] DateTime = 4,
        [Description("Time")] Time = 5,
        [Description("Radio Button")] RadioButton = 7,
        [Description("Check Box")] CheckBox = 8,
        [Description("Multi Check Box")] MultiCheckBox = 9,
        [Description("Multiline Text Box")] MultilineTextBox = 10,
        [Description("PartialDate")] PartialDate = 11
    }

    public enum VolunteerStatus : short
    {
        [Description("Incomplete")] InCompleted = 1,
        [Description("Completed")] Completed = 2
    }


    public enum FolderType
    {
        [Description("Logo")] Logo = 1,
        [Description("Employee")] Employee = 2,
        [Description("Volunteer")] Volunteer = 3,
        [Description("Project")] Project = 4,
        [Description("Screening")] Screening = 5,
        [Description("Medra Dictionary")] MedraDictionary = 6,
        [Description("DossierReport")] DossierReport = 7,
        [Description("ExcleTemplate")] ExcleTemplate = 8,
        [Description("Project Worksplace")] ProjectWorksplace = 9,

    }

    public enum AuditModule : short
    {
        [Description("Volunteer")] Volunteer = 1,
        [Description("Common")] Common = 2,
        [Description("Attendance")] Attendance = 3,
        [Description("Configuration")] Configuration = 4,
        [Description("User management")] UserManagement = 5,
        [Description("Study Set Up")] StudySetUp = 6,
        [Description("Screening")] Screening = 7,
        [Description("Design library")] DesignLibrary = 8,
        [Description("Master")] Master = 9,
        [Description("Barcode")] Barcode = 10,
        [Description("Medical Coding")] MedicalCoding = 13
    }

    public enum AuditAction : short
    {
        [Description("Inserted")] Inserted = 1,
        [Description("Updated")] Updated = 2,
        [Description("Deleted")] Deleted = 3,
        [Description("Activated")] Activated = 4
    }

    public enum AuditTable : short
    {
        Volunteer = 1,
        VolunteerAddress = 2,
        VolunteerContact = 3,
        VolunteerDocument = 4,
        VolunteerLanguage = 5
    }

    public enum ActivityMode : short
    {
        [Description("Generic")] Generic = 1,
        [Description("Subject Specific")] SubjectSpecific = 2
    }

    public enum DateFormats : short
    {
        [Description("MM/dd/yyyy")] MMddyyyy = 1,
        [Description("dd/MM/yyyy")] DdMMyyyy = 2,
        [Description("dd-MM-yyyy")] DMMyyyy = 3
    }

    public enum TimeFormats : short
    {
        [Description("HH:mm")] Hours12 = 1,
        [Description("hh:mm a")] Hours24 = 2
    }

    public enum EditCheckValidation : short
    {
        [Description("Soft Fetch")] SoftFetch = 1,

        [Description("Hard Fetch")] HardFetch = 2,

        [Description("Disable")] Disable = 3,

        [Description("Required")] Required = 4,

        [Description("Optional")] ReadOnly = 5,

        [Description("Message")] Message = 6
    }


    public enum AttendanceType : short
    {
        [Description("Screening")] Screening = 1,
        [Description("Project")] Project = 2,
        [Description("None Register")] NoneRegister = 3
    }

    public enum ScreeningStatus : short
    {
        [Description("Not Started")] Pending = 1,
        [Description("In Progress")] InProcess = 2,
        [Description("Submitted")] Submitted = 3,
        [Description("Reviewed")] Reviewed = 4,
        [Description("Completed")] Completed = 5
    }

    public enum UserRecent : short
    {
        [Description("Project")] Project = 1,
        [Description("Volunteer")] Volunteer = 2,
        [Description("Project Design")] ProjectDesign = 3,
        [Description("Screening")] Screening = 4
    }

    public enum QueryStatus : short
    {
        [Description("Open")] Open = 1,
        [Description("Answered")] Answered = 2,
        [Description("Resolved")] Resolved = 3,
        [Description("ReOpened")] Reopened = 4,
        [Description("Closed")] Closed = 5,
        [Description("Self Correction")] SelfCorrection = 6,
        [Description("Acknowledge")] Acknowledge = 7
    }

    public enum BlockndLotNo : short
    {
        [Description("Block Number")] BlockNo = 1,
        [Description("Lot Number")] InProcess = 2
    }

    public enum ReceiptDateExpRet : short
    {
        [Description("Expiration Date")] ExpirationDate = 1,
        [Description("Retested Date")] RetestedDate = 2
    }

    public enum TrainigType : short
    {
        [Description("Self")] Self = 1,
        [Description("Class Room")] ClassRoom = 2
    }

    public enum FormType : short
    {
        [Description("Receipt")] Receipt = 1,
        [Description("Verification")] Verification = 2,
        [Description("Dispense")] Dispense = 3
    }

    public enum IsFormType : short
    {
        [Description("IsReceipt")] IsReceipt = 1,
        [Description("IsVerification")] IsVerification = 2,
        [Description("IsDispense")] IsDispense = 3
    }

    public enum BarcodeFor : short
    {
        [Description("Linear")] Linear = 1,
        [Description("QR")] Qr = 2
    }

    public enum VariableCategoryType : short
    {
        [Description("Fitness")] Fitness = 1,
        [Description("Discontinued")] Discontinued = 2
    }

    public enum DateValidateType : short
    {
        [Description("Current Date")] CurrentDate = 1,
        [Description("Current and Past Date")] CurrentPastDate = 2,
        [Description("Future and Past Date")] FuturePastDate = 3,
        [Description("Only Future Date")] FutureDate = 4,
        [Description("Only Past Date")] OnlyPastDate = 5
    }

    public enum AttendaceStatus : short
    {
        [Description("Fitness Pass")] FitnessPass = 1,
        [Description("Fitness Failed")] FitnessFailed = 2,
        [Description("Discounted")] Discounted = 3,
        [Description("Replaced")] Replaced = 4,
        [Description("Suspended")] Suspended = 5
    }

    public enum SubjectNumberType : short
    {
        [Description("Normal")] Normal = 1,
        [Description("StandBy")] StandBy = 2,
        [Description("Replaced")] Replaced = 3
    }

    public enum EditCheckRuleBy : short
    {
        [Description("By Variable")] ByVariable = 1,

        [Description("By Variable Annotation")]
        ByVariableAnnotation = 2,
        [Description("By Template")] ByTemplate = 3,

        [Description("By Template Annotation")]
        ByTemplateAnnotation = 4
    }

    public enum Operator : short
    {
        [Description("Not Null")] NotNull = 1,
        [Description("=")] Equal = 2,
        [Description("!=")] NotEqual = 3,
        [Description(">")] Greater = 4,
        [Description(">=")] GreaterEqual = 5,
        [Description("<")] Lessthen = 6,
        [Description("<=")] LessthenEqual = 7,
        [Description("+")] Plus = 8,
        [Description("Diff")] Different = 9,
        [Description("%")] Percentage = 10,
        //[Description("BMI")] Bmi = 11,
        [Description("Null")] Null = 12,
        [Description("Soft Fetch")] SoftFetch = 13,
        [Description("Hard Fetch")] HardFetch = 14,
        [Description("Required")] Required = 15,
        [Description("Optional")] Optional = 16,
        [Description("Enable")] Enable = 17,
        [Description("Warning")] Warning = 18,
        [Description("Visible")] Visible = 19,
        [Description("Between")] Between = 20,
        [Description("Not Between")] NotBetween = 21,
        [Description("Filter")] Filter = 22,
        [Description("In")] In = 23,
        [Description("Not In")] NotIn = 24,
        [Description("-")] Minus = 25,
        [Description("/")] Divide = 26,
        [Description("*")] Multiplication = 27,
        [Description("^")] Power = 28,
        [Description("√")] SquareRoot = 29,
    }

    public enum ProjectScheduleOperator : short
    {
        [Description("=")] Equal = 1,
        [Description("+")] Plus = 2,
        [Description(">")] Greater = 3,
        [Description(">=")] GreaterEqual = 4,
        [Description("<")] Lessthen = 5,
        [Description("<=")] LessthenEqual = 6
    }

    public enum EditCheckValidateType : short
    {
        NotProcessed = 1,
        ReferenceVerifed = 2,
        RuleValidated = 3,
        Failed = 4
    }

    public enum DossierPdfType : short
    {
        Draft = 1,
        Final = 2,
    }

    public enum DossierPdfStatus : short
    {
        Blank = 1,
        Subject = 2,
    }

    public enum CodedType : short
    {
        [Description("Un Coded")] UnCoded = 1,
        [Description("Auto Coded")] AutoCoded = 2,
        [Description("Manual Coded")] ManualCoded = 3,
        [Description("Re-Coded")] ReCoded = 4
    }

    public enum InclutionTypeData : short
    {
        [Description("Recommended")] Recommended = 1,
        [Description("Core")] Core = 2,

    }

    public enum CommentStatus : short
    {
        [Description("Open")] Open = 1,
        [Description("Answered")] Answered = 2,
        [Description("Resolved")] Resolved = 3,
        [Description("Closed")] Closed = 4,
        [Description("Self Correction")] SelfCorrection = 5
    }

    public enum ETMFMaterLibraryColumn : short
    {
        [Description("Zone #")] Zone = 1,
        [Description("Zone Name")] ZoneName = 2,
        [Description("Section #")] Section = 3,
        [Description("Section Name")] SectionName = 4,
        [Description("Artifact #")] Artifact = 5,
        [Description("Artifact name")] Artifactname = 6,
        [Description("Core or Recommended for inclusion")] CoreorRecommendedforinclusion = 7,
        [Description("Non Device Sponsor Document")] NonDeviceSponsorDocument = 8,
        [Description("Non Device Investigator Document")] NonDeviceInvestigatorDocument = 9,
        [Description("Device Sponsor Document")] DeviceSponsorDocument = 10,
        [Description("Device Investigator Document")] DeviceInvestigatorDocument = 11,
        [Description("Investigator Initiated Study Artifacts")] InvestigatorInitiatedStudyArtifacts = 12,
        [Description("Trial Level Document ")] TrialLevelDocument = 13,
        [Description("Country/ Region Level Document")] CountryRegionLevelDocument = 14,
        [Description("Site Level Document")] SiteLevelDocument = 15,

    }

    public enum WorkPlaceFolder : short
    {
        [Description("Country")] Country = 1,
        [Description("Site")] Site = 2,
        [Description("Trial")] Trial = 3,
    }
    public enum HolidayType : short
    {
        [Description("Public")] Public = 1,
        [Description("Week Off")] WeekOff = 2
    }

    public enum JobNameType : short
    {
        [Description("Dossier Report")] DossierReport = 1,
        [Description("Medra")] Medra = 2
    }

    public enum JobTypeEnum : short
    {
        [Description("Report")] Report = 1,
        [Description("Excel")] Excel = 2
    }

    public enum JobStatusType : short
    {
        [Description("Completed")] Completed = 1,
        [Description("In Process")] InProcess = 2,
        [Description("Log")] Log = 3,
    }

    public enum Relationship : short
    {
        [Description("Husband")] Husband = 1,
        [Description("Wife")] Wife = 2,
        [Description("Partner")] Partner = 3,
        [Description("Friend")] Friend = 4,
        [Description("Father")] Father = 5,
        [Description("Mother")] Mother = 6,
        [Description("Brother")] Brother = 7,
        [Description("Sister")] Sister = 8,
        [Description("Other")] Other = 9
    }
}