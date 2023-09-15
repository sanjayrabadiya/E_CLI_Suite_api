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

    //public enum RegulatoryType : short
    //{
    //    [Description("IEC/IRB")] Iecirb = 1,
    //    [Description("T-License")] License = 2,
    //    [Description("NOC")] Noc = 3,
    //    [Description("Others")] Others = 4
    //}

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
        [Description("Horizontal")] Horizontal = 1,
        [Description("Vertical")] Vertical = 2,
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

    public enum EditCheckInfoType : short
    {
        [Description("Failed")] Failed = 1,
        [Description("Info")] Info = 2,
        [Description("Warning")] Warning = 3
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
        [Description("PartialDate")] PartialDate = 11,
        [Description("Scale Horizontal")] HorizontalScale = 12,
        //   [Description("Scale Vertical")] VerticalScale = 13,
        [Description("Numeric Scale")] NumericScale = 14,
        [Description("Relation")] Relation = 15,
        [Description("Table")] Table = 16
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
        //[Description("Project Worksplace")] ProjectWorksplace = 9,
        [Description("ETMF")] Etmf = 9,
        [Description("Inform Consent")] InformConcent = 10,
        [Description("DBDS Report")] DBDSReport = 7,
        [Description("DataEntry Audit")] DataEntryAudit = 11,
        [Description("Study Design Audit")] StudyDesignAudit = 12,
        [Description("Product Receipt")] ProductReceipt = 13,
        [Description("Product Verification")] ProductVerification = 14,
        [Description("Traning document")] TraningDocument = 15,
        [Description("DataEntry")] DataEntry = 16,
        [Description("User")] User = 17,
        [Description("Company")] Company = 18,
        [Description("Clent")] Client = 19,
        [Description("Lab Management")] LabManagement = 20,
        [Description("ICF Detail Report")] ICFDetailReport = 21,
        [Description("RoleIcon")] RoleIcon = 22,
        [Description("Ctms")] Ctms = 23,
        [Description("Randomization Upload")] RandomizationUpload = 24
    }

    public enum AuditModule : short
    {
        [Description("Volunteer")] Volunteer = 1,
        [Description("Common")] Common = 2,
        [Description("Attendance")] Attendance = 3,
        [Description("General Configuration")] GeneralConfiguration = 4,
        [Description("User management")] UserManagement = 5,
        [Description("Study Set Up")] StudySetUp = 6,
        //[Description("Screening")] Screening = 7,
        [Description("Design library")] DesignLibrary = 8,
        [Description("Master")] Master = 9,
        [Description("Barcode")] Barcode = 10,
        [Description("Medical Coding")] MedicalCoding = 13,
        [Description("Data Management")] DataManagement = 14,
        [Description("ETMF")] ETMF = 15,
        [Description("Report")] Report = 16,
        [Description("Site Management")] SiteManagement = 17,
        [Description("Study Output")] StudyOutput = 18,
        [Description("Inform Consent")] InformConsent = 19,
        [Description("AdverseEvent")] AdverseEvent = 20,
        [Description("CTMS")] CTMS = 22,
        [Description("Supply Management")] SupplyManagement = 23,
        [Description("Lab Management")] LabManagement = 24
    }



    public enum AuditTable : short
    {
        Volunteer = 1,
        VolunteerAddress = 2,
        VolunteerContact = 3,
        VolunteerDocument = 4,
        VolunteerLanguage = 5,
        VolunteerScreening = 6,
    }

    public enum ActivityMode : short
    {
        [Description("Non CRF Form")] Generic = 1,
        [Description("CRF Form")] SubjectSpecific = 2
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


    public enum DataEntryType : short
    {
        [Description("Screening")] Screening = 1,
        [Description("Project")] Project = 2,
        [Description("Randomization")] Randomization = 3
    }

    public enum ScreeningTemplateStatus : short
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
        [Description("By Variable")]
        ByVariable = 1,

        [Description("By Variable Annotation")]
        ByVariableAnnotation = 2,

        [Description("By Form")]
        ByTemplate = 3,

        [Description("By Template Annotation")]
        ByTemplateAnnotation = 4,

        [Description("By Variable Rule")]
        ByVariableRule = 5,

        [Description("By Visit")]
        ByVisit = 6
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
        [Description("Avg")] Avg = 11,
        [Description("Null")] Null = 12,
        [Description("Soft Fetch")] SoftFetch = 13,
        [Description("Hard Fetch")] HardFetch = 14,
        [Description("Required")] Required = 15,
        [Description("Optional")] Optional = 16,
        [Description("Enable")] Enable = 17,
        [Description("Warning")] Warning = 18,
        [Description("Hide")] Hide = 19,
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
        [Description("Default")] Default = 30
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
        Passed = 3,
        Failed = 4,
        Warning = 5
    }

    public enum DossierPdfType : short
    {
        Draft = 1,
        Final = 2,
    }

    public enum DossierPdfStatus : short
    {
        [Description("Blank")] Blank = 1,
        [Description("Subject/Volunteer")] Subject = 2,
    }

    public enum ScreeningPdfStatus : short
    {
        Blank = 1,
        Volunteer = 2,
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

    public enum WorkplaceStatus : short
    {
        [Description("Sent For Review")] SentForReview = 1,
        [Description("Sent For Approve")] SentForApprove = 2,
    }
    public enum HolidayType : short
    {
        [Description("Public")] Public = 1,
        [Description("Week Off")] WeekOff = 2
    }

    public enum JobNameType : short
    {
        [Description("Dossier Report")] DossierReport = 1,
        [Description("Medra")] Medra = 2,
        [Description("DBDS Report")] DBDSReport = 3,
        [Description("DataEntry Audit")] DataEntryAudit = 4,
        [Description("Project Design Audit")] ProjectDesignAudit = 5,
        [Description("ICF Detail Report")] ICFDetailReport = 6,
        [Description("ETMF")] ETMF = 7,
        [Description("Screening Report")] ScreeningReport = 8,
    }

    public enum JobTypeEnum : short
    {
        [Description("Report")] Report = 1,
        [Description("Excel")] Excel = 2,
        [Description("Csv")] Csv = 3,
        [Description("Pdf")] Pdf = 4,
        [Description("Zip")] Zip = 5
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

    public enum ArtifactDocStatusType : short
    {
        [Description("Draft")] Draft = 1,
        [Description("Final")] Final = 2,
        [Description("Supersede")] Supersede = 3,
        [Description("Not Required")] NotRequired = 4,
        [Description("Expired")] Expired = 5,
    }


    public enum ScreeningVisitStatus : int
    {
        [Description("Not Started")] NotStarted = 1,
        [Description("Scheduled")] Scheduled = 2,
        [Description("Re-Schedule")] ReSchedule = 3,
        [Description("Open")] Open = 4,
        [Description("In Progress")] InProgress = 5,
        [Description("Missed")] Missed = 6,
        [Description("Withdrawal")] Withdrawal = 7,
        [Description("On Hold")] OnHold = 8,
        [Description("Screening Failure")] ScreeningFailure = 9,
        [Description("Completed")] Completed = 10
    }

    public enum ScreeningPatientStatus : int
    {
        [Description("Pre-Screening")] PreScreening = 1,
        [Description("Screened")] Screening = 2,
        [Description("Consent In Process")] ConsentInProcess = 3,
        [Description("Consent Completed")] ConsentCompleted = 4,
        [Description("Re-Consent in Process")] ReConsentInProcess = 5,
        [Description("Screening Failure")] ScreeningFailure = 6,
        [Description("On Trial")] OnTrial = 7,
        [Description("On Hold")] OnHold = 8,
        [Description("Completed")] Completed = 9,
        [Description("Withdrawal")] Withdrawal = 10,
    }

    public enum MyTaskModule : short
    {
        [Description("Volunteer")] Volunteer = 1,
        [Description("Common")] Common = 2,
        //[Description("Attendance")] Attendance = 3,
        [Description("General Configuration")] GeneralConfiguration = 4,
        [Description("User management")] UserManagement = 5,
        [Description("Study Set Up")] StudySetUp = 6,
        //[Description("Screening")] Screening = 7,
        [Description("Design library")] DesignLibrary = 8,
        [Description("Master")] Master = 9,
        //[Description("Barcode")] Barcode = 10,
        [Description("Medical Coding")] MedicalCoding = 13,
        [Description("Data Management")] DataManagement = 14,
        [Description("ETMF")] ETMF = 15,
        [Description("Report")] Report = 16,
        [Description("Site Management")] SiteManagement = 17,
        [Description("Inform Consent")] InformConsent = 18,
        //[Description("Study Output")] StudyOutput = 18,
        [Description("CTMS")] CTMS = 22,
        [Description("Adverse Event")] AdverseEvent = 23
    }

    public enum MyTaskMethodModule : short
    {
        [Description("Approved")] Approved = 1,
        [Description("Reviewed")] Reviewed = 2,
        [Description("SendBack")] SendBack = 2,
    }

    public enum DBDSReportFilter : short
    {
        [Description("DBDS")] DBDS = 1,
        [Description("MedDRA")] MedDRA = 2
    }

    public enum Alignment : short
    {
        [Description("Left")] Left = 1,
        [Description("Right")] Right = 2,
        [Description("Top")] Top = 3,
        [Description("Bottom")] Bottom = 4
    }

    public enum AdverseEventEffect : int
    {
        [Description("Low")] Low = 1,
        [Description("Medium")] Medium = 2,
        [Description("High")] High = 3
    }

    public enum EtmfChartType : int
    {
        //1: core
        //2: Recommended
        //3: Missing
        //4: Pending Review
        //5: Pending Approve
        //6: Final
        //7: Incomplete
        //8: Not Required
        [Description("Nothing")] Nothing = 0,
        [Description("core")] core = 1,
        [Description("Recommended")] Recommended = 2,
        [Description("Missing")] Missing = 3,
        [Description("Pending Review")] PendingReview = 4,
        [Description("Pending Approve")] PendingApprove = 5,
        [Description("Final")] Final = 6,
        [Description("Incomplete")] Incomplete = 7,
        [Description("Not Required")] NotRequired = 8,
        [Description("Pending Final")] PendingFinal = 9,
        [Description("Expired")] Expired = 10

    }

    public enum VideoCallStatus : int
    {
        [Description("Not Answered")] NotAnswered = 1,
        [Description("Call Declined")] CallDeclined = 2,
        [Description("Call Accepted")] CallAccepted = 3,
        [Description("Call End By Sender Before Connecting")] CallEndBySenderBeforeConnecting = 4
    }

    public enum VideoCallStatusCallEndBy : int
    {
        [Description("Sender")] Sender = 1,
        [Description("Receiver")] Receiver = 2
    }

    public enum Position
    {
        Above,
        Below,
        Child,
        None
    }
    public enum ActivityType : short
    {
        [Description("Finish-Finish")] FF = 1,
        [Description("Finish-Start")] FS = 2,
        [Description("Start-Finish")] SF = 3,
        [Description("Start-Start")] SS = 4
    }
    public enum RefrenceType : short
    {
        [Description("Study")] Study = 1,
        [Description("Sites")] Sites = 2,
        [Description("Country")] Country = 3,

    }

    public enum DepotType : short
    {
        [Description("Central")] Central = 1,
        [Description("Local")] Local = 2
    }

    public enum DayType : short
    {
        [Description("Sunday")] Sunday = 1,
        [Description("Monday")] Monday = 2,
        [Description("Tuesday")] Tuesday = 3,
        [Description("Wednesday")] Wednesday = 4,
        [Description("Thursday")] Thursday = 5,
        [Description("Friday")] Friday = 6,
        [Description("Saturday")] Saturday = 7,
    }

    public enum FrequencyType : short
    {
        [Description("All")] All = 1,
        [Description("1st")] First = 2,
        [Description("2nd")] Second = 3,
        [Description("3rd")] Third = 4,
        [Description("4th")] Fourth = 5,
        [Description("5th")] Fifth = 6,
        [Description("Odd(1,3,5)")] Odd = 7,
        [Description("Even(2,4)")] Even = 8,
    }

    public enum VersionStatus : short
    {
        [Description("On Trial")] OnTrial = 1,
        [Description("Go Live")] GoLive = 2,
        [Description("Archive")] Archive = 3,
    }

    public enum DbdsReportType : short
    {
        [Description("Domain")] Domain = 1,
        [Description("Patient")] Patient = 2
    }

    public enum ProductUnitType : short
    {
        [Description("Kit")] Kit = 1,
        [Description("Individual")] Individual = 2
    }

    public enum BatchLotType : short
    {
        [Description("Batch")] Batch = 1,
        [Description("Lot")] Lot = 2
    }

    public enum ReTestExpiry : short
    {
        [Description("Re-Test")] ReTest = 1,
        [Description("Expiry")] Expiry = 2
    }

    public enum CtmsChartType : int
    {
        [Description("Completed")] Completed = 1,
        [Description("DueDate")] DueDate = 2,
        [Description("DeviatedDate")] DeviatedDate = 3,
        [Description("OnGoingDate")] OnGoingDate = 4,
        [Description("NotStarted")] NotStarted = 5,
    }
    public enum UploadLimitType : short
    {
        [Description("Unlimited")] Unlimited = 1,
        [Description("StudyBase")] StudyBase = 2
    }
    public enum DocumentStatus : short
    {
        [Description("Pending")] Pending = 1,
        [Description("Final")] Final = 2
    }
    //Changes made by Sachin
    public enum MonitoringReportStatus : short
    {
        [Description("On Going")] OnGoing = 1,
        //[Description("Ongoing")] Ongoing = 2,
        [Description("Under Review")] UnderReview = 3,
        [Description("Review In Progress")] ReviewInProgress = 4,
        [Description("Approved")] Approved = 5,
    }

    public enum MonitoringSiteStatus : short
    {
        [Description("Approved")] Approved = 1,
        [Description("Rejected")] Rejected = 2,
        [Description("Terminated")] Terminated = 3,
        [Description("On Hold")] OnHold = 4,
        [Description("Close Out")] CloseOut = 5,
        [Description("Active")] Active = 6,
    }


    public enum LabManagementUploadStatus : short
    {
        [Description("Pending")] Pending = 1,
        [Description("Reject")] Reject = 2,
        [Description("Approve")] Approve = 3
    }

    public enum ICFAction : short
    {
        [Description("Screened")] Screened = 1,
        [Description("Approve")] Approve = 2,
        //[Description("Reject")] Reject = 3,
        [Description("Withdraw")] Withdraw = 3,
    }

    public enum CtmsCommentStatus : short
    {
        [Description("Open")] Open = 1,
        [Description("Answered")] Answered = 2,
        [Description("Resolved")] Resolved = 3,
        [Description("Closed")] Closed = 4,
    }

    public enum CtmsActionPointStatus : short
    {
        [Description("Open")] Open = 1,
        [Description("Resolved")] Resolved = 2,
        [Description("Closed")] Closed = 3,
    }

    public enum LabManagementExcelFileColumn : short
    {
        [Description("Study Code")] StudyCode = 1,
        [Description("Site Code")] SiteCode = 2,
        [Description("Screening ID")] ScreeningID = 3,
        [Description("Randomization Number")] RandomizationNumber = 4,
        [Description("Visit")] Visit = 5,
        [Description("Repeat sample collection (Yes, No)")] RepeatSampleCollection = 6,
        [Description("Laboratory Name")] LaboratoryName = 7,
        [Description("Date of sample collection (dd-mmm-yyyy)")] DateOfSampleCollection = 8,
        [Description("Date of Report (dd-mmm-yyyy)")] DateOfReport = 9,
        [Description("Panel (Hematology, Biochemistry, serology, Urinalysis)")] Panel = 10,
        [Description("Test Name")] TestName = 11,
        [Description("Result")] Result = 12,
        [Description("Unit")] Unit = 13,
        [Description("Abnormal Flag (L=Low, H=High, N=Normal)")] AbnormalFlag = 14,
        [Description("Reference Range Low")] ReferenceRangeLow = 15,
        [Description("Reference Range High")] ReferenceRangeHigh = 16

    }

    public enum ProductVerificationStatus : short
    {
        [Description("Quarantine")] Quarantine = 1,
        [Description("Sent For Approval")] SentForApproval = 2,
        [Description("Approved")] Approved = 3,
        [Description("Rejected")] Rejected = 4,
    }
    public enum SupplyMangementShipmentStatus : short
    {
        [Description("Approved")] Approved = 1,
        [Description("Rejected")] Rejected = 2
    }

    public enum SupplyManagementUploadFileLevel : short
    {
        [Description("Study")] Study = 1,
        [Description("Country")] Country = 2,
        [Description("Site")] Site = 3
    }
    public enum CRFTypes : short
    {
        [Description("ESource")] NonCRF = 1,
        [Description("CRF")] CRF = 2,
        [Description("Both")] Both = 3
    }
    public enum PdfLayouts : short
    {
        [Description("Layout 1")] Layout1 = 1,
        [Description("Layout 2")] Layout2 = 2,
        [Description("Layout 3")] Layout3 = 3

    }
    public enum TableCollectionSource : short
    {
        [Description("TextBox")] TextBox = 1,
        [Description("CheckBox")] CheckBox = 2,
        [Description("DateTime")] DateTime = 3,
        [Description("Date")] Date = 4,
        [Description("Time")] Time = 5
    }

    public enum DashboardMyTaskType : short
    {
        [Description("ETMFApproveData")] ETMFApproveData = 1,
        [Description("ETMFSubSecApproveData")] ETMFSubSecApproveData = 2,
        [Description("ETMFSendData")] ETMFSendData = 3,
        [Description("ETMFSubSecSendData")] ETMFSubSecSendData = 4,
        [Description("EConsentData")] EConsentData = 5,
        [Description("ETMFSendBackData")] ETMFSendBackData = 6,
        [Description("ETMFSubSecSendBackData")] ETMFSubSecSendBackData = 7,
        [Description("ManageMonitoringReportSendData")] ManageMonitoringReportSendData = 8,
        [Description("ManageMonitoringReportSendBackData")] ManageMonitoringReportSendBackData = 9,
        [Description("EAdverseEvent")] EAdverseEvent = 10,
    }

    public enum ScaleType : short
    {
        [Description("Normal")] Normal = 1,
        [Description("Color Range")] ColorRange = 2,
        [Description("Smiley")] Smiley = 3
    }

    public enum ScreeningFitnessFit : short
    {
        [Description("FitnessFit")] FitnessFit = 1
    }

    public enum ScreeningFitnessFitVariable : short
    {
        [Description("FF001")] FitnessFit = 1,
        [Description("FF002")] Enrolled = 2,
        [Description("FF003")] ProjectNumber = 3,
        [Description("FF004")] Reason = 4,
        [Description("FF005")] Note = 5,
        [Description("FF006")] NameOfPI = 6,
        [Description("FF007")] DateTimeOfFitnessFit = 7,
    }

    public enum QueryTypes : short
    {
        [Description("Critical")] Critical = 1,
        [Description("Major")] Major = 2,
        [Description("Minor")] Minor = 3,
    }
    public enum ShipmentType : short
    {
        [Description("Site to Site")] SiteToSite = 1,
        [Description("Site to Study")] SiteToStudy = 2
    }
    public enum KitCreationType : int
    {
        [Description("Individual Kit")] KitWise = 1,
        [Description("Kit Pack")] SequenceWise = 2
    }
    public enum KitStatus : int
    {
        [Description("Created")] AllocationPending = 1,
        [Description("Shipped")] Shipped = 2,
        [Description("Returned")] Returned = 3,
        [Description("Missing")] Missing = 4,
        [Description("Damage/Expiry")] Damaged = 5,
        [Description("Valid")] WithIssue = 6,
        [Description("Valid")] WithoutIssue = 7,
        [Description("Allocated")] Allocated = 8,
        [Description("Discard")] Discard = 9,
        [Description("Send to sponsor")] Sendtosponser = 10,
        [Description("Invalid")] ReturnReceive = 11,
        [Description("Valid")] ReturnReceiveWithIssue = 12,
        [Description("Valid")] ReturnReceiveWithoutIssue = 13,
        [Description("Invalid")] ReturnReceiveMissing = 14,
        [Description("Damage/Expiry")] ReturnReceiveDamaged = 15
    }

    public enum ScreeningReport : short
    {
        [Description("Query Report")] QueryReport = 1,
        [Description("Review Report")] ReviewReport = 2,
        [Description("PDF")] PDF = 3,
    }

    public enum SupplyManagementAllocationType : int
    {
        [Description("Randomization No")] RandomizationNo = 1,
        [Description("Kit No")] KitNo = 2,
        [Description("Product Code")] ProductCode = 3,
        [Description("Product Name")] ProductName = 4,
        [Description("Randomization Date")] RandomizationDate = 5

    }

    public enum InformConsentChart : short
    {
        [Description("Screened")] Screened = 1,
        [Description("Consent Inprogress")] ConsentInprogress = 2,
        [Description("Consent Completed")] ConsentCompleted = 3,
        [Description("ReConsent")] ReConsent = 4,
        [Description("Consent Withdraw")] ConsentWithdraw = 5
    }

    public enum ProjectStatusEnum : short
    {
        [Description("Pre Start-Up")] PreStartUp = 1,
        [Description("Not Started")] NotStarted = 2,
        [Description("On Going")] Ongoing = 3,
        [Description("On Hold")] OnHold = 4,
        [Description("Completed")] Completed = 5,
        [Description("Terminated")] Terminated = 6,
    }
    public enum Fector : short
    {
        [Description("Gender")] Gender = 1,
        [Description("Dietory")] Diatory = 2,
        [Description("BMI")] BMI = 3,
        [Description("Age")] Age = 4,
        [Description("Joint")] Joint = 5,
        [Description("Eligibility")] Eligibility = 6,
        [Description("Weight")] Weight = 7,
        [Description("Dose")] Dose = 8
    }
    public enum FectorOperator : short
    {
        [Description("=")] Equal = 1,
        [Description(">")] Greater = 2,
        [Description(">=")] GreaterEqual = 3,
        [Description("<")] Lessthen = 4,
        [Description("<=")] LessthenEqual = 5,
    }

    public enum FectoreType : short
    {
        [Description("Study Level")] StudyLevel = 1,
        [Description("Site Level")] SiteLevel = 2,
        [Description("Country Level")] CountryLevel = 3
    }
    public enum DaitoryFector : short
    {
        [Description("Veg")] Veg = 1,
        [Description("Non-Veg")] NonVeg = 2
    }
    public enum KitStatusRandomization : short
    {
        [Description("Used")] Used = 1,
        [Description("UnUsed")] UnUsed = 2,
        [Description("Damaged")] Damaged = 3,
        [Description("Returned")] Return = 4,
        [Description("Discard")] Discard = 5,
        [Description("Send to sponser")] Sendtosponser = 6,
        [Description("Missing")] Missing = 7,
        [Description("Return Receive")] ReturnReceive = 8,
        [Description("Return Receive With Issue")] ReturnReceiveWithIssue = 9,
        [Description("Return Receive Without Issue")] ReturnReceiveWithoutIssue = 10,
        [Description("Return Receive Missing")] ReturnReceiveMissing = 11,
        [Description("Return Receive Damaged")] ReturnReceiveDamaged = 12


    }
    public enum Jointfactor : short
    {
        [Description("Knee")] Knee = 1,
        [Description("Low Back")] LowBack = 2
    }
    public enum Eligibilityfactor : short
    {
        [Description("Yes")] Yes = 1,
        [Description("No")] No = 2
    }
    public enum SupplyManagementEmailTriggers : short
    {
        [Description("Threshold")] Threshold = 1,
        [Description("Send for approval(Verification Template)")] SendforApprovalVerificationTemplate = 2,
        [Description("Verification Template Approved/Rejected")] VerificationTemplateApproveReject = 3,
        [Description("Randomization Sheet Approved/Rejected")] RandomizationSheetApprovedRejected = 4,
        [Description("Shipment Request")] ShipmentRequest = 5,
        [Description("Shipment Approved/Rejected")] ShipmentApproveReject = 6,
        [Description("Kit Return")] KitReturn = 7,
        [Description("Subject Randomization")] SubjectRandomization = 8,
        [Description("Unblind")] Unblind = 9
    }

    public enum TreatmentUnblindType : int
    {
        [Description("Emeregency")] Emeregency = 1,
        [Description("Planned")] Planned = 2
    }

    public enum PKBarcodeOption : int
    {
        [Description("Duplicate")] Duplicate = 2,
        [Description("Triplicate")] Triplicate = 3,
        [Description("Singlicate")] Singlicate = 1,
        [Description("Replicate")] Replicate = 4
    }

    public enum BarcodeGenerationType : int
    {
        [Description("Subject Barcode")] SubjectBarcode = 1,
        [Description("Pk Barcode")] PkBarocde = 2,
        [Description("Sample Barcode")] SampleBarcode = 3,
        [Description("Dosing Barcode")] DosingBarcode = 4
    }

    public enum ProductAccountabilityActions : int
    {
        [Description("Product Receipt")] ProductReciept = 1,
        [Description("Verification")] ProductVerification = 2,
        [Description("Kit Pack")] KitPack = 3,
        [Description("Individual Kit")] Kit = 4,
        [Description("Individual")] Individual = 5
    }

    public enum CentrifugationFilter : short
    {
        [Description("Centrifuged")] Centrifugation = 1,
        [Description("Remaining")] Remaining = 2,
        [Description("Missed")] Missed = 3,
        [Description("Recentrifuged")] ReCentrifugation = 4
    }

    public enum SampleSeparationFilter : short
    {
        [Description("Separated")] Separated = 1,
        [Description("Remaining")] Remaining = 2,
        [Description("Missed")] Missed = 3,
        [Description("Hemolized")] Hemolized = 4
    }

    public enum SupplyManagementApprovalType : short
    {
        [Description("Shipment Request Approval")] ShipmentApproval = 1,
        [Description("Workflow Approved")] WorkflowApproval = 2
    }

    public enum SupplyManagementApprovalStatus : short
    {
        [Description("Approved")] Approved = 1,
        [Description("Rejected")] Rejected = 2
    }

    // Changes made by Sachin added Vendor Management on 2/6/2023
    public enum VendorManagementAudit : short
    {
        [Description("Yes")] Yes = 1,
        [Description("No")] No = 2,
        [Description("N/A")] NA = 3
    }
    public enum MetricsType : short
    {
        [Description("Enrolled")] Enrolled = 1,
        [Description("Screened")] Screened = 2,
        [Description("Randomized")] Randomized = 3
    }
    public enum PlanningType : short
    {
        [Description("Day")] Day = 1,
        [Description("Week")] Week = 2,
        [Description("Month")] Month = 3,
        [Description("Year")] Year = 4
    }

    public enum PacketType : short
    {
        [Description("Bottle")] Bottle = 1,
        [Description("HDPE")] HDPE = 2,
        [Description("Strip")] Strip = 3,
        [Description("Patch")] Patch = 4
    }

    public enum DosePriority : short
    {
        [Description("Priority 1")] Priority1 = 1,
        [Description("Priority 2")] Priority2 = 2,
        [Description("Priority 3")] Priority3 = 3

    }

    public enum HideDisableType : short
    {
        [Description("None")] None = 0,
        [Description("Hide")] Hide = 1,
        [Description("Disable")] Disable = 2

    }

    public enum KitHistoryReportType : int
    {
        [Description("Kit Wise")] KitWise = 1,
        [Description("Randomization Wise")] RandomizationWise = 2
    }


    public enum DocumentVerifyStatus : int
    {
        [Description("Pending")] Pending = 1,
        [Description("Approved")] Approved = 2,
        [Description("Rejected")] Rejected = 3
    }
    public enum ResourceTypeEnum : short
    {
        [Description("Manpower")] Manpower = 1,
        [Description("Material")] Material = 2,
    }
    public enum SubResourceType : short
    {
        [Description("Permanent")] Permanent = 1,
        [Description("Contract")] Contract = 2,
        [Description("Consumable")] Consumable = 3,
        [Description("Non Consumable")] NonConsumable = 4
    }
}