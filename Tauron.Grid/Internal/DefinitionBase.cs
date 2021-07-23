using Microsoft.AspNetCore.Components;

namespace Tauron.Grid.Internal
{
    public abstract class DefinitionBase : ComponentBase
    {
        [Parameter]
        public CSSUnit Unit { get; set; } = CSSUnit.Fraction;

        [Parameter]
        public string? Name { get; set; }

        [Parameter]
        public string? Custom { get; set; }

        [Parameter]
        public 

        [CascadingParameter]
        public TauGrid? Grid { get; set; }

        protected internal abstract string PropertyName { get; }

        protected abstract int Value { get; }

        protected override void OnParametersSet()
        {
            Grid?.RegisterDefinition(this);
            base.OnParametersSet();
        }

        internal string Render()
            => string.IsNullOrWhiteSpace(Custom)
                ? string.IsNullOrWhiteSpace(Name)
                    ? Converter.ToCss(Value, Unit)
                    : $"[{Name}] {Converter.ToCss(Value, Unit)}"
                : Custom;
    }
}