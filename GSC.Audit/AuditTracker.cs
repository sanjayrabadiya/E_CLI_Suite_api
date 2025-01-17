﻿
using GSC.Common;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
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

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IDictionaryCollection _dictionaryCollection;
        public AuditTracker(IDictionaryCollection dictionaryCollection,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _dictionaryCollection = dictionaryCollection;
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public List<AuditTrail> GetAuditTracker(IList<EntityEntry> entities, DbContext context)
        {
            List<TrackerResult> trackers = new List<TrackerResult>();
            var auditTrailCommons = new List<AuditTrail>();
            try
            {
                var userId = _jwtTokenAccesser.UserId;
                var clientDate = _jwtTokenAccesser.GetClientDate();
         
                var changeTracker = entities.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).ToList();


                var reason = _jwtTokenAccesser.GetHeader("audit-reason-name");
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");

                foreach (var dbEntry in changeTracker)
                {
                    var action = Enum.GetName(typeof(EntityState), dbEntry.State);
                    var tableName = dbEntry.CurrentValues.EntityType.ClrType.Name.ToString();

                    var recordId = (dbEntry.Entity as BaseEntity).Id;

                    if ((dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Deleted || (dbEntry.Entity as BaseEntity).AuditAction == AuditAction.Activated)
                    {
                        var auditTrail = new AuditTrail
                        {
                            TableName = tableName,
                            Action = (dbEntry.Entity as BaseEntity).AuditAction.ToString(),
                            UserId = userId,
                            RecordId = recordId,
                            CreatedBy = userId,
                            UserRole = _jwtTokenAccesser.RoleName,
                            CreatedDate = clientDate,
                            Reason = reason,
                            ReasonOth = reasonOth,
                            IpAddress = _jwtTokenAccesser.IpAddress,
                            TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                        };
                        auditTrailCommons.Add(auditTrail);
                    }
                    else
                    {
                        trackers = new List<TrackerResult>();
                        GetOldAndNewValues(dbEntry, ref trackers, recordId, context);

                        trackers.ForEach(r =>
                        {
                            var auditTrail = new AuditTrail
                            {
                                TableName = tableName,
                                Action = action,
                                ColumnName = r.FieldName,
                                OldValue = r.OldValue,
                                NewValue = r.NewValue,
                                UserId = userId,
                                CreatedBy = userId,
                                UserRole = _jwtTokenAccesser.RoleName,
                                RecordId = recordId,
                                CreatedDate = clientDate,
                                IpAddress = _jwtTokenAccesser.IpAddress,
                                Reason = reason,
                                ReasonOth = reasonOth,
                                TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone"),
                                Entity = (dbEntry.Entity as BaseEntity)
                            };
                            auditTrailCommons.Add(auditTrail);
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


        void GetOldAndNewValues(EntityEntry dbEntry, ref List<TrackerResult> trackers, int recordId, DbContext context)
        {
            PropertyValues dbValueProps = null;

            foreach (var prop in dbEntry.Properties)
            {
                var dictionary = _dictionaryCollection.Dictionaries.FirstOrDefault(x =>
                    x.FieldName.ToLower() == prop.Metadata.Name.ToLower());

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
                        if (string.IsNullOrEmpty(oldValue) || string.IsNullOrEmpty(newValue) || !Convert.ToDecimal(oldValue).Equals(Convert.ToDecimal(newValue)))
                        {
                            break;
                        }
                        else
                        {
                            continue;
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

                        string strSql = $"{"SELECT "} {dictionary.SourceColumn} {" AS Value FROM "} {dictionary.TableName} {"WHERE "} {pkName} {"="} {newValue}";
                        newValue = context.Set<AuditValue>().FromSqlRaw(strSql, "").AsEnumerable().Select(r => r.Value).FirstOrDefault();

                    }

                    if (!string.IsNullOrEmpty(oldValue))
                    {
                        string strSql = $"{"SELECT "} {dictionary.SourceColumn} {" AS Value FROM "} {dictionary.TableName} {"WHERE "} {pkName} {"="} {oldValue}";
                        oldValue = context.Set<AuditValue>().FromSqlRaw(strSql, "").AsEnumerable().Select(r => r.Value).FirstOrDefault();
                    }


                }

                if (oldValue != newValue)
                {
                    if (string.IsNullOrEmpty(oldValue)) oldValue = "Empty";
                    if (string.IsNullOrEmpty(newValue)) newValue = "Empty";
                    var tableName = dbEntry.CurrentValues.EntityType.ClrType.Name.ToString().Trim();
                    if (!trackers.Exists(x => x.EntityName == tableName && x.FieldName == displayName.Trim()))
                        trackers.Add(new TrackerResult
                        {
                            EntityName = tableName,
                            OldValue = oldValue,
                            FieldName = displayName.Trim(),
                            NewValue = newValue
                        });
                }

            }
        }

    }


}

