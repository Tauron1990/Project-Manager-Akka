﻿using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services.Tasks;

public interface ITaskManager
{
    [ComputeMethod(KeepAliveTime = 5)]
    Task<PendingTask[]> GetTasks(CancellationToken token);

    Task<string> DeleteTask(string id, CancellationToken token);
}