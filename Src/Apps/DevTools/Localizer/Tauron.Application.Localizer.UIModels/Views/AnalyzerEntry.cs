namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed record AnalyzerEntry(string RuleName, string ErrorName, string project, string Message)
    {
        public sealed class Builder
        {
            private readonly string _project;
            private readonly string _ruleName;

            public Builder(string ruleName, string project)
            {
                _ruleName = ruleName;
                _project = project;
            }

            public AnalyzerEntry Entry(string errorName, string message) => new(_ruleName, _project, message, errorName);
        }
    }
}