using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Blazor;
using Stl.Fusion.Client;
using Stl.Fusion.Extensions;
using TestWebApplication.Client.Services;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var baseUri = new Uri(builder.HostEnvironment.BaseAddress);
            var apiBaseUri = new Uri($"{baseUri}api/");

            Console.WriteLine(apiBaseUri);

            builder.Logging.AddProvider(new ConcoleLogger());

            builder.Services.AddFusion()
               .AddFusionTime()
               .AddRestEaseClient(
                    b => b.AddReplicaService<ICounterService, ICounterServiceDef>()
                       .ConfigureHttpClientFactory(
                            (_, name, o) =>
                            {
                                var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                                var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                                o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
                            }),
                    (_, o) => o.BaseUri = baseUri)
               .AddBlazorUIServices();

            builder.Services.AddSingleton<BlazorModeHelper>();

            builder.Services.AddSingleton(
                sp =>
                    sp.GetRequiredService<IStateFactory>()
                       .NewMutable<string>(Guid.NewGuid().ToString("D")));

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            var host = builder.Build();
            await host.Services.HostedServices().Start();
            await host.RunAsync();
        }

        private sealed class ConcoleLogger : ILoggerProvider
        {
            public void Dispose() { }

            public ILogger CreateLogger(string categoryName)
                => new ConsoleLogger(categoryName);

            private class SimpleConsoleFormatter : ConsoleFormatter
            {
                private const string LoglevelPadding = ": ";
                private static readonly string MessagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
                private static readonly string NewLineWithMessagePadding = Environment.NewLine + MessagePadding;

                public SimpleConsoleFormatter(SimpleConsoleFormatterOptions options)
                    : base(ConsoleFormatterNames.Simple)
                {
                    ReloadLoggerOptions(options);
                }

                private SimpleConsoleFormatterOptions FormatterOptions { get; set; }

                private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
                {
                    FormatterOptions = options;
                }

                public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
                {
                    var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

                    if (logEntry.Exception == null && message == null) return;

                    var logLevel = logEntry.LogLevel;
                    var logLevelString = GetLogLevelString(logLevel);

                    string timestamp = null;
                    var timestampFormat = FormatterOptions.TimestampFormat;
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (timestampFormat is not null)
                    {
                        var dateTimeOffset = GetCurrentDateTime();
                        timestamp = dateTimeOffset.ToString(timestampFormat);
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (timestamp is not null) textWriter.Write(timestamp);
                    if (logLevelString != null) textWriter.Write(logLevelString);
                    CreateDefaultLogMessage(textWriter, logEntry, message, scopeProvider);
                }

                private void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message, IExternalScopeProvider scopeProvider)
                {
                    var singleLine = FormatterOptions.SingleLine;
                    var eventId = logEntry.EventId.Id;
                    var exception = logEntry.Exception;

                    // Example:
                    // info: ConsoleApp.Program[10]
                    //       Request received

                    // category and event id
                    textWriter.Write(LoglevelPadding + logEntry.Category + '[' + eventId + "]");
                    if (!singleLine) textWriter.Write(Environment.NewLine);

                    // scope information
                    WriteScopeInformation(textWriter, scopeProvider, singleLine);
                    WriteMessage(textWriter, message, singleLine);

                    // Example:
                    // System.InvalidOperationException
                    //    at Namespace.Class.Function() in File:line X
                    if (exception != null)
                        // exception message
                        WriteMessage(textWriter, exception.ToString(), singleLine);
                    if (singleLine) textWriter.Write(Environment.NewLine);
                }

                private void WriteMessage(TextWriter textWriter, string message, bool singleLine)
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        if (singleLine)
                        {
                            textWriter.Write(' ');
                            WriteReplacing(textWriter, Environment.NewLine, " ", message);
                        }
                        else
                        {
                            textWriter.Write(MessagePadding);
                            WriteReplacing(textWriter, Environment.NewLine, NewLineWithMessagePadding, message);
                            textWriter.Write(Environment.NewLine);
                        }
                    }

                    static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
                    {
                        var newMessage = message.Replace(oldValue, newValue);
                        writer.Write(newMessage);
                    }
                }

                private DateTimeOffset GetCurrentDateTime()
                    => FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;

                private static string GetLogLevelString(LogLevel logLevel)
                {
                    return logLevel switch
                    {
                        LogLevel.Trace => "trce",
                        LogLevel.Debug => "dbug",
                        LogLevel.Information => "info",
                        LogLevel.Warning => "warn",
                        LogLevel.Error => "fail",
                        LogLevel.Critical => "crit",
                        _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
                    };
                }

                private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider scopeProvider, bool singleLine)
                {
                    if (FormatterOptions.IncludeScopes && scopeProvider != null)
                    {
                        var paddingNeeded = !singleLine;
                        scopeProvider.ForEachScope(
                            (scope, state) =>
                            {
                                if (paddingNeeded)
                                {
                                    paddingNeeded = false;
                                    state.Write(MessagePadding + "=> ");
                                }
                                else
                                {
                                    state.Write(" => ");
                                }

                                state.Write(scope);
                            },
                            textWriter);

                        if (!paddingNeeded && !singleLine) textWriter.Write(Environment.NewLine);
                    }
                }
            }

            private class ConsoleLogger : ILogger
            {
                [ThreadStatic] private static StringWriter _tStringWriter;

                private readonly string _name;

                internal ConsoleLogger(string name)
                {
                    _name = name ?? throw new ArgumentNullException(nameof(name));
                    Formatter = new SimpleConsoleFormatter(new SimpleConsoleFormatterOptions());
                    ScopeProvider = new LoggerExternalScopeProvider();
                }

                private ConsoleFormatter Formatter { get; }
                private IExternalScopeProvider ScopeProvider { get; }

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (!IsEnabled(logLevel)) return;

                    if (formatter == null) throw new ArgumentNullException(nameof(formatter));

                    _tStringWriter ??= new StringWriter();
                    var logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
                    Formatter.Write(in logEntry, ScopeProvider, _tStringWriter);

                    var sb = _tStringWriter.GetStringBuilder();

                    if (sb.Length == 0) return;

                    var computedAnsiString = sb.ToString();
                    sb.Clear();
                    if (sb.Capacity > 1024) sb.Capacity = 1024;

                    Console.WriteLine(computedAnsiString);
                }

                public bool IsEnabled(LogLevel logLevel)
                    => logLevel != LogLevel.None;

                public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state);
            }
        }
    }
}