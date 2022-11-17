using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

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

    public IList<IFormFile> Files
    {
        get;
        [UsedImplicitly]
        set;
    } = new List<IFormFile>();
}