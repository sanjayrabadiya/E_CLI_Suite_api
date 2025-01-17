﻿using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Data.Entities.Master;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignTemplate : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVisitId { get; set; }
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string ActivityName { get; set; }
        public int VariableTemplateId { get; set; }
        public IList<ProjectDesignVariable> Variables { get; set; }
        public int? ParentId { get; set; }
        public int DesignOrder { get; set; }
        public int? DomainId { get; set; }
        public bool IsRepeated { get; set; }
        public bool IsParticipantView { get; set; }
        public Domain Domain { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
        public double? InActiveVersion { get; set; }
        public double? StudyVersion { get; set; }
        public string Label { get; set; }
        public string PreLabel { get; set; }
        public IList<ProjectDesignTemplateNote> ProjectDesignTemplateNote { get; set; }
        public List<TemplateLanguage> TemplateLanguage { get; set; }
        public List<ProjectDesingTemplateRestriction> ProjectDesingTemplateRestriction { get; set; }
        public List<WorkflowTemplate> WorkflowTemplate { get; set; }
    }
}