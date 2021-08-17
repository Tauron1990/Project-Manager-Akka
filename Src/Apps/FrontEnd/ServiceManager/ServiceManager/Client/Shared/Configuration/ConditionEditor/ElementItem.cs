using System;
using System.Collections.Immutable;
using System.Linq;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    
    
    public sealed class AndElement : ElementItem
    {
        public ImmutableList<ElementItem> Items { get; set; }

        public override string? Validate()
            => Items.Count < 2 
                   ? "Zu wenig Elemenete für Und Bedingung" 
                   : Items.Select(e => e.Validate()).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        public override Condition? Build()
            => new(GetName(), Excluding, null, ConditionType.And, Items.Select(c => c.Build()).ToImmutableList()!, Order);
    }

    public sealed class InvalidItem : ElementItem
    {
       
        public override string Validate()
            => "Kein Element Vorhanden";

        public override Condition? Build()
            => null;
    }

    public abstract class ElementItem
    {
        public static readonly ElementItem Invalid = new InvalidItem();

        public bool Excluding { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }
        
        public abstract string? Validate();
        
        public abstract Condition? Build();

        protected string GetName()
            => string.IsNullOrWhiteSpace(Name)
                   ? $"ID-{Guid.NewGuid():D}"
                   : Name;
    }
}