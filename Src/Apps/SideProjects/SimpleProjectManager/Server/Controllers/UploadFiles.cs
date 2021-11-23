using JetBrains.Annotations;

namespace SimpleProjectManager.Server.Controllers;


public class UploadFiles
{
    public string JobName { get; [UsedImplicitly]set; } = string.Empty;

    public List<IFormFile> Files { get; [UsedImplicitly]set; } = new();
}