using JetBrains.Annotations;

namespace Tauron.Application.Workshop.Analyzing;

[PublicAPI]
public sealed class Issue
{
    private Issue(string issueType, object? data, string project, string ruleName)
    {
        IssueType = issueType;
        Data = data;
        Project = project;
        RuleName = ruleName;
    }

    public string RuleName { get; }

    public string IssueType { get; }

    public string Project { get; }

    public object? Data { get; }

    public static IssueCompleter New(string type) => new(type, string.Empty, null);

    public sealed class IssueCompleter
    {
        private readonly object? _data;
        private readonly string _project;
        private readonly string _type;

        public IssueCompleter(string type, string project, object? data)
        {
            _type = type;
            _project = project;
            _data = data;
        }

        public Issue Build(string ruleName) => new(_type, _data, _project, ruleName);
    }
}