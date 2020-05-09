using System.Collections.Generic;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningSummaryValueItem
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }

    public class ScreeningSummaryValue
    {
        public int Id { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
    }

    public class ScreeningSummaryVariable
    {
        public int ProjectDesignVariableId { get; set; }
        public string VariableCategoryName { get; set; }
        public int DesignOrder { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public CollectionSources CollectionSource { get; set; }
        public IList<ScreeningSummaryValueItem> Items { get; set; }

        public IList<ProjectDesignVariableValue> Values { get; set; }
    }

    public class ScreeningSummaryTemplate
    {
        public string Name { get; set; }
        public int DesignOrder { get; set; }
        public IList<ScreeningSummaryVariable> Variables { get; set; }
    }

    public class ScreeningSummaryVisit
    {
        public string Name { get; set; }
        public IList<ScreeningSummaryTemplate> Templates { get; set; }
    }

    public class ScreeningSummaryDto
    {
        public IList<ScreeningSummaryVisit> Visits { get; set; }
    }
}