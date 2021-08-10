using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
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
                              .ConfigureHttpClientFactory((c, name, o) =>
                                                          {
                                                              var isFusionClient = (name ?? "").StartsWith("Stl.Fusion");
                                                              var clientBaseUri = isFusionClient ? baseUri : apiBaseUri;
                                                              o.HttpClientActions.Add(client => client.BaseAddress = clientBaseUri);
                                                          }),
                        (p, o) => o.BaseUri = baseUri)
                   .AddBlazorUIServices();

            builder.Services.AddSingleton<BlazorModeHelper>();

            builder.Services.AddSingleton(sp =>
                                              sp.GetRequiredService<IStateFactory>()
                                                .NewMutable<string>(Guid.NewGuid().ToString("D")));

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            var host = builder.Build();
            await host.Services.HostedServices().Start();
            await host.RunAsync();
        }

        private sealed class ConcoleLogger : ILoggerProvider
        {
            internal class SimpleConsoleFormatter : ConsoleFormatter
            {
                private const string LoglevelPadding = ": ";
                private static readonly string _messagePadding = new string(' ', GetLogLevelString(LogLevel.Information).Length + LoglevelPadding.Length);
                private static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

                public SimpleConsoleFormatter(SimpleConsoleFormatterOptions options)
                    : base(ConsoleFormatterNames.Simple)
                {
                    ReloadLoggerOptions(options);
                }

                private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
                {
                    FormatterOptions = options;
                }

                internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

                public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
                {
                    string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
                    if (logEntry.Exception == null && message == null)
                    {
                        return;
                    }
                    LogLevel logLevel = logEntry.LogLevel;
                    string logLevelString = GetLogLevelString(logLevel);

                    string timestamp = null;
                    string timestampFormat = FormatterOptions.TimestampFormat;
                    if (timestampFormat != null)
                    {
                        DateTimeOffset dateTimeOffset = GetCurrentDateTime();
                        timestamp = dateTimeOffset.ToString(timestampFormat);
                    }
                    if (timestamp != null)
                    {
                        textWriter.Write(timestamp);
                    }
                    if (logLevelString != null)
                    {
                        textWriter.Write(logLevelString);
                    }
                    CreateDefaultLogMessage(textWriter, logEntry, message, scopeProvider);
                }

                private void CreateDefaultLogMessage<TState>(TextWriter textWriter, in LogEntry<TState> logEntry, string message, IExternalScopeProvider scopeProvider)
                {
                    bool singleLine = FormatterOptions.SingleLine;
                    int eventId = logEntry.EventId.Id;
                    Exception exception = logEntry.Exception;

                    // Example:
                    // info: ConsoleApp.Program[10]
                    //       Request received

                    // category and event id
                    textWriter.Write(LoglevelPadding + logEntry.Category + '[' + eventId + "]");
                    if (!singleLine)
                    {
                        textWriter.Write(Environment.NewLine);
                    }

                    // scope information
                    WriteScopeInformation(textWriter, scopeProvider, singleLine);
                    WriteMessage(textWriter, message, singleLine);

                    // Example:
                    // System.InvalidOperationException
                    //    at Namespace.Class.Function() in File:line X
                    if (exception != null)
                    {
                        // exception message
                        WriteMessage(textWriter, exception.ToString(), singleLine);
                    }
                    if (singleLine)
                    {
                        textWriter.Write(Environment.NewLine);
                    }
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
                            textWriter.Write(_messagePadding);
                            WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message);
                            textWriter.Write(Environment.NewLine);
                        }
                    }

                    static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
                    {
                        string newMessage = message.Replace(oldValue, newValue);
                        writer.Write(newMessage);
                    }
                }

                private DateTimeOffset GetCurrentDateTime()
                {
                    return FormatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
                }

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

                private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
                {
                    bool disableColors = (FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled) ||
                        (FormatterOptions.ColorBehavior == LoggerColorBehavior.Default && System.Console.IsOutputRedirected);
                    if (disableColors)
                    {
                        return new ConsoleColors(null, null);
                    }
                    // We must explicitly set the background color if we are setting the foreground color,
                    // since just setting one can look bad on the users console.
                    return logLevel switch
                    {
                        LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
                        LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
                        LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
                        LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
                        LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
                        LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
                        _ => new ConsoleColors(null, null)
                    };
                }

                private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider scopeProvider, bool singleLine)
                {
                    if (FormatterOptions.IncludeScopes && scopeProvider != null)
                    {
                        bool paddingNeeded = !singleLine;
                        scopeProvider.ForEachScope((scope, state) =>
                        {
                            if (paddingNeeded)
                            {
                                paddingNeeded = false;
                                state.Write(_messagePadding + "=> ");
                            }
                            else
                            {
                                state.Write(" => ");
                            }
                            state.Write(scope);
                        }, textWriter);

                        if (!paddingNeeded && !singleLine)
                        {
                            textWriter.Write(Environment.NewLine);
                        }
                    }
                }

                private readonly struct ConsoleColors
                {
                    public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
                    {
                        Foreground = foreground;
                        Background = background;
                    }

                    public ConsoleColor? Foreground { get; }

                    public ConsoleColor? Background { get; }
                }
            }

            internal class ConsoleLogger : ILogger
            {
                private readonly string _name;

                internal ConsoleLogger(string name)
                {
                    if (name == null)
                    {
                        throw new ArgumentNullException(nameof(name));
                    }

                    _name = name;
                    Formatter = new SimpleConsoleFormatter(new SimpleConsoleFormatterOptions());
                    ScopeProvider = new LoggerExternalScopeProvider();
                }

                private ConsoleFormatter Formatter { get; set; }
                private IExternalScopeProvider ScopeProvider { get; set; }

                [ThreadStatic]
                private static StringWriter t_stringWriter;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (!IsEnabled(logLevel))
                    {
                        return;
                    }
                    if (formatter == null)
                    {
                        throw new ArgumentNullException(nameof(formatter));
                    }
                    t_stringWriter ??= new StringWriter();
                    LogEntry<TState> logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
                    Formatter.Write(in logEntry, ScopeProvider, t_stringWriter);

                    var sb = t_stringWriter.GetStringBuilder();
                    if (sb.Length == 0)
                    {
                        return;
                    }
                    string computedAnsiString = sb.ToString();
                    sb.Clear();
                    if (sb.Capacity > 1024)
                    {
                        sb.Capacity = 1024;
                    }
                    
                    Console.WriteLine(computedAnsiString);
                }

                public bool IsEnabled(LogLevel logLevel)
                {
                    return logLevel != LogLevel.None;
                }

                public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state);
            }

            public void Dispose()
            {
                
            }

            public ILogger CreateLogger(string categoryName)
                => new ConsoleLogger(categoryName);
        }
    }
}
