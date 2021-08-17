using System;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public sealed class RootElement : ElementItem
    {
        public ElementItem Factory { get; set; } = Invalid;

        public          string  Name       { get; set; } = string.Empty;
        public override string? Validate()
        {
            var info =  Factory.Validate();
            if (string.IsNullOrWhiteSpace(info) && string.IsNullOrWhiteSpace(Name))
                info = "Kein Name Gewählt";

            return info;
        }

        public override Condition? Build()
        {
            var result = Factory.Build();

            if (result == null) return null;
            
            return result with { Name = Name };
        }
    }
}