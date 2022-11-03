using Akkatecture.Core;

namespace SimpleProjectManager.Shared.Services;

public sealed class ErrorId : Identity<ErrorId>
{
    public ErrorId(string value) : base(value) { }
}