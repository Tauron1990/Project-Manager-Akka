namespace Tauron.Application.Localizer.DataModel.Processing.Messages
{
    public sealed record ForceSave(bool AndSeal, ProjectFile File)
    {
        public static ForceSave Force(ProjectFile file) => new(AndSeal: false, file);

        public static ForceSave Seal(ProjectFile file) => new(AndSeal: true, file);
    }
}