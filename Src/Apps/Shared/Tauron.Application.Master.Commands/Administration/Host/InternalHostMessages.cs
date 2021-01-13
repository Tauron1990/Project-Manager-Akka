using Newtonsoft.Json;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public static class InternalHostMessages
    {
        public enum CommandType
        {
            AppManager,
            AppRegistry,
            Installer
        }

        public abstract record CommandBase(string Target, [property:JsonIgnore] CommandType Type);

        public sealed record GetHostName;

        public sealed record GetHostNameResult(string Name);
    }
}