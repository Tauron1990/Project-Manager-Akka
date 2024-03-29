﻿namespace SimpleProjectManager.Shared.Services;

public record JobInfo(ProjectId Project, ProjectName Name, ProjectDeadline? Deadline, ProjectStatus Status, bool FilesPresent);