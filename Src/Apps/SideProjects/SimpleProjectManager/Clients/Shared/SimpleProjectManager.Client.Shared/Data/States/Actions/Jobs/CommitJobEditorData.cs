using System;
using SimpleProjectManager.Client.Shared.Data.JobEdit;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

public record CommitJobEditorData(JobEditorCommit Commit, Action<bool> OnCompled);