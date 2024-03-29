﻿using Akkatecture.Core;

namespace SimpleProjectManager.Shared;

public sealed class ProjectFileId : Identity<ProjectFileId>
{
    public static readonly Guid FileNamespace = Guid.Parse("5124368E-157D-44FE-AFFB-EB04B0F59825");

    public ProjectFileId(string value) : base(value) { }

    public static ProjectFileId For(ProjectName projectNumber, FileName name)
        => NewDeterministic(FileNamespace, $"{projectNumber.Value}--{name.Value}");
}