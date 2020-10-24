
using GSC.Common;
using GSC.Data.Dto.Audit;
using GSC.Data.Entities.Audit;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;



namespace GSC.Audit
{
    public class AuditTracker : IAuditTracker
    {
        private readonly GscContext _gscContext;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IDictionaryCollection _dictionaryCollection;
        public AuditTracker(GscContext gscContext, IDictionaryCollection dictionaryCollection, IJwtTokenAccesser jwtTokenAccesser)
        {
            _gscContext = gscContext;
            _dictionaryCollection = dictionaryCollection;
            _jwtTokenAccesser = jwtTokenAccesser;

        }

        public List<AuditTrailCommon> GetAuditTracker()
        {
            List<TrackerResult> trackers = new List<TrackerResult>();
            var auditTrailCommons = new List<AuditTrailCommon>();
            try
            {

                var userId = _jwtTokenAccesser.UserId;
                var roleId = _jwtTokenAccesser.RoleId;
                var createdDate = Convert.ToDateTime(_jwtTokenAccesser.GetHeader("clientDateTime"));
                var changeTracker = _gscContext.GetAuditTracker()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).ToList();


                int.TryParse(_jwtTokenAccesser.GetHeader("audit-reason-id"), out var reasonId);
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");

                foreach (var dbEntry in changeTracker)
                {
                    var action = Enum.GetName(typeof(EntityState), dbEntry.State);
                    var tableName = dbEntry.CurrentValues.EntityType.ClrType.Name.ToString();

                    var recordId = (dbEntry.Entity as BaseEntity).Id;

                    if ((dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Deleted || (dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Activated)
                    {
                        var auditTrailCommon = new AuditTrailCommon
                        {
                            TableName = tableName,
                            Action = (dbEntry.Entity as BaseEntity).AuditAction.ToString(),
                            UserId = userId,
                            RecordId = recordId,
                            CreatedBy = userId,
                            UserRoleId = roleId,
                            CreatedDate = createdDate,
                            ReasonId = reasonId > 0 ? reasonId : (int?)null,
                            ReasonOth = reasonOth,
                            IpAddress = _jwtTokenAccesser.IpAddress,
                            TimeZone = _jwtTokenAccesser.GetHeader("timeZone")
                        };
                        auditTrailCommons.Add(auditTrailCommon);
                    }
                    else
                    {
                        GetOldAndNewValues(dbEntry, ref trackers, recordId);

                        trackers.ForEach(r =>
                        {
                            var auditTrailCommon = new AuditTrailCommon
                            {
                                TableName = tableName,
                                Action = action,
                                ColumnName = r.FieldName,
                                OldValue = r.OldValue,
                                NewValue = r.NewValue,
                                UserId = userId,
                                CreatedBy=userId,
                                UserRoleId = roleId,
                                RecordId = recordId,
                                CreatedDate = createdDate,
                                IpAddress = _jwtTokenAccesser.IpAddress,
                                ReasonId = reasonId > 0 ? reasonId : (int?)null,
                                ReasonOth = reasonOth,
                                TimeZone = _jwtTokenAccesser.GetHeader("timeZone")
                            };
                            auditTrailCommons.Add(auditTrailCommon);
                        });

                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
            }

            return auditTrailCommons;

        }


        void GetOldAndNewValues(EntityEntry dbEntry, ref List<TrackerResult> trackers, int recordId)
        {
            PropertyValues dbValueProps = null;



            foreach (var prop in dbEntry.Properties)
            {
                var dictionary = _dictionaryCollection.Dictionaries.Where(x =>
                    x.FieldName.ToLower() == prop.Metadata.Name.ToLower()).FirstOrDefault();

                if (dictionary == null) continue;

                if (dbValueProps == null && recordId > 0)
                    dbValueProps = dbEntry.GetDatabaseValues();

                string newValue = Convert.ToString(prop.CurrentValue);
                string oldValue = "";
                if (dbValueProps != null && dbValueProps.GetValue<object>(prop.Metadata.Name) != null)
                    oldValue = Convert.ToString(dbValueProps.GetValue<object>(prop.Metadata.Name));

                if (prop.Metadata.PropertyInfo != null)
                {
                    if (prop.Metadata.PropertyInfo.PropertyType == typeof(DateTime?) || prop.Metadata.PropertyInfo.PropertyType == typeof(DateTime))
                    {
                        if (!string.IsNullOrEmpty(oldValue)) oldValue = Convert.ToDateTime(oldValue).ToString("dd/MM/yyyy");
                        if (!string.IsNullOrEmpty(newValue)) newValue = Convert.ToDateTime(newValue).ToString("dd/MM/yyyy");
                    }

                    if (prop.Metadata.PropertyInfo.PropertyType == typeof(decimal?) || prop.Metadata.PropertyInfo.PropertyType == typeof(decimal))
                    {
                        if (!string.IsNullOrEmpty(oldValue) && !string.IsNullOrEmpty(newValue))
                        {
                            if (Convert.ToDecimal(oldValue).Equals(Convert.ToDecimal(newValue))) continue;
                        }

                    }

                    if (prop.Metadata.PropertyInfo.PropertyType == typeof(bool?) || prop.Metadata.PropertyInfo.PropertyType == typeof(bool))
                    {
                        if (!string.IsNullOrEmpty(oldValue))
                            oldValue = oldValue.ToLower() == "true" || oldValue == "1" ? "Yes" : "No";

                        if (!string.IsNullOrEmpty(newValue))
                            newValue = newValue.ToLower() == "true" || oldValue == "1" ? "Yes" : "No";
                    }
                }

                if (oldValue == newValue) continue;

                string displayName = dictionary.DisplayName;
                if (!string.IsNullOrEmpty(dictionary.SourceColumn) &&
                    !string.IsNullOrEmpty(dictionary.TableName))
                {
                    var pkName = string.IsNullOrEmpty(dictionary.PkName)
                        ? dictionary.FieldName
                        : dictionary.PkName;
                    if (!string.IsNullOrEmpty(newValue))
                    {

                        string strSql =
                            $"{"SELECT "} {dictionary.SourceColumn} {" AS Value FROM "} {dictionary.TableName} {"WHERE "} {pkName} {"="} {newValue}";
                        newValue = _gscContext.AuditValue.FromSqlRaw(strSql).Select(r => r.Value).FirstOrDefault();

                    }

                    if (!string.IsNullOrEmpty(oldValue))
                    {
                        string strSql =
                            $"{"SELECT "} {dictionary.SourceColumn} {" AS Value FROM "} {dictionary.TableName} {"WHERE "} {pkName} {"="} {oldValue}";
                        oldValue = _gscContext.AuditValue.FromSqlRaw(strSql).Select(r => r.Value).FirstOrDefault();
                    }


                }

                if (oldValue != newValue)
                {
                    if (string.IsNullOrEmpty(oldValue)) oldValue = "Empty";
                    if (string.IsNullOrEmpty(newValue)) newValue = "Empty";

                    if (!trackers.Any(x => x.EntityName == dbEntry.CurrentValues.EntityType.ClrType.Name.ToString() && x.FieldName == displayName))
                        trackers.Add(new TrackerResult
                        {
                            EntityName = dbEntry.CurrentValues.EntityType.ClrType.Name.ToString(),
                            OldValue = oldValue,
                            FieldName = displayName,
                            NewValue = newValue
                        });
                }

            }
        }

    }

   
}

