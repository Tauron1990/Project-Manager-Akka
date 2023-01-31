using System.Runtime.InteropServices;

namespace SimpleProjectManager.Client.Operations.Shared;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct NameState(ObjectName Name, NameClientState ClientState);