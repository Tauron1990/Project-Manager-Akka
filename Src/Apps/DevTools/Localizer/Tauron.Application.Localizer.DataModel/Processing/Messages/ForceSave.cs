namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record ForceSave(bool AndSeal, ProjectFile File)
    {
        public static ForceSave Force(ProjectFile file) => new(false, file);

        public static ForceSave Seal(ProjectFile file) => new(true, file);
    }
}