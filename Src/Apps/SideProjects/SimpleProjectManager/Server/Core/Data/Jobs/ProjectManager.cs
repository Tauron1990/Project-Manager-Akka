using Akkatecture.Aggregates;
using Akkatecture.Commands;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Server.Core.Data;

public class ProjectManager : AggregateManager<Project, ProjectId, Command<Project, ProjectId>>
{
    
}