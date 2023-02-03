using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Localizer.DataModel.Processing.Messages;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class BuildActorCoordinator : ObservableActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private bool _fail;

        private int _remaining;

        public BuildActorCoordinator()
        {
            Receive<BuildRequest>(obs => obs.SubscribeWithStatus(ProcessRequest));
            Receive<AgentCompled>(obs => obs.Select(SingleBuildCompled).ToSender());
        }

        private BuildCompled? SingleBuildCompled(AgentCompled arg)
        {
            _remaining--;
            if (arg.Failed)
            {
                _fail = true;
                _log.Warning(arg.Cause, "Error in Build Agent");
            }

            return _remaining == 0 ? new BuildCompled(arg.OperationId, _fail) : null;
        }

        private void ProcessRequest(BuildRequest obj)
        {
            var (idObservable, data) = obj;
            idObservable
               .Take(1)
               .ObserveOnSelf()
               .Subscribe(
                    operationId =>
                    {
                        Context.Sender.Tell(BuildMessage.GatherData(operationId));

                        _remaining = 0;
                        _fail = false;

                        var toBuild = new List<PreparedBuild>(data.Projects.Count);

                        toBuild.AddRange(
                            from project in data.Projects
                            let path = data.FindProjectPath(project)
                            where !string.IsNullOrWhiteSpace(path)
                            select new PreparedBuild(data.BuildInfo, project, data, operationId, path));

                        if (toBuild.Count == 0)
                        {
                            Context.Sender.Tell(BuildMessage.NoData(operationId));

                            return;
                        }

                        var agent = 1;
                        foreach (var preparedBuild in toBuild)
                        {
                            _remaining++;
                            Context.GetOrAdd<BuildAgent>("Agent-" + agent).Forward(preparedBuild);
                            agent++;
                        }
                    });
        }
    }
}