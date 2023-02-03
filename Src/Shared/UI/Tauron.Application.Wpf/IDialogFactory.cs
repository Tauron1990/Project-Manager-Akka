using System;
using System.Reactive;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf;
#pragma warning disable CA1069 // Enumerationswerte dürfen nicht dupliziert werden
#pragma warning restore CA1069 // Enumerationswerte dürfen nicht dupliziert werden

[PublicAPI]
public interface IDialogFactory
{
    IObservable<Unit> FormatException(System.Windows.Window? owner, Exception exception);

    IObservable<MsgBoxResult> ShowMessageBox(
        System.Windows.Window? owner, string text, string caption,
        MsgBoxButton button, MsgBoxImage icon);


    IObservable<string[]?> ShowOpenFileDialog(
        Window? owner,
        bool checkFileExists, string defaultExt,
        bool dereferenceLinks, string filter,
        bool multiSelect, string title,
        bool validateNames,
        bool checkPathExists);


    IObservable<string?> ShowOpenFolderDialog(
        System.Windows.Window? owner, string description,
        Environment.SpecialFolder rootFolder, bool showNewFolderButton, bool useDescriptionForTitle);

    IObservable<string?> ShowOpenFolderDialog(
        System.Windows.Window? owner, string description, string rootFolder,
        bool showNewFolderButton, bool useDescriptionForTitle);


    IObservable<string?> ShowSaveFileDialog(
        System.Windows.Window? owner, bool addExtension, bool checkFileExists,
        bool checkPathExists, string defaultExt, bool dereferenceLinks, string filter,
        bool createPrompt, bool overwritePrompt, string title, string initialDirectory);
}