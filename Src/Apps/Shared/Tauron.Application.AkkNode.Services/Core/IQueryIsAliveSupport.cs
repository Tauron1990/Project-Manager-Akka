using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace Tauron.Application.AkkaNode.Services.Core;

public interface IQueryIsAliveSupport
{
    Task<IsAliveResponse> QueryIsAlive(ActorSystem system, TimeSpan timeout);
}