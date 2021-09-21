using Microsoft.AspNetCore.Components;

namespace Tauron.Grid.Internal
{
    public abstract class DefinitionBase : ComponentBase
    {
        [Parameter] public string? Name { get; set; }

        [Parameter] public string? Custom { get; set; }

        [Parameter] public bool IsAuto { get; set; }

        [CascadingParameter] public TauGrid? Grid { get; set; }

        protected internal abstract string PropertyName { get; }

        protected abstract Lenght Value { get; }

        protected override void OnParametersSet()
        {
            Grid?.RegisterDefinition(this);
            base.OnParametersSet();
        }

        internal string Render()
        {
            if (IsAuto) return "auto";

            return string.IsNullOrWhiteSpace(Custom)
                ? string.IsNullOrWhiteSpace(Name)
                    ? Value.ToString()
                    : $"[{Name}] {Value}"
                : Custom;
        }
    }
}