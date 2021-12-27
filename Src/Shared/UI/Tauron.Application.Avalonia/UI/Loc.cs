using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.AkkaHost;
using Tauron.Localization;

namespace Tauron.Application.Avalonia.UI
{
    [PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        private static readonly Dictionary<string, object?> Cache = new();

        public Loc(string entryName) => EntryName = entryName;

        public string EntryName { get; set; }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                lock (Cache)
                {
                    if (Cache.TryGetValue(EntryName, out var result))
                        return result!;
                }

                ActorApplication.ActorSystem.Loc().Request(
                    EntryName,
                    o =>
                    {
                        var res = o.GetOrElse(EntryName);
                        lock (Cache)
                        {
                            Cache[EntryName] = res;
                        }

                        UpdateValue(res);
                    });

                return "Loading";
            }
            catch (Exception e)
            {
                return $"Error... {e}";
            }
        }
    }
}