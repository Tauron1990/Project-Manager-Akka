using Akkatecture.Core;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class ProjectId : Identity<ProjectId>
{
    #pragma warning disable CS0618
    public static ProjectId Empty = new();
    #pragma warning restore CS0618
    
    public static readonly Guid JobsNameSpace = Guid.Parse("122C375D-7EB3-457F-98D3-D3125E784B34");

    public ProjectId(string value)
        : base(value) { }
    
    [UsedImplicitly, Obsolete("Used for Serialization")]
    public ProjectId()
    {
        
    }

    public static ProjectId For(ProjectName projectNumber)
        => NewDeterministic(JobsNameSpace, projectNumber.Value);
}