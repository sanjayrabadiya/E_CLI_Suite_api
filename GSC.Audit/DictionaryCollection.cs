
using System.Collections.Generic;
using System.Text;

namespace GSC.Audit
{
    public class DictionaryCollection : IDictionaryCollection
    {
        readonly List<Dictionary> _dictionaries = new List<Dictionary>();
        public IList<Dictionary> Dictionaries { get; }
        public IList<string> SkipEntityForAudit { get; }

        public DictionaryCollection()
        {
            Dictionaries = LoadDictionaries();
        }

        List<Dictionary> LoadDictionaries()
        {
            SetDictionariesForMaster();

            return _dictionaries;
        }

        void SetDictionariesForMaster()
        {
            _dictionaries.Add(new Dictionary { FieldName = "TrialTypeId", DisplayName = "Trial Type", SourceColumn = "TrialTypeName", TableName = "TrialType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "OtherTitle", DisplayName = "Other Title" });
            //_dictionaries.Add(new Dictionary { FieldName = "Forename", DisplayName = "First Name" });
            //_dictionaries.Add(new Dictionary { FieldName = "MiddleName", DisplayName = "Middle Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Surname", DisplayName = "Last Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DateOfBirth", DisplayName = "Date Of Birth" });
            _dictionaries.Add(new Dictionary { FieldName = "CorrespondenceAddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
            _dictionaries.Add(new Dictionary { FieldName = "AddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainName", DisplayName = "Domain Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainCode", DisplayName = "Domain Code" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainClassId", DisplayName = "Domain Class", SourceColumn = "DomainClassName", TableName = "DomainClass", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainClassName", DisplayName = "Domain Class Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainClassCode", DisplayName = "Domain Class Code" });
            _dictionaries.Add(new Dictionary { FieldName = "TemplateName", DisplayName = "Form Name" });
            _dictionaries.Add(new Dictionary { FieldName = "TemplateCode", DisplayName = "Form Code" });
            _dictionaries.Add(new Dictionary { FieldName = "IsRepeated", DisplayName = "Is Repeated" });
            _dictionaries.Add(new Dictionary { FieldName = "ActivityName", DisplayName = "Activity Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DomainId", DisplayName = "Domain", SourceColumn = "DomainName", TableName = "Domain", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "CategoryName", DisplayName = "Category Name" });
            _dictionaries.Add(new Dictionary { FieldName = "CategoryCode", DisplayName = "Category Code" });
            _dictionaries.Add(new Dictionary { FieldName = "CompanyCode", DisplayName = "Company Code" });
            _dictionaries.Add(new Dictionary { FieldName = "CompanyName", DisplayName = "Company Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Phone1", DisplayName = "Phone 1" });
            _dictionaries.Add(new Dictionary { FieldName = "Phone2", DisplayName = "Phone 2" });
            _dictionaries.Add(new Dictionary { FieldName = "LocationId", DisplayName = "Location", SourceColumn = "Address", TableName = "Location", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "ReasonName", DisplayName = "Reason Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Notes", DisplayName = "Notes" });
            _dictionaries.Add(new Dictionary { FieldName = "IsOther", DisplayName = "Is Other" });
            _dictionaries.Add(new Dictionary { FieldName = "BlockCode", DisplayName = "Block Code" });
            _dictionaries.Add(new Dictionary { FieldName = "BlockCategoryName", DisplayName = "Block Category Name" });
            _dictionaries.Add(new Dictionary { FieldName = "AreaName", DisplayName = "Area Name" });
            _dictionaries.Add(new Dictionary { FieldName = "CityId", DisplayName = "City", SourceColumn = "CityName", TableName = "City", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "CityCode", DisplayName = "City Code" });
            _dictionaries.Add(new Dictionary { FieldName = "CityName", DisplayName = "City Name" });
            _dictionaries.Add(new Dictionary { FieldName = "StateId", DisplayName = "State", SourceColumn = "StateName", TableName = "State", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "CountryName", DisplayName = "Country Name" });
            _dictionaries.Add(new Dictionary { FieldName = "CountryCode", DisplayName = "Country Code" });
            _dictionaries.Add(new Dictionary { FieldName = "CountryCallingCode", DisplayName = "Country Calling Code" });
            _dictionaries.Add(new Dictionary { FieldName = "TypeName", DisplayName = "Type Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactCode", DisplayName = "Contact Code" });
            _dictionaries.Add(new Dictionary { FieldName = "ClientTypeName", DisplayName = "Client Type" });
            _dictionaries.Add(new Dictionary { FieldName = "DepartmentName", DisplayName = "Department Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DepartmentCode", DisplayName = "Department Code" });
            _dictionaries.Add(new Dictionary { FieldName = "TypeName", DisplayName = "Type Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Name", DisplayName = "Document Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DocumentTypeId", DisplayName = "Document Type", SourceColumn = "TypeName", TableName = "DocumentType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "DrugName", DisplayName = "Drug Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DosageForm", DisplayName = "Dosage Form" });
            _dictionaries.Add(new Dictionary { FieldName = "Strength", DisplayName = "Strength" });
            _dictionaries.Add(new Dictionary { FieldName = "FreezerName", DisplayName = "Freezer Name" });
            _dictionaries.Add(new Dictionary { FieldName = "FreezerType", DisplayName = "Freezer Type" });
            _dictionaries.Add(new Dictionary { FieldName = "Location", DisplayName = "Location" });
            _dictionaries.Add(new Dictionary { FieldName = "Temprature", DisplayName = "Temprature" });
            _dictionaries.Add(new Dictionary { FieldName = "Capacity", DisplayName = "Capacity" });
            //_dictionaries.Add(new Dictionary { FieldName = "Note", DisplayName = "Note" });
            _dictionaries.Add(new Dictionary { FieldName = "LanguageName", DisplayName = "Language" });
            _dictionaries.Add(new Dictionary { FieldName = "ShortName", DisplayName = "Short Name" });
            _dictionaries.Add(new Dictionary { FieldName = "MaritalStatusName", DisplayName = "Marital Status" });
            _dictionaries.Add(new Dictionary { FieldName = "OccupationName", DisplayName = "Occupation Name" });
            _dictionaries.Add(new Dictionary { FieldName = "PopulationName", DisplayName = "Population Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ProductTypeName", DisplayName = "Product Type Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ProductTypeCode", DisplayName = "Product Type Code" });
            _dictionaries.Add(new Dictionary { FieldName = "RaceName", DisplayName = "Race Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ReligionName", DisplayName = "Religion Name" });
            _dictionaries.Add(new Dictionary { FieldName = "RegulatoryTypeCode", DisplayName = "Regulatory Type Code" });
            _dictionaries.Add(new Dictionary { FieldName = "RegulatoryTypeName", DisplayName = "Regulatory Type Name" });
            _dictionaries.Add(new Dictionary { FieldName = "StateName", DisplayName = "State Name" });
            _dictionaries.Add(new Dictionary { FieldName = "CountryId", DisplayName = "Country", SourceColumn = "CountryName", TableName = "Country", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "ScopeName", DisplayName = "Scope Name" });
            _dictionaries.Add(new Dictionary { FieldName = "TestGroupName", DisplayName = "Test Group Name" });
            _dictionaries.Add(new Dictionary { FieldName = "TestName", DisplayName = "Test Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Anticoagulant", DisplayName = "Anticoagulant" });
            _dictionaries.Add(new Dictionary { FieldName = "TestGroupId", DisplayName = "Test Group", SourceColumn = "TestGroupName", TableName = "TestGroup", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "TrialTypeId", DisplayName = "Trial Type", SourceColumn = "TrialTypeName", TableName = "TrialType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "DesignTrialName", DisplayName = "Trial Design" });
            _dictionaries.Add(new Dictionary { FieldName = "DesignTrialCode", DisplayName = "Trial Design Code" });
            _dictionaries.Add(new Dictionary { FieldName = "TrialTypeName", DisplayName = "Therapeutic Indication" });
            _dictionaries.Add(new Dictionary { FieldName = "UnitName", DisplayName = "Unit Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Code", DisplayName = "Code" });
            _dictionaries.Add(new Dictionary { FieldName = "StatusName", DisplayName = "Status Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DisplayName", DisplayName = "Display Name" });
            _dictionaries.Add(new Dictionary { FieldName = "IsAuto", DisplayName = "Is Auto" });
            _dictionaries.Add(new Dictionary { FieldName = "NameOfInvestigator", DisplayName = "Name Of Investigator" });
            _dictionaries.Add(new Dictionary { FieldName = "RegistrationNumber", DisplayName = "Registration Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactNumber", DisplayName = "Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "EmailOfInvestigator", DisplayName = "Email Of Investigator" });
            _dictionaries.Add(new Dictionary { FieldName = "ManageSiteId", DisplayName = "Site Name", SourceColumn = "SiteName", TableName = "ManageSite", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "IecirbId", DisplayName = "IEC/IRB Name", SourceColumn = "IECIRBName", TableName = "Iecirb", PkName = "Id" });
            //_dictionaries.Add(new Dictionary { FieldName = "IecirbId", DisplayName = "IEC/IRB Contact Name", SourceColumn = "IECIRBContactName", TableName = "Iecirb", PkName = "Id" });
            //_dictionaries.Add(new Dictionary { FieldName = "IecirbId", DisplayName = "IEC/IRB Contact No", SourceColumn = "IECIRBContactNumber", TableName = "Iecirb", PkName = "Id" });
            //_dictionaries.Add(new Dictionary { FieldName = "IecirbId", DisplayName = "IEC/IRB Contact Email", SourceColumn = "IECIRBContactEmail", TableName = "Iecirb", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactName", DisplayName = "Contact Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactEmail", DisplayName = "Contact Email" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactNo", DisplayName = "Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactTypeId", DisplayName = "Contact Type", SourceColumn = "TypeName", TableName = "ContactType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "SecurityRoleId", DisplayName = "Role", SourceColumn = "RoleShortName", TableName = "SecurityRole", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "HolidayName", DisplayName = "Holiday Name" });
            _dictionaries.Add(new Dictionary { FieldName = "HolidayDate", DisplayName = "Holiday Date" });
            _dictionaries.Add(new Dictionary { FieldName = "Description", DisplayName = "Description" });
            _dictionaries.Add(new Dictionary { FieldName = "SiteName", DisplayName = "Site Name" });
            _dictionaries.Add(new Dictionary { FieldName = "SiteEmail", DisplayName = "Email" });
            _dictionaries.Add(new Dictionary { FieldName = "SiteAddress", DisplayName = "Site Address" });
            //_dictionaries.Add(new Dictionary { FieldName = "Status", DisplayName = "Status" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactNumber", DisplayName = "Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactName", DisplayName = "Contact Name" });
            _dictionaries.Add(new Dictionary { FieldName = "IECIRBName", DisplayName = "IEC/IRB Name" });
            _dictionaries.Add(new Dictionary { FieldName = "RegistrationNumber", DisplayName = "Registration Number" });
            _dictionaries.Add(new Dictionary { FieldName = "IECIRBContactName", DisplayName = "IEC/IRB Contact Name" });
            _dictionaries.Add(new Dictionary { FieldName = "IECIRBContactEmail", DisplayName = "IEC/IRB Contact Email" });
            _dictionaries.Add(new Dictionary { FieldName = "IECIRBContactNumber", DisplayName = "IEC/IRB Contact Number" });

            _dictionaries.Add(new Dictionary { FieldName = "ClientName", DisplayName = "Client Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ClientCode", DisplayName = "Client Code" });
            //_dictionaries.Add(new Dictionary { FieldName = "RoleId", DisplayName = "Role", SourceColumn = "RoleShortName", TableName = "SecurityRole", PkName = "Id" });
            //_dictionaries.Add(new Dictionary { FieldName = "ClientTypeId", DisplayName = "Client Type", SourceColumn = "ClientTypeName", TableName = "ClientType", PkName = "Id" });
            //_dictionaries.Add(new Dictionary { FieldName = "UserId", DisplayName = "Project Manager", SourceColumn = "UserName", TableName = "Users", PkName = "Id" });

            // usermanagement audit change

            _dictionaries.Add(new Dictionary { FieldName = "ClientId", DisplayName = "Client", SourceColumn = "ClientName", TableName = "Client", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "IsDefault", DisplayName = "Is Default" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactNo", DisplayName = "Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ContactName", DisplayName = "Contact Name" });
            //_dictionaries.Add(new Dictionary { FieldName = "IsLogin", DisplayName = "Is Login" });
            _dictionaries.Add(new Dictionary { FieldName = "IsPowerAdmin", DisplayName = "Is Power Admin" });
            _dictionaries.Add(new Dictionary { FieldName = "IsLocked", DisplayName = "Locked Status" });
            _dictionaries.Add(new Dictionary { FieldName = "IsFirstTime", DisplayName = "Is First Time" });
            _dictionaries.Add(new Dictionary { FieldName = "UserName", DisplayName = "User Name" });
            _dictionaries.Add(new Dictionary { FieldName = "FirstName", DisplayName = "First Name" });
            _dictionaries.Add(new Dictionary { FieldName = "LastName", DisplayName = "Last Name" });
            _dictionaries.Add(new Dictionary { FieldName = "MiddleName", DisplayName = "Middle Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Email", DisplayName = "Email" });
            _dictionaries.Add(new Dictionary { FieldName = "Phone", DisplayName = "Phone" });
            _dictionaries.Add(new Dictionary { FieldName = "ValidFrom", DisplayName = "Valid From" });
            _dictionaries.Add(new Dictionary { FieldName = "ValidTo", DisplayName = "Valid To" });
            _dictionaries.Add(new Dictionary { FieldName = "RoleShortName", DisplayName = "Role Short Name" });
            _dictionaries.Add(new Dictionary { FieldName = "RoleName", DisplayName = "Role Name" });
            _dictionaries.Add(new Dictionary { FieldName = "IsSystemRole", DisplayName = "Is System Role" });

            // medra coding audit
            _dictionaries.Add(new Dictionary { FieldName = "PathName", DisplayName = "Path Name" });
            _dictionaries.Add(new Dictionary { FieldName = "MimeType", DisplayName = "Mime Type" });
            _dictionaries.Add(new Dictionary { FieldName = "DictionaryId", DisplayName = "Dictionary Name", SourceColumn = "DictionaryName", TableName = "Dictionary", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "MedraVersionId", DisplayName = "Medra Version", SourceColumn = "Version", TableName = "MedraVersion", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "LanguageId", DisplayName = "Language", SourceColumn = "LanguageName", TableName = "Language", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "IsByAnnotation", DisplayName = "Is By Annotation" });
            _dictionaries.Add(new Dictionary { FieldName = "CoderProfile", DisplayName = "Coder Profile" });
            _dictionaries.Add(new Dictionary { FieldName = "CoderApprover", DisplayName = "Coder Approver" });
            _dictionaries.Add(new Dictionary { FieldName = "Version", DisplayName = "Version" });

            // manage study audit
            _dictionaries.Add(new Dictionary { FieldName = "IsStatic", DisplayName = "Is Static" });
            _dictionaries.Add(new Dictionary { FieldName = "Period", DisplayName = "Period" });
            _dictionaries.Add(new Dictionary { FieldName = "ProjectCode", DisplayName = "Project Code" });
            _dictionaries.Add(new Dictionary { FieldName = "ProjectName", DisplayName = "Project Name" });
            _dictionaries.Add(new Dictionary { FieldName = "ProjectNumber", DisplayName = "Project Number" });
            _dictionaries.Add(new Dictionary { FieldName = "AttendanceLimit", DisplayName = "Attendance Limit" });
            _dictionaries.Add(new Dictionary { FieldName = "PinCode", DisplayName = "Pin Code" });
            _dictionaries.Add(new Dictionary { FieldName = "SiteName", DisplayName = "Site Name" });
            _dictionaries.Add(new Dictionary { FieldName = "InvestigatorContactId", DisplayName = "Investigator Contact", SourceColumn = "NameOfInvestigator", TableName = "InvestigatorContact", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "RegulatoryType", DisplayName = "Regulatory Type", SourceColumn = "RegulatoryTypeName", TableName = "RegulatoryType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "DesignTrialId", DisplayName = "Design Trial", SourceColumn = "DesignTrialName", TableName = "DesignTrial", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "DrugId", DisplayName = "Drug", SourceColumn = "DrugName", TableName = "Drug", PkName = "Id" });

            _dictionaries.Add(new Dictionary { FieldName = "CheckFormula", DisplayName = "Check Formula" });
            _dictionaries.Add(new Dictionary { FieldName = "TargetFormula", DisplayName = "Target Formula" });
            _dictionaries.Add(new Dictionary { FieldName = "SampleResult", DisplayName = "Sample Result" });
            _dictionaries.Add(new Dictionary { FieldName = "SourceFormula", DisplayName = "Source Formula" });
            _dictionaries.Add(new Dictionary { FieldName = "ErrorMessage", DisplayName = "Error Message" });
            _dictionaries.Add(new Dictionary { FieldName = "IsReferenceVerify", DisplayName = "Is Reference Verify" });
            _dictionaries.Add(new Dictionary { FieldName = "IsOnlyTarget", DisplayName = "Is Only Target" });
            _dictionaries.Add(new Dictionary { FieldName = "IsFormula", DisplayName = "Is Formula" });
            _dictionaries.Add(new Dictionary { FieldName = "AutoNumber", DisplayName = "Auto Number" });
            //_dictionaries.Add(new Dictionary { FieldName = "ProjectId", DisplayName = "Project", SourceColumn = "ProjectCode", TableName = "Project", PkName = "Id" });

            // Inform Consent audit
            _dictionaries.Add(new Dictionary { FieldName = "DocumentName", DisplayName = "Document Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DocumentPath", DisplayName = "Document Path" });
            _dictionaries.Add(new Dictionary { FieldName = "ReferenceTitle", DisplayName = "Reference Title" });
            _dictionaries.Add(new Dictionary { FieldName = "FilePath", DisplayName = "File Path" });

            // All Enums Audit
            _dictionaries.Add(new Dictionary { FieldName = "ModuleId", DisplayName = "Module" });
            _dictionaries.Add(new Dictionary { FieldName = "RoleVariableType", DisplayName = "Role" });
            _dictionaries.Add(new Dictionary { FieldName = "CoreVariableType", DisplayName = "Core Type" });
            _dictionaries.Add(new Dictionary { FieldName = "CollectionSource", DisplayName = "Collection Source" });
            _dictionaries.Add(new Dictionary { FieldName = "ValidationType", DisplayName = "Validation Type" });
            _dictionaries.Add(new Dictionary { FieldName = "DataType", DisplayName = "Data Type" });
            _dictionaries.Add(new Dictionary { FieldName = "DateValidate", DisplayName = "Date Validate" });
            _dictionaries.Add(new Dictionary { FieldName = "PrintType", DisplayName = "Print Type" });
            _dictionaries.Add(new Dictionary { FieldName = "ActivityMode", DisplayName = "Type of Form" });
            _dictionaries.Add(new Dictionary { FieldName = "Gender", DisplayName = "Gender" });
            _dictionaries.Add(new Dictionary { FieldName = "LanguageId", DisplayName = "Preferred Language" });
            _dictionaries.Add(new Dictionary { FieldName = "LegalRelationship", DisplayName = "Relationship to the Subject" });
            _dictionaries.Add(new Dictionary { FieldName = "HolidayType", DisplayName = "Holiday" });
            _dictionaries.Add(new Dictionary { FieldName = "FreezerType", DisplayName = "Freezer Type" });
            //
            _dictionaries.Add(new Dictionary { FieldName = "ScreeningNumber", DisplayName = "Screening Number" });
            _dictionaries.Add(new Dictionary { FieldName = "RandomizationNumber", DisplayName = "Randomization Number" });
            _dictionaries.Add(new Dictionary { FieldName = "DateOfScreening", DisplayName = "Date Of Screening" });
            _dictionaries.Add(new Dictionary { FieldName = "DateOfRandomization", DisplayName = "Date Of Randomization" });

            //
            _dictionaries.Add(new Dictionary { FieldName = "EmergencyContactNumber", DisplayName = "Emergency Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "Qualification", DisplayName = "Qualification" });
            _dictionaries.Add(new Dictionary { FieldName = "Occupation", DisplayName = "Occupation" });
            _dictionaries.Add(new Dictionary { FieldName = "AddressLine1", DisplayName = "Address Line1" });

            _dictionaries.Add(new Dictionary { FieldName = "AddressLine2", DisplayName = "Address Line2" });
            _dictionaries.Add(new Dictionary { FieldName = "ZipCode", DisplayName = "ZipCode" });
            _dictionaries.Add(new Dictionary { FieldName = "LegalFirstName", DisplayName = "(LR)First Name" });

            _dictionaries.Add(new Dictionary { FieldName = "LegalMiddleName", DisplayName = "(LR)Middle Name" });
            _dictionaries.Add(new Dictionary { FieldName = "LegalLastName", DisplayName = "(LR)Last Name" });
            _dictionaries.Add(new Dictionary { FieldName = "LegalEmergencyCoNumber", DisplayName = "(LR)Contact Number" });
            _dictionaries.Add(new Dictionary { FieldName = "LegalEmail", DisplayName = "(LR)Email" });
            _dictionaries.Add(new Dictionary { FieldName = "PrimaryContactNumber", DisplayName = "Primary Contact Number" });
            //
            _dictionaries.Add(new Dictionary { FieldName = "IsManualRandomNo", DisplayName = "Is Manual Randomization Number" });
            _dictionaries.Add(new Dictionary { FieldName = "IsSiteDependentRandomNo", DisplayName = "Is Site Dependent Randomization Number" });
            _dictionaries.Add(new Dictionary { FieldName = "RandomNoLength", DisplayName = "Randomization Number Length" });
            _dictionaries.Add(new Dictionary { FieldName = "IsAlphaNumRandomNo", DisplayName = "Is Alpha Numeric Randomization Number" });
            _dictionaries.Add(new Dictionary { FieldName = "PrefixRandomNo", DisplayName = "Prefix Randomization Number" });
            _dictionaries.Add(new Dictionary { FieldName = "RandomNoStartsWith", DisplayName = "Randomization Number Starts with" });
            //
            _dictionaries.Add(new Dictionary { FieldName = "IsManualScreeningNo", DisplayName = "Is Manual Screening Number" });
            _dictionaries.Add(new Dictionary { FieldName = "IsSiteDependentScreeningNo", DisplayName = "Is Site Dependent Screening Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ScreeningLength", DisplayName = "Screening Number Length" });
            _dictionaries.Add(new Dictionary { FieldName = "IsAlphaNumScreeningNo", DisplayName = "Is Alpha Numeric Screening Number" });
            _dictionaries.Add(new Dictionary { FieldName = "PrefixScreeningNo", DisplayName = "Prefix Screening Number" });
            _dictionaries.Add(new Dictionary { FieldName = "ScreeningNoStartsWith", DisplayName = "Screening Number Starts with" });

            _dictionaries.Add(new Dictionary { FieldName = "ProjectId", DisplayName = "Study", SourceColumn = "ProjectCode", TableName = "Project", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "ProjectDesignTemplateIdPatient", DisplayName = "Patient Template", SourceColumn = "ProjectDesignTemplateIdPatient", TableName = "ProjectDesignTemplate", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "ProjectDesignTemplateIdInvestigator", DisplayName = "Investigator Template", SourceColumn = "ProjectDesignTemplateIdInvestigator", TableName = "ProjectDesignTemplate", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "SeveritySeqNo1", DisplayName = "Label for Low", SourceColumn = "SeveritySeqNo1", TableName = "ProjectDesignVariableValue", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "SeveritySeqNo2", DisplayName = "Label for Medium", SourceColumn = "SeveritySeqNo2", TableName = "ProjectDesignVariableValue", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "SeveritySeqNo3", DisplayName = "Label for High", SourceColumn = "SeveritySeqNo3", TableName = "ProjectDesignVariableValue", PkName = "Id" });

            _dictionaries.Add(new Dictionary { FieldName = "IsSendEmail", DisplayName = "Send Email to Patients" });
            _dictionaries.Add(new Dictionary { FieldName = "IsSendSMS", DisplayName = "Send SMS to Patients" });

            _dictionaries.Add(new Dictionary { FieldName = "SectionNo", DisplayName = "SectionNo" });
            _dictionaries.Add(new Dictionary { FieldName = "ReferenceTitle", DisplayName = "Reference Title" });
            _dictionaries.Add(new Dictionary { FieldName = "FilePath", DisplayName = "File Path" });
        }


    }
}
