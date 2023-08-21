using System.Runtime.Serialization;
using Akkatecture.Core;
using MemoryPack;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable]
public sealed partial class ProjectId : Identity<ProjectId>
{
// #pragma warning disable GU0023
//     public static readonly ProjectId Empty = For(new ProjectName("Empty"));
// #pragma warning restore GU0023

    public static readonly Guid JobsNameSpace = Guid.Parse("122C375D-7EB3-457F-98D3-D3125E784B34");


    [MemoryPackConstructor]
    public ProjectId(string value)
        : base(value) { }

    public static ProjectId For(ProjectName projectNumber)
        => NewDeterministic(JobsNameSpace, projectNumber.Value);
}