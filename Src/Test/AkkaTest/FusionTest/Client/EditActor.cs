using System;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaTest.FusionTest.Data;
using Spectre.Console;

namespace AkkaTest.FusionTest.Client
{
    public class EditActor : ReceiveActor
    {
        private readonly IActorRef     _self;
        private readonly IClaimManager _manager;

        public EditActor(IClaimManager manager)
        {
            _manager = manager;
            _self    = Self;
           
            Receive<Trigger>(RunEditor);
            
            AnsiConsole.Clear();
            Self.Tell(Trigger.Inst);
        }

        private void RunEditor(Trigger obj)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Render(new Rule("Editor"));
            AnsiConsole.Prompt(
                    new SelectionPrompt<(Action Action, string Display)>()
                       .UseConverter(p => p.Display)
                       .AddChoices(
                            (NewClaim, "Neuer Claim"),
                            (EditClaim, "Claim Editieren"),
                            (DeleteClaim, "Claim Löschen")))
               .Action();
        }

        private async void NewClaim()
        {
            try
            {
                var name         = AnsiConsole.Ask<string>("Name:");
                var info         = AnsiConsole.Ask<string>("Info:");
                
                AnsiConsole.WriteLine();
                var id = await AnsiConsole.Progress()
                            .StartAsync(async ctx =>
                                        {
                                            var task = ctx.AddTask("Kommando Übermitteln");
                                            try
                                            {
                                                return await _manager.AddClaim(new AddClaimCommand(name, info));
                                            }
                                            finally
                                            {
                                                task.Increment(100);
                                            }
                                        });

                if (id.Data == Guid.Empty)
                {
                    AnsiConsole.WriteLine("Übermittlung Fehlgeschlagen");
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            finally
            {
                _self.Tell(Trigger.Inst);
            }
        }

        private async void EditClaim()
        {
            try
            {
                var oldClaim = await FetchClaim();
                if (oldClaim == null)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteLine("Claim nicht Gefunden");
                    return;
                }

                var info = AnsiConsole.Ask<string>("Neue Info Eingeben:");
                if (info == "x")
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteLine("Editieren Abgebrochen");
                    return;
                }
                
                AnsiConsole.WriteLine();
                await AnsiConsole.Progress()
                   .StartAsync(
                        async ctx =>
                        {
                            var task = ctx.AddTask("Aktualisire Claim");
                            await _manager.UpdateClaim(new UpdateClaimCommand(oldClaim.Id, info));
                            task.Increment(100);
                        });
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            finally
            {
                _self.Tell(Trigger.Inst);
            }
        }
        
        private async void DeleteClaim()
        {
            try
            {
                var claim = await FetchClaim();
                if (claim == null)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteLine("Claim nicht gefunden");
                    return;
                }

                var isOk = AnsiConsole.Prompt(
                    new SelectionPrompt<bool>()
                       .UseConverter(
                            b => b switch
                            {
                                true => "Löschen",
                                _ => "Abbrechen"
                            })
                       .AddChoices(true, false));

                if (!isOk)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.WriteLine("Löschen Abbgebrochen");
                    return;
                }
                
                AnsiConsole.WriteLine();
                await AnsiConsole.Progress()
                   .StartAsync(
                        async ctx =>
                        {
                            var task = ctx.AddTask("Lösche Claim");
                            await _manager.RemoveClaim(new RemoveClaimCommand(claim.Id));
                            task.Increment(100);
                        });
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine("Löschen Erfolgreich");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            finally
            {
                _self.Tell(Trigger.Inst);
            }
        }

        private async Task<Claim?> FetchClaim()
        {
            AnsiConsole.WriteLine();
            var name = AnsiConsole.Ask<string>("Name des Claims zum beabeiten:");
            AnsiConsole.WriteLine();
            return await AnsiConsole.Progress()
                              .StartAsync(
                                   async ctx =>
                                   {
                                       var task = ctx.AddTask("Lade Id");
                                       var id   = await _manager.GetId(name);
                                       task.Increment(100);
                                       if(id.Data == Guid.Empty)
                                           return default;

                                       task = ctx.AddTask("Lade Claim");
                                       var claim = await _manager.Get(id);
                                       task.Increment(100);

                                       return claim;
                                   });
        }

        private sealed record Trigger
        {
            public static readonly Trigger Inst = new();
        }
    }
}