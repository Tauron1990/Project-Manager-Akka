using Microsoft.AspNetCore.Components.Forms;

namespace SimpleProjectManager.Client.Shared.EditJob;

public partial class FileUploader
{
    private string? _dragEnterStyle;
    private IList<string> _fileNames = new List<string>();
    
    void OnInputFileChanged(InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        _fileNames = files.Select(f => f.Name).ToList();
    }
    
    void Upload()
    {
    }
}