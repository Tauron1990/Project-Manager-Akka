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

        public interface IHostApiCommand
        {
            CommandType Type { get; }

            string Target { get; }

            OperationResponse CreateDefaultFailed();
        }

        //public interface IMarkFailed<out TType>
        //{
        //    TType Failed();
        //}

        public abstract record CommandBase<TResult>(string Target, [property: JsonIgnore] CommandType Type) : IHostApiCommand
            where TResult : OperationResponse, new()
        {
            OperationResponse IHostApiCommand.CreateDefaultFailed() 
                => new TResult();
        }

        public sealed record GetHostName;

        public sealed record GetHostNameResult(string Name);
    }
}