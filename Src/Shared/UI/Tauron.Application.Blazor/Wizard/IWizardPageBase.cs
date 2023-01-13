using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tauron.Application.Blazor.Wizard;

public interface IWizardPageBase
{
    bool ShowControls { get; set; }

    string Title { get; }

    IEnumerable<(string Label, Func<Task> Handler)> CustomActions { get; }

    Task<bool> NeedRender(WizardContextBase context);

    Task<Type> Init(WizardContextBase context, CancellationToken token);

    Task<string?> VerifyNext(WizardContextBase context, CancellationToken token);

    Task BeforeNext(WizardContextBase context);
}