﻿using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.UIModels.Services.Data;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class InitProjectViewModel
    {
        public Project Project { get; }

        public ProjectFileWorkspace Workspace { get; }

        public InitProjectViewModel(Project project, ProjectFileWorkspace workspace)
        {
            Project = project;
            Workspace = workspace;
        }
    }
}