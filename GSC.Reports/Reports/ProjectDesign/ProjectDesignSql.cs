using GSC.Reports.General;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Reports.Reports.ProjectDesign
{
    public class ProjectDesignSql
    {
        public DataSet GetProjectDesign(int id)
        {
            var strSql = new StringBuilder();

            strSql.Append(@"SELECT Project.ProjectCode,Project.ProjectNumber,Project.ProjectName,ProjectDesign.Id,ProjectDesign.Period FROM ProjectDesign
                    JOIN Project on Project.Id = ProjectDesign.ProjectId
                    WHERE ProjectDesign.Id = @id");
           
            Database.ResetParameters();
            Database.AddParam("@id", id);
            DataSet dsReturn = Database.ExecuteSql(strSql.ToString());

            return dsReturn;
        }

        public DataSet GetProjectPeriod(int id)
        {
            var strSql = new StringBuilder();

            strSql.Append(@"SELECT Id,DisplayName,Description FROM ProjectDesignPeriod
            WHERE ProjectDesignPeriod.ProjectDesignId = @id");

            Database.ResetParameters();
            Database.AddParam("@id", id);
            DataSet dsReturn = Database.ExecuteSql(strSql.ToString());

            return dsReturn;
        }

        public DataSet GetProjectVisit(int id)
        {
            var strSql = new StringBuilder();

            strSql.Append(@"SELECT Id,DisplayName,Description FROM ProjectDesignVisit
                WHERE ProjectDesignVisit.ProjectDesignPeriodId = @id");

            Database.ResetParameters();
            Database.AddParam("@id", id);
            DataSet dsReturn = Database.ExecuteSql(strSql.ToString());

            return dsReturn;
        }

        public DataSet GetProjectTemplate(int id)
        {
            var strSql = new StringBuilder();

            strSql.Append(@"SELECT Id, TemplateCode,TemplateName,ActivityName  FROM ProjectDesignTemplate
            WHERE ProjectDesignTemplate.ProjectDesignVisitId = @id");

            Database.ResetParameters();
            Database.AddParam("@id", id);
            DataSet dsReturn = Database.ExecuteSql(strSql.ToString());

            return dsReturn;
        }

        public DataSet GetProjectVariable(int id)
        {
            var strSql = new StringBuilder();

            strSql.Append(@"SELECT VariableCode,VariableName, Annotation FROM ProjectDesignVariable
                WHERE ProjectDesignVariable.ProjectDesignTemplateId = @id");

            Database.ResetParameters();
            Database.AddParam("@id", id);
            DataSet dsReturn = Database.ExecuteSql(strSql.ToString());

            return dsReturn;
        }
    }
}
