﻿using System;
using System.Threading.Tasks;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.JobEdit;

public sealed record JobEditorPair<TData>(TData NewData, TData? OldData);

public sealed record JobEditorCommit(JobEditorPair<JobData> JobData, Func<Task<string>> Upload);