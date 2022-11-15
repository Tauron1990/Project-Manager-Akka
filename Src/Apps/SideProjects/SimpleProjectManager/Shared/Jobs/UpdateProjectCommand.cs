using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

public sealed record UpdateProjectCommand(ProjectId Id, ProjectName? Name, ProjectStatus? Status, NullableData<ProjectDeadline?>? Deadline)
{
    public static UpdateProjectCommand Create(JobData newData, JobData oldData)
    {
        ProjectName? newName = newData.JobName == oldData.JobName ? null : newData.JobName;
        ProjectStatus? newStatus = newData.Status == oldData.Status ? null : newData.Status;
        var newDeadline = newData.Deadline == oldData.Deadline ? null : new NullableData<ProjectDeadline?>(newData.Deadline);

        return new UpdateProjectCommand(newData.Id, newName, newStatus, newDeadline);
    }
}