using AnyConsole;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Parsing;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    public sealed class AnyConsoleSink : ILogEventSink
    {
        private readonly ExtendedConsole _console;
        private readonly MessageTemplate _template;

        public AnyConsoleSink(ExtendedConsole console)
        {
            _console = console;
            _template = new MessageTemplateParser().Parse("[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        public void Emit(LogEvent logEvent)
        {
            new MessageTemplate()
            logEvent.MessageTemplate.Render(logEvent.Properties);
        }
    }
}