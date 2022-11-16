using System;
using System.Threading.Tasks;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.Data.JobEdit;

public sealed record JobEditorCommit(JobEditorPair<JobData> JobData, Func<Task<SimpleResult>> Upload);