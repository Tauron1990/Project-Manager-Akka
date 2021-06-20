using System;
using System.Collections.Generic;
using System.Linq;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceHost.Client.Shared.ConfigurationServer
{
    public static class ConditionChecker
    {
        public static bool MeetCondition(string softwareName, string applicationName, SpecificConfig entity)
        {
            bool Apply(Condition condition, Func<bool> isMeet)
                => condition.Excluding
                    ? !isMeet()
                    : isMeet();

            bool CheckConditions(IEnumerable<Condition> conditions, bool isOr)
            {
                bool Predicate(Condition c)
                    => c switch
                    {
                        AppCondition app => Apply(c, () => softwareName == app.AppName),
                        InstalledAppCondition installedApp => Apply(c, () => applicationName == installedApp.AppName),
                        AndCondition andCondition => Apply(c, () => CheckConditions(andCondition.Conditions, false)),
                        OrCondition orCondition => Apply(c, () => CheckConditions(orCondition.Conditions, true)),
                        _ => false
                    };

                return isOr 
                    ? conditions.OrderBy(c => c.Order).Any(Predicate) 
                    : conditions.OrderBy(c => c.Order).All(Predicate);
            }

            return entity.Conditions.IsEmpty
                || CheckConditions(entity.Conditions, false);
        }
    }
}