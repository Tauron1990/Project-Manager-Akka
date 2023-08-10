using System.Runtime.Serialization;
using MemoryPack;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Shared;

[DataContract, MemoryPackable(GenerateType.VersionTolerant)]
public sealed partial record UpdateProjectCommand(
    [property:DataMember, MemoryPackOrder(0)]ProjectId Id,
    [property:DataMember, MemoryPackOrder(1)]ProjectName? Name, 
    [property:DataMember, MemoryPackOrder(2)]ProjectStatus? Status, 
    [property:DataMember, MemoryPackOrder(3)]NullableData<ProjectDeadline?>? Deadline)
{
    public static UpdateProjectCommand Create(JobData newData, JobData oldData)
    {
        ProjectName? newName = newData.JobName == oldData.JobName ? null : newData.JobName;
        ProjectStatus? newStatus = newData.Status == oldData.Status ? null : newData.Status;
        var newDeadline = newData.Deadline == oldData.Deadline ? null : new NullableData<ProjectDeadline?>(newData.Deadline);

        return new UpdateProjectCommand(newData.Id, newName, newStatus, newDeadline);
    }
}