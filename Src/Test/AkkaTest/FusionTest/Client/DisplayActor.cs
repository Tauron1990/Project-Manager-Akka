using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.FusionTest.Data;
using Spectre.Console;
using Stl.Fusion;

namespace AkkaTest.FusionTest.Client
{
    public sealed class DisplayActor : ReceiveActor
    {
        private readonly CancellationTokenSource _cancellation = new();
        private readonly IClaimManager _manager;

        public DisplayActor(IClaimManager manager)
        {
            _manager = manager;
            RunDisplay();
            }

        private async void RunDisplay()
        {
            try
            {
                Table CreateTable()
                {
                    var table =new Table
                    {
                        Alignment = Justify.Center,
                        Border = TableBorder.HeavyHead,
                        Title = new TableTitle("Claims Display", new Style(Color.DarkOrange)),
                        BorderStyle = Style.Plain
                    };
                    table.AddColumn("Name");
                    table.AddColumn("Info");
                    table.AddColumn("Datum");
                    table.AddColumn("Id");

                    return table;
                }

                await AnsiConsole.Live(CreateTable())
                                 .AutoClear(false)
                                 .Overflow(VerticalOverflow.Ellipsis)
                                 .Cropping(VerticalOverflowCropping.Top)
                                 .StartAsync(
                                      async ctx =>
                                      {
                                          var manager = new TableManager(_manager, _cancellation.Token);
                                          await manager.Run(Refresh);
                                          
                                          void Refresh()
                                          {
                                              var table = CreateTable();

                                              foreach (var claim in manager.Claims.Values) 
                                                  table.AddRow(claim.Name, claim.Info, claim.CreationTime.ToString("g"), claim.Id.ToString("D"));

                                              ctx.UpdateTarget(table);
                                          }
                                              
                                      });
                
                _cancellation.Dispose();
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (!_cancellation.IsCancellationRequested)
                {
                    try
                    {
                        _cancellation.Cancel();
                    }
                    catch(Exception e2)
                    {
                        AnsiConsole.WriteException(e2);
                    }
                }
                AnsiConsole.WriteLine();
                AnsiConsole.Render(new Rule("Schwer Fehler"));
                AnsiConsole.WriteException(e);
            }
        }



        protected override void PostStop()
        {
            _cancellation.Cancel();
            base.PostStop();
        }

        private sealed class TableManager
        {
            private readonly IClaimManager _manager;
            private readonly CancellationToken _masterCancel;
            private readonly ConcurrentDictionary<Guid, (ComputedObserver<Claim> Observer, Task Task, CancellationTokenSource Cancel)> _entrys = new();
            
            private readonly object _refrehLock = new();
            private Action _refresh = () => {};

            public TableManager(IClaimManager manager, CancellationToken masterCancel)
            {
                _manager = manager;
                _masterCancel = masterCancel;
            }

            public ConcurrentDictionary<Guid, Claim> Claims { get; } = new();

            public async Task Run(Action refresh)
            {
                lock (_refrehLock)
                    _refresh = refresh;
                
                var computed = await Computed.Capture(_ => _manager.GetAll(), _masterCancel);
                
                while (!_masterCancel.IsCancellationRequested)
                {
                    await UpdateClaims(computed.ValueOrDefault ?? Array.Empty<Guid>());
                    lock(_refrehLock)
                        _refresh();
                    await computed.WhenInvalidated(_masterCancel);
                    if(_masterCancel.IsCancellationRequested) break;
                    computed = await computed.Update(_masterCancel);
                }
            }

            private async Task UpdateClaims(Guid[] ids)
            {
                var current = new List<Guid>(_entrys.Keys);

                foreach (var newId in ids)
                {
                    if (current.Contains(newId))
                    {
                        current.Remove(newId);
                    }
                    else
                    {
                        var computed = await Computed.Capture(_ => _manager.Get(newId), _masterCancel);
                        var observerCancel = CancellationTokenSource.CreateLinkedTokenSource(_masterCancel);
                        var observer = new ComputedObserver<Claim>(computed, observerCancel.Token, UpdateClaim);

                        _entrys[newId] = (observer, observer.Run(), observerCancel);
                    }

                    foreach (var guid in current)
                    {
                        if (!_entrys.TryRemove(guid, out var ele)) continue;

                        Claims.TryRemove(guid, out _);
                        
                        ele.Cancel.Cancel();
                        await ele.Task;
                        ele.Cancel.Dispose();
                    }
                }
            }

            private Task UpdateClaim(Claim? arg)
            {
                if(arg == null) return Task.CompletedTask;

                Claims[arg.Id] = arg;
                
                lock (_refresh)
                    _refresh();
                
                return Task.CompletedTask;
            }
        }
        
        private sealed class ComputedObserver<TType>
        {
            private IComputed<TType> _computed;
            private readonly CancellationToken _cancellationToken;
            private readonly Func<TType?, Task> _runner;

            public ComputedObserver(IComputed<TType> computed, CancellationToken cancellationToken, Func<TType?, Task> runner)
            {
                _computed = computed;
                _cancellationToken = cancellationToken;
                _runner = runner;
            }

            public async Task Run()
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    await _runner(_computed.ValueOrDefault);
                    await _computed.WhenInvalidated(_cancellationToken);
                    if(_cancellationToken.IsCancellationRequested) break;
                    _computed = await _computed.Update(_cancellationToken);
                }
            }
        }
    }
}