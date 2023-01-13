using System.Runtime.InteropServices;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.CriticalErrors;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ErrorData(bool IsOnline, CriticalErrorList Errors);