using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SimpleProjectManager.Client.Avalonia.ViewModels.CriticalErrors;

namespace SimpleProjectManager.Client.Avalonia.Views.CriticalErrors;

public partial class CriticalErrorDisplay : ReactiveUserControl<ErrorViewModel>
{
    public CriticalErrorDisplay()
    {
        InitializeComponent();

        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        if(ViewModel is null) yield break;

        yield return this.OneWayBind(ViewModel, m => m.Item.Id, v => v.IdBlock.Text);
        yield return ViewModel.WhenAny(m => m.Item, c => c.Value).Select(d => d.Occurrence.ToString("G")).BindTo(this, v => v.OccurenceBlock.Text);
        yield return this.OneWayBind(ViewModel, m => m.Item.Message, v => v.MessageBlock.Text);
        yield return this.OneWayBind(ViewModel, m => m.Item.ApplicationPart, v => v.PartBlock.Text);

        yield return this.BindCommand(ViewModel, m => m.Hide, v => v.HideCommand.Command);
    }
}