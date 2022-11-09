using System;
using System.Collections.Generic;
using System.Linq;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceHost.Client.Shared.ConfigurationServer;

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
                => c.Type switch
                {
                    ConditionType.DefinedApp => Apply(c, () => softwareName == c.AppName),
                    ConditionType.InstalledApp => Apply(c, () => applicationName == c.AppName),
                    ConditionType.And when c.Conditions != null => Apply(c, () => CheckConditions(c.Conditions, false)),
                    ConditionType.Or when c.Conditions != null => Apply(c, () => CheckConditions(c.Conditions, true)),
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