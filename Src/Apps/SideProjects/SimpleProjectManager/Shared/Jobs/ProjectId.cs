﻿using Akkatecture.Core;

namespace SimpleProjectManager.Shared;

public sealed class ProjectId : Identity<ProjectId>
{
    public static readonly ProjectId Empty;

    public static readonly Guid JobsNameSpace = Guid.Parse("122C375D-7EB3-457F-98D3-D3125E784B34");

    static ProjectId()
        => Empty = For(new ProjectName("Empty"));

    public ProjectId(string value)
        : base(value) { }

    public static ProjectId For(ProjectName projectNumber)
        => NewDeterministic(JobsNameSpace, projectNumber.Value);
}