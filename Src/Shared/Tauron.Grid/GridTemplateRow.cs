using Microsoft.AspNetCore.Components;

namespace Tauron.Grid
{
    public sealed class GridTemplateRow : ComponentBase
    {
        [Parameter] public string Def { get; set; } = "none";

        [CascadingParameter] public TauGrid? TauGrid { get; set; }

        protected override void OnParametersSet()
        {
            TauGrid?.RegisterTemplate(this);
            base.OnParametersSet();
        }
    }
}