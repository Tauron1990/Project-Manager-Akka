using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Blazor.Wizard;

public abstract class WizardContext<TData> : WizardContextBase
{
    protected WizardContext(TData data, ImmutableList<IWizardPageBase> pages) : base(pages)
        => Data = data;

    [PublicAPI]
    public TData Data { get; set; }
}