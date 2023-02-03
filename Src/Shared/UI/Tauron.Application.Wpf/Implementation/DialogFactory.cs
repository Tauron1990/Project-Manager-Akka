using System;
using System.Reactive;
using System.Windows;
using Ookii.Dialogs.Wpf;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Wpf.AppCore;

namespace Tauron.Application.Wpf.Implementation;

public sealed class DialogFactory : IDialogFactory
{
    private readonly System.Windows.Window _mainWindow;

    public DialogFactory(IUIDispatcher currentDispatcher, IMainWindow mainWindow)
    {
        _mainWindow = ((WpfWindow)mainWindow.Window).Window;
        CurrentDispatcher = currentDispatcher;
    }

    private IUIDispatcher CurrentDispatcher { get; }

    public IObservable<Unit> FormatException(System.Windows.Window? owner, Exception exception)
        => ShowMessageBox(
            owner,
            $"Type: {exception.GetType().Name} \n {exception.Message}",
            "Error",
            MsgBoxButton.Ok,
            MsgBoxImage.Error).ToUnit();

    public IObservable<MsgBoxResult> ShowMessageBox(
        System.Windows.Window? owner, string text, string caption,
        MsgBoxButton button, MsgBoxImage icon)
    {
        return CurrentDispatcher.InvokeAsync(
            () => (MsgBoxResult)MessageBox.Show(
                owner ?? _mainWindow,
                text,
                caption,
                (MessageBoxButton)button,
                (MessageBoxImage)icon));
    }

    public IObservable<string[]?> ShowOpenFileDialog(
        Window? owner, bool checkFileExists, string defaultExt,
        bool dereferenceLinks, string filter,
        bool multiSelect, string title, bool validateNames, bool checkPathExists)
    {
        return CurrentDispatcher.InvokeAsync(
            () =>
            {
                var dialog = new VistaOpenFileDialog
                             {
                                 CheckFileExists = checkFileExists,
                                 DefaultExt = defaultExt,
                                 DereferenceLinks =
                                     dereferenceLinks,
                                 Filter = filter,
                                 Multiselect = multiSelect,
                                 Title = title,
                                 ValidateNames = validateNames,
                                 CheckPathExists = checkPathExists
                             };

                TranslateDefaultExt(dialog);

                bool? tempresult = owner != null
                    ? dialog.ShowDialog(owner)
                    : dialog.ShowDialog(_mainWindow);

                return tempresult == false
                    ? null
                    : dialog.FileNames;
            });
    }

    public IObservable<string?> ShowOpenFolderDialog(
        System.Windows.Window? owner, string description,
        Environment.SpecialFolder rootFolder, bool showNewFolderButton,
        bool useDescriptionForTitle)
    {
        return CurrentDispatcher.InvokeAsync(
            () =>
            {
                var dialog = new VistaFolderBrowserDialog
                             {
                                 Description = description,
                                 RootFolder = rootFolder,
                                 ShowNewFolderButton = showNewFolderButton,
                                 UseDescriptionForTitle = useDescriptionForTitle
                             };

                bool? tempresult = owner != null
                    ? dialog.ShowDialog(owner)
                    : dialog.ShowDialog(_mainWindow);

                return tempresult == false
                    ? null
                    : dialog.SelectedPath;
            });
    }

    public IObservable<string?> ShowOpenFolderDialog(
        System.Windows.Window? owner, string description,
        string rootFolder, bool showNewFolderButton, bool useDescriptionForTitle)
    {
        return CurrentDispatcher.InvokeAsync(
            () =>
            {
                var dialog = new VistaFolderBrowserDialog
                             {
                                 Description = description,
                                 SelectedPath = rootFolder,
                                 ShowNewFolderButton = showNewFolderButton,
                                 UseDescriptionForTitle = useDescriptionForTitle
                             };

                bool? tempresult = owner != null
                    ? dialog.ShowDialog(owner)
                    : dialog.ShowDialog(_mainWindow);

                return tempresult == false
                    ? null
                    : dialog.SelectedPath;
            });
    }

    public IObservable<string?> ShowSaveFileDialog(
        System.Windows.Window? owner, bool addExtension,
        bool checkFileExists, bool checkPathExists, string defaultExt, bool dereferenceLinks, string filter,
        bool createPrompt, bool overwritePrompt, string title, string initialDirectory)
    {
        return CurrentDispatcher.InvokeAsync(
            () =>
            {
                var dialog = new VistaSaveFileDialog
                             {
                                 AddExtension = addExtension,
                                 CheckFileExists = checkFileExists,
                                 DefaultExt = defaultExt,
                                 DereferenceLinks = dereferenceLinks,
                                 Filter = filter,
                                 Title = title,
                                 CheckPathExists = checkPathExists,
                                 CreatePrompt = createPrompt,
                                 OverwritePrompt = overwritePrompt,
                                 InitialDirectory = initialDirectory
                             };

                TranslateDefaultExt(dialog);

                bool? tempresult = owner != null
                    ? dialog.ShowDialog(owner)
                    : dialog.ShowDialog(_mainWindow);

                return tempresult == false ? null : dialog.FileName;
            });
    }

    private static void TranslateDefaultExt(VistaFileDialog dialog)
    {
        if(string.IsNullOrWhiteSpace(dialog.DefaultExt)) return;

        string ext = "*." + dialog.DefaultExt;
        string? filter = dialog.Filter;
        string[] filters = filter.Split('|');
        for (var i = 1; i < filters.Length; i += 2)
            if(string.Equals(filters[i], ext, StringComparison.Ordinal))
                dialog.FilterIndex = 1 + (i - 1) / 2;
    }
}