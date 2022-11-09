using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace SimpleProjectManager.Server.Controllers;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public class UploadFiles
{
    public string JobName
    {
        get;
        [UsedImplicitly]
        set;
    } = string.Empty;

    public List<IFormFile> Files
    {
        get;
        [UsedImplicitly]
        set;
    } = new();
}